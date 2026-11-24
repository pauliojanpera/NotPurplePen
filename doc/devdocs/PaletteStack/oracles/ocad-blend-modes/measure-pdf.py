"""Read the structure of a PDF export of the blend-mode sample of ticket 027, and tell how it writes the palette colors:
the color of each shape, the blend modes and the opacities of the graphics states, the overprint flags, and the
transparency groups with their blending color spaces. It reads the objects of the PDF and not the image of a viewer,
so its result does not depend on the color conversion of a viewer.

Usage: measure-pdf.py CELLS.json EXPORT.pdf [--json OUT.json]

The script interprets the content stream of the page: the graphics states (gs), the colors (k, K, cs, scn and the
others), the paths and their painting, the marked content (BDC, EMC) and the form XObjects (Do), with the transparency
group of a form and the graphics state that is in effect at its Do. A transparency group starts with the blend mode
Normal and the opacity 1, so the blend mode and the opacity at the Do apply to the group as a whole (ISO 32000-1,
11.6.6).

The registration marks, the painted shapes on the layer "Registration mark", give the scale and the position of the
paper. Each painted shape is put in the cell of the cells file that holds it, and matched with the shape of the cell
whose rectangle it has.

For each region of a cell whose shapes are in DeviceCMYK, the script also gives the inks that the PDF model gives there
in a CMYK blending space: in each transparency group of the page, the shapes paint their inks over the backdrop, a
shape with the overprint flag and OPM 1 leaves the inks that it gives 0, and the group is composited onto the backdrop
with its blend mode and opacity. Darken takes the larger share of each ink, and Multiply the product of the
complements, as the PDF model blends a subtractive color space (ISO 32000-1, 11.3.5). It does not model a group that is
isolated or a knockout group, a soft mask, or a shape in a spot color space."""
import json
import math
import re
import sys
import zlib

WHITESPACE = b" \t\r\n\f\x00"
DELIMITERS = b"()<>[]{}/%"
NUMBER = re.compile(rb"[+-]?(?:\d+\.?\d*|\.\d+)$")
INKS = ("C", "M", "Y", "K")


class Name(str):
    """A PDF name, without its slash."""


class Ref(tuple):
    """A reference to an indirect object: (number, generation)."""


class Operator(str):
    """A keyword of a content stream."""


class Lexer:
    """The tokens of PDF syntax in a byte string."""

    def __init__(self, data, pos=0):
        self.data, self.pos = data, pos

    def skip(self):
        data, n = self.data, len(self.data)
        while self.pos < n:
            c = data[self.pos]
            if c in WHITESPACE:
                self.pos += 1
            elif c == 0x25:  # %
                while self.pos < n and data[self.pos] not in b"\r\n":
                    self.pos += 1
            else:
                break

    def token(self):
        """The next token: a delimiter as bytes, a Name, a byte string, a number, or a keyword as an Operator."""
        self.skip()
        data, n = self.data, len(self.data)
        if self.pos >= n:
            return None
        c = data[self.pos:self.pos + 1]
        if data.startswith(b"<<", self.pos) or data.startswith(b">>", self.pos):
            self.pos += 2
            return data[self.pos - 2:self.pos]
        if c in (b"[", b"]", b"{", b"}"):
            self.pos += 1
            return c
        if c == b"/":
            end = self.pos + 1
            while end < n and data[end] not in WHITESPACE and data[end] not in DELIMITERS:
                end += 1
            raw = data[self.pos + 1:end]
            self.pos = end
            return Name(re.sub(rb"#([0-9A-Fa-f]{2})", lambda m: bytes([int(m.group(1), 16)]), raw).decode("latin-1"))
        if c == b"(":
            return self.literal_string()
        if c == b"<":
            end = data.index(b">", self.pos)
            hexdigits = re.sub(rb"\s", b"", data[self.pos + 1:end])
            self.pos = end + 1
            if len(hexdigits) % 2:
                hexdigits += b"0"
            return bytes.fromhex(hexdigits.decode("ascii"))
        end = self.pos
        while end < n and data[end] not in WHITESPACE and data[end] not in DELIMITERS:
            end += 1
        if end == self.pos:
            raise ValueError("unexpected byte %r at %d" % (c, self.pos))
        raw = data[self.pos:end]
        self.pos = end
        if NUMBER.match(raw):
            return float(raw) if b"." in raw else int(raw)
        return Operator(raw.decode("latin-1"))

    def literal_string(self):
        data, depth, out = self.data, 0, bytearray()
        escapes = {ord("n"): b"\n", ord("r"): b"\r", ord("t"): b"\t", ord("b"): b"\b", ord("f"): b"\f"}
        while True:
            c = data[self.pos]
            self.pos += 1
            if c == 0x5C:  # backslash
                e = data[self.pos]
                self.pos += 1
                if e in escapes:
                    out += escapes[e]
                elif 0x30 <= e <= 0x37:
                    digits = bytes([e])
                    while len(digits) < 3 and 0x30 <= data[self.pos] <= 0x37:
                        digits += data[self.pos:self.pos + 1]
                        self.pos += 1
                    out.append(int(digits, 8) & 0xFF)
                elif e in b"\r\n":
                    if e == 0x0D and data[self.pos:self.pos + 1] == b"\n":
                        self.pos += 1
                else:
                    out.append(e)
            elif c == 0x28:
                if depth:
                    out.append(c)
                depth += 1
            elif c == 0x29:
                depth -= 1
                if depth == 0:
                    return bytes(out)
                out.append(c)
            else:
                out.append(c)

    def value(self, tok=None):
        """The next object: a dictionary, an array, a Name, a byte string, a number, a Ref, True, False or None."""
        if tok is None:
            tok = self.token()
        if tok == b"<<":
            result = {}
            while True:
                key = self.token()
                if key == b">>":
                    return result
                result[key] = self.value()
        if tok == b"[":
            result = []
            while True:
                t = self.token()
                if t == b"]":
                    return result
                result.append(self.value(t))
        if isinstance(tok, int):
            saved = self.pos
            generation, keyword = self.token(), self.token()
            if isinstance(generation, int) and keyword == "R":
                return Ref((tok, generation))
            self.pos = saved
            return tok
        if isinstance(tok, Operator):
            return {"true": True, "false": False, "null": None}.get(tok, tok)
        return tok


class Pdf:
    """The indirect objects of a PDF file, with their streams, and the objects of its object streams."""

    def __init__(self, path):
        self.data = data = open(path, "rb").read()
        self.version = data[:16].split(b"\n")[0].strip().decode("latin-1")
        self.objects, self.streams = {}, {}
        pos = 0
        header = re.compile(rb"(\d+)\s+(\d+)\s+obj\b")
        while True:
            m = header.search(data, pos)
            if not m:
                break
            lexer = Lexer(data, m.end())
            number = int(m.group(1))
            self.objects[number] = lexer.value()
            lexer.skip()
            pos = lexer.pos
            if data.startswith(b"stream", pos):
                start = pos + len(b"stream")
                start += 2 if data.startswith(b"\r\n", start) else 1
                end = data.index(b"endstream", start)
                self.streams[number] = (start, end)
                pos = end + len(b"endstream")
        for number, obj in list(self.objects.items()):
            if isinstance(obj, dict) and obj.get("Type") == "ObjStm":
                content = self.stream(number)
                lexer = Lexer(content)
                pairs = [lexer.value() for _ in range(2 * obj["N"])]
                for i in range(obj["N"]):
                    self.objects.setdefault(pairs[2 * i], Lexer(content, obj["First"] + pairs[2 * i + 1]).value())
        self.trailer = {}
        for m in re.finditer(rb"trailer\s*<<", data):
            self.trailer.update(Lexer(data, m.end() - 2).value())
        for number, obj in self.objects.items():
            if isinstance(obj, dict) and obj.get("Type") == "XRef":
                self.trailer.update({k: v for k, v in obj.items() if k in ("Root", "Info")})

    def get(self, value):
        """The value, with a reference resolved."""
        while isinstance(value, Ref):
            value = self.objects.get(value[0])
        return value

    def stream(self, number):
        """The decoded data of the stream of an object."""
        start, end = self.streams[number]
        info = self.objects[number]
        length = self.get(info.get("Length"))
        raw = self.data[start:start + length] if isinstance(length, int) else self.data[start:end].rstrip(b"\r\n")
        filters = self.get(info.get("Filter"))
        filters = filters if isinstance(filters, list) else [filters] if filters else []
        for f in filters:
            if f in ("FlateDecode", "Fl"):
                raw = zlib.decompress(raw)
            else:
                raise ValueError("the filter %s is not handled" % f)
        return raw

    def content(self, value):
        """The decoded content of a content stream, or of an array of them."""
        if isinstance(value, list):
            return b"\n".join(self.content(v) for v in value)
        return self.stream(value[0])


def multiply(a, b):
    """The product of two matrices [a b c d e f], the transformation a first."""
    return [a[0] * b[0] + a[1] * b[2], a[0] * b[1] + a[1] * b[3], a[2] * b[0] + a[3] * b[2],
            a[2] * b[1] + a[3] * b[3], a[4] * b[0] + a[5] * b[2] + b[4], a[4] * b[1] + a[5] * b[3] + b[5]]


def apply(m, x, y):
    return (m[0] * x + m[2] * y + m[4], m[1] * x + m[3] * y + m[5])


def short(value):
    """A short text of a value of a dictionary."""
    if isinstance(value, float):
        return ("%.4f" % value).rstrip("0").rstrip(".")
    if isinstance(value, bytes):
        return value.decode("latin-1")
    if isinstance(value, list):
        return "[" + " ".join(short(v) for v in value) + "]"
    if isinstance(value, Ref):
        return "%d %d R" % value
    return str(value)


class Interpreter:
    """Interprets the content of a page, and records each painted path with its state."""

    def __init__(self, pdf):
        self.pdf = pdf
        self.painted = []
        self.groups = []
        self.forms_seen = {}

    def color_space(self, name, resources):
        """A description of a color space: its family, its colorant names and its alternate space."""
        if name in ("DeviceCMYK", "DeviceRGB", "DeviceGray", "Pattern", "CMYK", "RGB", "G"):
            family = {"CMYK": "DeviceCMYK", "RGB": "DeviceRGB", "G": "DeviceGray"}.get(name, name)
            return {"family": family}
        spaces = self.pdf.get(resources.get("ColorSpace")) or {}
        cs = self.pdf.get(spaces.get(name, name))
        if isinstance(cs, Name):
            return self.color_space(cs, resources)
        family = cs[0]
        if family == "Separation":
            return {"family": "Separation", "colorants": [cs[1]], "alternate": short(self.pdf.get(cs[2]))}
        if family == "DeviceN":
            return {"family": "DeviceN", "colorants": list(self.pdf.get(cs[1])), "alternate": short(self.pdf.get(cs[2]))}
        if family == "ICCBased":
            info = self.pdf.get(cs[1])
            return {"family": "ICCBased", "components": info.get("N"), "alternate": short(info.get("Alternate"))}
        return {"family": short(family)}

    def describe_space(self, value, resources):
        if value is None:
            return None
        value = self.pdf.get(value)
        if isinstance(value, Name):
            return self.color_space(value, resources)
        if isinstance(value, list):
            return self.color_space(value[0], {}) if value and value[0] in ("DeviceCMYK", "DeviceRGB", "DeviceGray") \
                else {"family": short(value[0])}
        return {"family": short(value)}

    def run(self, page):
        resources = self.pdf.get(page.get("Resources")) or {}
        state = {
            "ctm": [1, 0, 0, 1, 0, 0], "fill": ({"family": "DeviceGray"}, [0]), "stroke": ({"family": "DeviceGray"}, [0]),
            "BM": "Normal", "ca": 1.0, "CA": 1.0, "OP": False, "op": False, "OPM": 0, "SMask": None, "width": 1.0,
        }
        self.interpret(self.pdf.content(page["Contents"]), resources, state, [], [])

    def interpret(self, content, resources, state, groups, layers):
        lexer = Lexer(content)
        operands, stack = [], []
        path, subpath = [], None
        layers = list(layers)
        while True:
            tok = lexer.token()
            if tok is None:
                break
            if not isinstance(tok, Operator) or tok in ("true", "false", "null"):
                operands.append(lexer.value(tok))
                continue
            op = str(tok)
            if op == "BI":
                end = re.compile(rb"\sEI(?=[\s]|$)").search(content, content.index(b"ID", lexer.pos) + 3)
                lexer.pos = end.end()
            elif op == "q":
                stack.append(dict(state))
            elif op == "Q":
                state = stack.pop()
            elif op == "cm":
                state["ctm"] = multiply(operands[-6:], state["ctm"])
            elif op == "w":
                state["width"] = operands[-1]
            elif op == "gs":
                gs = self.pdf.get((self.pdf.get(resources.get("ExtGState")) or {}).get(operands[-1])) or {}
                for key in ("BM", "ca", "CA", "OP", "op", "OPM", "SMask"):
                    if key in gs:
                        value = self.pdf.get(gs[key])
                        state[key] = value[0] if key == "BM" and isinstance(value, list) else value
                if "OP" in gs and "op" not in gs:
                    state["op"] = self.pdf.get(gs["OP"])
            elif op in ("k", "K"):
                state["fill" if op == "k" else "stroke"] = ({"family": "DeviceCMYK"}, list(operands[-4:]))
            elif op in ("rg", "RG"):
                state["fill" if op == "rg" else "stroke"] = ({"family": "DeviceRGB"}, list(operands[-3:]))
            elif op in ("g", "G"):
                state["fill" if op == "g" else "stroke"] = ({"family": "DeviceGray"}, list(operands[-1:]))
            elif op in ("cs", "CS"):
                state["fill" if op == "cs" else "stroke"] = (self.color_space(operands[-1], resources), [])
            elif op in ("sc", "scn", "SC", "SCN"):
                key = "fill" if op in ("sc", "scn") else "stroke"
                state[key] = (state[key][0], list(operands))
            elif op == "re":
                x, y, w, h = operands[-4:]
                corners = [apply(state["ctm"], px, py) for px, py in ((x, y), (x + w, y), (x + w, y + h), (x, y + h))]
                path.append(corners)
                subpath = None
            elif op == "m":
                subpath = [apply(state["ctm"], *operands[-2:])]
                path.append(subpath)
            elif op in ("l", "c", "v", "y"):
                points = [apply(state["ctm"], operands[i], operands[i + 1]) for i in range(0, len(operands), 2)]
                if subpath is None:
                    subpath = []
                    path.append(subpath)
                subpath.extend(points)
            elif op == "h":
                subpath = None
            elif op in ("f", "F", "f*", "S", "s", "B", "B*", "b", "b*"):
                if path:
                    self.painted.append({
                        "op": op, "subpaths": [list(sp) for sp in path if sp], "fill": state["fill"],
                        "stroke": state["stroke"], "BM": state["BM"], "ca": state["ca"], "CA": state["CA"],
                        "OP": state["OP"], "op_flag": state["op"], "OPM": state["OPM"], "SMask": state["SMask"],
                        "width": state["width"], "groups": list(groups), "layers": list(layers),
                    })
                path, subpath = [], None
            elif op == "n":
                path, subpath = [], None
            elif op in ("BDC", "BMC"):
                title = operands[-2] if op == "BDC" else operands[-1]
                if op == "BDC":
                    props = operands[-1]
                    if isinstance(props, Name):
                        props = self.pdf.get((self.pdf.get(resources.get("Properties")) or {}).get(props)) or {}
                    if isinstance(props, dict):
                        title = props.get("Title") or self.pdf.get(props.get("Name")) or title
                layers.append(short(self.pdf.get(title)))
            elif op == "EMC":
                if layers:
                    layers.pop()
            elif op == "Do":
                self.do(operands[-1], resources, state, groups, layers)
            operands = []

    def do(self, name, resources, state, groups, layers):
        ref = (self.pdf.get(resources.get("XObject")) or {}).get(name)
        xobject = self.pdf.get(ref)
        if not isinstance(xobject, dict) or xobject.get("Subtype") != "Form":
            return
        inner = dict(state)
        inner["ctm"] = multiply(self.pdf.get(xobject.get("Matrix")) or [1, 0, 0, 1, 0, 0], state["ctm"])
        inner_groups = list(groups)
        group = self.pdf.get(xobject.get("Group"))
        form_resources = self.pdf.get(xobject.get("Resources")) or resources
        if isinstance(group, dict) and group.get("S") == "Transparency":
            entry = {
                "index": len(self.groups), "xobject": name, "layer": layers[-1] if layers else None,
                "CS": self.describe_space(group.get("CS"), form_resources), "I": bool(self.pdf.get(group.get("I"))),
                "K": bool(self.pdf.get(group.get("K"))), "BM": state["BM"], "ca": state["ca"], "CA": state["CA"],
                "OP": state["OP"], "op": state["op"], "OPM": state["OPM"], "SMask": state["SMask"],
                "depth": len(groups),
            }
            self.groups.append(entry)
            inner_groups.append(entry["index"])
            inner.update({"BM": "Normal", "ca": 1.0, "CA": 1.0, "SMask": None})
        self.interpret(self.pdf.stream(ref[0]), form_resources, inner, inner_groups, layers)


def color_text(color):
    space, comps = color
    family = space["family"]
    if family == "DeviceCMYK":
        return "CMYK " + " ".join("%s%s" % (ink, short(round(100 * v, 1))) for ink, v in zip(INKS, comps))
    if family in ("Separation", "DeviceN"):
        return "%s %s %s (alternate %s)" % (family, ", ".join(space["colorants"]), " ".join(short(v) for v in comps),
                                            space["alternate"])
    return "%s %s" % (family, " ".join(short(v) for v in comps))


def space_text(space):
    if space is None:
        return "none"
    text = space["family"]
    if "colorants" in space:
        text += " " + ", ".join(space["colorants"])
    return text


def bbox(points):
    xs, ys = [p[0] for p in points], [p[1] for p in points]
    return (min(xs), min(ys), max(xs), max(ys))


def fit_marks(painted, marks_mm):
    """The scale and the offset of the page in pt from the paper in mm, on each axis, from the registration marks."""
    centres = []
    for p in painted:
        if any("Registration mark" in layer for layer in p["layers"]) and p["op"] in ("f", "F", "f*", "b", "B"):
            for sp in p["subpaths"]:
                x0, y0, x1, y1 = bbox(sp)
                centres.append(((x0 + x1) / 2, (y0 + y1) / 2))
    if len(centres) != 4:
        raise SystemExit("found %d shapes on the layer of the registration marks, and not 4" % len(centres))
    # Each mark of the cells file is the painted mark nearest to its place relative to the others.
    xs = sorted(c[0] for c in centres)
    ys = sorted(c[1] for c in centres)
    xmid, ymid = (xs[1] + xs[2]) / 2, (ys[1] + ys[2]) / 2
    found = {}
    for c in centres:
        found[(c[0] > xmid, c[1] > ymid)] = c
    ordered = [found[(False, True)], found[(True, True)], found[(False, False)], found[(True, False)]]
    fits = []
    for axis in (0, 1):
        pairs = [(m[axis], c[axis]) for m, c in zip(marks_mm, ordered)]
        mean_m = sum(p[0] for p in pairs) / 4
        mean_c = sum(p[1] for p in pairs) / 4
        scale = sum((p[0] - mean_m) * (p[1] - mean_c) for p in pairs) / sum((p[0] - mean_m) ** 2 for p in pairs)
        offset = mean_c - scale * mean_m
        residual = max(abs(scale * p[0] + offset - p[1]) / scale for p in pairs)
        fits.append((scale, offset, residual))
    return fits


def to_mm(fits, point):
    return tuple((point[axis] - fits[axis][1]) / fits[axis][0] for axis in (0, 1))


def inside(rect, x, y, tolerance=0.0):
    return rect[0] - tolerance <= x <= rect[2] + tolerance and rect[1] - tolerance <= y <= rect[3] + tolerance


def covers(shape, x, y):
    """Whether a painted shape covers a point in mm: by the bounding box of each subpath, with the nonzero rule as a
    union and the even-odd rule as a parity. The shapes of the sample are rectangles."""
    count = sum(1 for box in shape["boxes_mm"] if inside(box, x, y))
    return count % 2 == 1 if shape["op"] in ("f*", "B*", "b*") else count > 0


def blend(mode, backdrop, source):
    """A separable blend mode on one ink, in a subtractive color space: on the complements of the inks."""
    if mode == "Darken":
        return max(backdrop, source)
    if mode == "Multiply":
        return 1 - (1 - backdrop) * (1 - source)
    if mode == "Lighten":
        return min(backdrop, source)
    if mode == "Screen":
        return backdrop * source
    if mode in ("Normal", "Compatible"):
        return source
    return None


def model_inks(shapes, groups, x, y):
    """The inks at a point, in percent, by the PDF model in a CMYK blending space, or the reason it is not modelled."""
    backdrop = [0.0] * 4
    by_group = {}
    for shape in shapes:
        if covers(shape, x, y) and shape["op"] in ("f", "F", "f*", "b", "B", "b*", "B*"):
            by_group.setdefault(shape["groups"][0] if shape["groups"] else None, []).append(shape)
    order = sorted(by_group, key=lambda g: -1 if g is None else g)
    for g in order:
        members = by_group[g]
        group = groups[g] if g is not None else None
        if group and (group["I"] or group["K"] or group["SMask"] or len(members[0]["groups"]) > 1):
            return None, "an isolated, knockout, soft-masked or nested group"
        content = list(backdrop)
        for shape in members:
            space, comps = shape["fill"]
            if space["family"] != "DeviceCMYK" or shape["SMask"]:
                return None, "a shape in %s" % space["family"]
            for i in range(4):
                if shape["op_flag"] and shape["OPM"] == 1 and comps[i] == 0:
                    continue
                b = blend(shape["BM"], content[i], comps[i])
                if b is None:
                    return None, "the blend mode %s" % shape["BM"]
                content[i] = (1 - shape["ca"]) * content[i] + shape["ca"] * b
        mode, alpha = (group["BM"], group["ca"]) if group else ("Normal", 1.0)
        for i in range(4):
            b = blend(mode, backdrop[i], content[i])
            if b is None:
                return None, "the blend mode %s" % mode
            backdrop[i] = (1 - alpha) * backdrop[i] + alpha * b
    return [100 * v for v in backdrop], None


def ink_text(inks):
    return " ".join("%s%s" % (ink, short(round(v, 1))) for ink, v in zip(INKS, inks))


def main():
    args = sys.argv[1:]
    out_json = None
    if "--json" in args:
        i = args.index("--json")
        out_json = args[i + 1]
        del args[i:i + 2]
    if len(args) != 2:
        raise SystemExit(__doc__)
    cells = json.load(open(args[0]))
    pdf = Pdf(args[1])
    catalog = pdf.get(pdf.trailer.get("Root"))
    pages = pdf.get(catalog["Pages"])
    kids = pdf.get(pages["Kids"])
    page = pdf.get(kids[0])
    interpreter = Interpreter(pdf)
    interpreter.run(page)
    painted, groups = interpreter.painted, interpreter.groups

    page_group = pdf.get(page.get("Group"))
    print("%s: %s, %d page(s), MediaBox %s" % (args[1], pdf.version, pages.get("Count"), short(pdf.get(page.get("MediaBox")))))
    if isinstance(page_group, dict):
        print("Page group: S %s, CS %s, I %s, K %s" % (short(page_group.get("S")),
              space_text(interpreter.describe_space(page_group.get("CS"), pdf.get(page.get("Resources")) or {})),
              short(page_group.get("I", False)), short(page_group.get("K", False))))
    else:
        print("Page group: none. The page has no blending color space of its own; the viewer chooses it.")
    intents = pdf.get(catalog.get("OutputIntents"))
    print("Output intents: %s" % ("none" if not intents else
          "; ".join(short(pdf.get(pdf.get(i).get("OutputConditionIdentifier"))) for i in intents)))

    fits = fit_marks(painted, cells["marks"])
    print("Scale from the registration marks: %.5f pt/mm across, %.5f pt/mm up; the marks fit within %.4f mm"
          % (fits[0][0], fits[1][0], max(f[2] for f in fits)))
    for shape in painted:
        shape["boxes_mm"] = []
        for sp in shape["subpaths"]:
            x0, y0, x1, y1 = bbox(sp)
            (a, b), (c, d) = to_mm(fits, (x0, y0)), to_mm(fits, (x1, y1))
            shape["boxes_mm"].append((min(a, c), min(b, d), max(a, c), max(b, d)))
        boxes = shape["boxes_mm"]
        shape["bbox_mm"] = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))

    print()
    print("The transparency groups, in the order of the page (blend mode, opacities and overprint at the Do):")
    for group in groups:
        members = [s for s in painted if group["index"] in s["groups"]]
        colors = sorted({color_text(s["fill"]) for s in members if s["op"] not in ("S", "s")})
        print("  %2d %-28s %-12s CS %-10s I %-5s K %-5s | BM %-8s ca %-4s CA %-4s OP %-5s op %-5s OPM %s | %d shapes, %s"
              % (group["index"], (group["layer"] or "")[:28], group["xobject"], space_text(group["CS"]), group["I"],
                 group["K"], group["BM"], short(group["ca"]), short(group["CA"]), group["OP"], group["op"],
                 group["OPM"], len(members), "; ".join(colors) or "-"))

    used = set()
    print()
    for cell in cells["cells"]:
        rect = cell["rect"]
        mine = [s for s in painted if inside(rect, (s["bbox_mm"][0] + s["bbox_mm"][2]) / 2, (s["bbox_mm"][1] + s["bbox_mm"][3]) / 2, 0.5)
                and not any("Registration mark" in layer for layer in s["layers"])]
        used.update(id(s) for s in mine)
        print("%s (case %d, row %d, column %d)" % (cell["id"], cell["case"], cell["row"], cell["column"]))
        for s in mine:
            box = s["bbox_mm"]
            shapes = cell["shapes"]
            match = [i for i, sh in enumerate(shapes) if all(abs(a - b) < 0.1 for a, b in zip(sh["rect"], box))]
            parts = [i for i, sh in enumerate(shapes) if inside(box, *sh["rect"][:2], 0.1) and inside(box, *sh["rect"][2:], 0.1)]
            if match:
                label = "%s, shape %d" % (shapes[match[0]]["symbol"], match[0] + 1)
            elif parts:
                label = "%s, shapes %s" % (shapes[parts[0]]["symbol"], " and ".join(str(i + 1) for i in parts))
            else:
                label = "no shape of the cell"
            group = groups[s["groups"][0]] if s["groups"] else None
            print("  %-40s %-3s %d subpath(s) | layer %s | %s | inside: BM %s ca %s OP %s op %s OPM %s | group %s: %s, ca %s"
                  % (label, s["op"], len(s["subpaths"]), " / ".join(s["layers"][-1:]),
                     color_text(s["fill"]) if s["op"] not in ("S", "s") else "stroke " + color_text(s["stroke"]),
                     s["BM"], short(s["ca"]), s["OP"], s["op_flag"], s["OPM"],
                     group["layer"] if group else "none", group["BM"] if group else "-", short(group["ca"]) if group else "-"))
        for region in cell["regions"]:
            inks, reason = model_inks(mine, groups, *region["point"])
            print("    %-46s %s" % (region["name"], ink_text(inks) if inks else "not modelled: " + reason))
    rest = [s for s in painted if id(s) not in used and not any("Registration mark" in layer for layer in s["layers"])]
    if rest:
        print()
        print("Painted shapes outside the cells:")
        for s in rest:
            print("  %s %s %s" % (s["op"], " / ".join(s["layers"]), short([round(v, 2) for v in s["bbox_mm"]])))

    if out_json:
        for s in painted:
            s["fill"] = [s["fill"][0], s["fill"][1]]
            s["stroke"] = [s["stroke"][0], s["stroke"][1]]
        json.dump({"groups": groups, "painted": painted, "fits": fits}, open(out_json, "w"), indent=1, default=short)


if __name__ == "__main__":
    main()

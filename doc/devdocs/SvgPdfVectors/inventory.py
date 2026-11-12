#!/usr/bin/env python3
"""inventory.py [--json <out>] <pdf>...

Lists what the first page of each PDF holds that an SVG of it would have to keep: its producer and page boxes; the
painting operators, by kind; the colours, by colour space (DeviceCMYK, DeviceRGB, DeviceGray, ICCBased, Separation
with each colourant's name, DeviceN, Indexed, Pattern); the graphics states that overprint (OP, op, OPM), blend (BM),
set opacity (ca, CA) or mask (SMask); the fonts, with their kind and whether their program is embedded; the images,
with their filter, colour space, size and bits; shadings, patterns, form XObjects, inline images and marked content.
Form XObjects, tiling patterns and Type 3 glyphs are followed into, each once.

Purple Pen converts and copies page 0 alone (PdfMapFile, CoursePdf), so only the first page is read.
Needs pikepdf (pip install pikepdf). Run with python3 -I, as the PDFs are data.
"""

import collections
import json
import sys

import pikepdf

PAINT_OPS = {
    "f": "fill", "F": "fill", "f*": "fill", "S": "stroke", "s": "stroke", "B": "fill+stroke", "B*": "fill+stroke",
    "b": "fill+stroke", "b*": "fill+stroke", "n": "no-paint", "W": "clip", "W*": "clip",
    "Tj": "text", "TJ": "text", "'": "text", '"': "text", "Do": "xobject", "sh": "shading", "BI": "inline-image",
    "INLINE IMAGE": "inline-image", "BDC": "marked-content", "BMC": "marked-content", "gs": "graphics-state",
    "d0": "type3-glyph", "d1": "type3-glyph",
}


def name_of(obj):
    """The name of a PDF name object, or the string of anything else."""
    return str(obj) if obj is not None else None


def describe_colour_space(cs, resources):
    """A short description of a colour space: its family, and for Separation and DeviceN its colourants and
    alternate space, for ICCBased its number of components, for Indexed its base."""
    if isinstance(cs, pikepdf.Name):
        n = str(cs)
        if n in ("/DeviceCMYK", "/DeviceRGB", "/DeviceGray", "/Pattern", "/CalRGB", "/CalGray", "/Lab"):
            return n[1:]
        spaces = resources.get("/ColorSpace") if resources is not None else None
        if spaces is not None and n in spaces:
            return describe_colour_space(spaces[n], resources)
        return n[1:]
    if isinstance(cs, pikepdf.Array) and len(cs) > 0:
        family = str(cs[0])[1:]
        if family == "ICCBased":
            stream = cs[1]
            return "ICCBased(N=%s)" % stream.get("/N")
        if family == "Separation":
            return "Separation(%s -> %s)" % (str(cs[1])[1:], describe_colour_space(cs[2], resources))
        if family == "DeviceN":
            names = ",".join(str(x)[1:] for x in cs[1])
            return "DeviceN(%s -> %s)" % (names, describe_colour_space(cs[2], resources))
        if family == "Indexed":
            return "Indexed(%s)" % describe_colour_space(cs[1], resources)
        if family == "Pattern":
            return "Pattern" + ("(%s)" % describe_colour_space(cs[1], resources) if len(cs) > 1 else "")
        return family
    return repr(cs)


class Inventory:
    """What one page holds, counted as its content streams are read."""

    def __init__(self):
        self.ops = collections.Counter()
        self.colours = collections.Counter()
        self.gstates = collections.Counter()
        self.fonts = {}
        self.images = []
        self.shadings = collections.Counter()
        self.patterns = collections.Counter()
        self.forms = 0
        self.inline_images = 0
        self.marked = collections.Counter()
        self.seen = set()
        self.paths = 0
        self.segments = 0

    def read(self, stream_owner, resources, depth=0):
        """Read one content stream with its resources, following its form XObjects, patterns and Type 3 glyphs."""
        key = None
        try:
            key = stream_owner.objgen
        except Exception:
            pass
        if key is not None and key != (0, 0):
            if key in self.seen:
                return
            self.seen.add(key)
        if resources is None:
            resources = pikepdf.Dictionary()
        fill_space = "DeviceGray"
        stroke_space = "DeviceGray"
        try:
            instructions = pikepdf.parse_content_stream(stream_owner)
        except Exception as e:
            self.ops["unreadable:" + type(e).__name__] += 1
            return
        for instruction in instructions:
            if isinstance(instruction, pikepdf.ContentStreamInlineImage):
                self.inline_images += 1
                self.ops["inline-image"] += 1
                continue
            operands, operator = instruction.operands, str(instruction.operator)
            if operator in PAINT_OPS:
                self.ops[PAINT_OPS[operator]] += 1
            if operator in ("m",):
                self.segments += 1
            elif operator in ("l", "c", "v", "y", "re"):
                self.segments += 1
            if operator in ("f", "F", "f*", "S", "s", "B", "B*", "b", "b*"):
                self.paths += 1
            if operator == "k":
                self.colours["DeviceCMYK fill"] += 1
            elif operator == "K":
                self.colours["DeviceCMYK stroke"] += 1
            elif operator == "rg":
                self.colours["DeviceRGB fill"] += 1
            elif operator == "RG":
                self.colours["DeviceRGB stroke"] += 1
            elif operator == "g":
                self.colours["DeviceGray fill"] += 1
            elif operator == "G":
                self.colours["DeviceGray stroke"] += 1
            elif operator == "cs":
                fill_space = describe_colour_space(operands[0], resources)
            elif operator == "CS":
                stroke_space = describe_colour_space(operands[0], resources)
            elif operator in ("sc", "scn"):
                self.colours[fill_space + " fill"] += 1
                if fill_space.startswith("Pattern") and operands and isinstance(operands[-1], pikepdf.Name):
                    self.read_pattern(operands[-1], resources, depth)
            elif operator in ("SC", "SCN"):
                self.colours[stroke_space + " stroke"] += 1
                if stroke_space.startswith("Pattern") and operands and isinstance(operands[-1], pikepdf.Name):
                    self.read_pattern(operands[-1], resources, depth)
            elif operator == "gs":
                self.read_gstate(operands[0], resources)
            elif operator in ("Tf",):
                self.read_font(operands[0], resources, depth)
            elif operator == "Do":
                self.read_xobject(operands[0], resources, depth)
            elif operator == "sh":
                shadings = resources.get("/Shading")
                if shadings is not None and operands[0] in shadings:
                    self.shadings["ShadingType %s" % shadings[operands[0]].get("/ShadingType")] += 1
            elif operator in ("BDC", "BMC"):
                self.marked[str(operands[0])[1:]] += 1

    def read_gstate(self, name, resources):
        """Count what a graphics state sets that an SVG would have to keep."""
        states = resources.get("/ExtGState")
        if states is None or name not in states:
            return
        state = states[name]
        for key in ("/OP", "/op", "/OPM", "/BM", "/ca", "/CA", "/SMask", "/AIS", "/TK"):
            if key in state:
                value = state[key]
                if key == "/SMask" and isinstance(value, pikepdf.Dictionary):
                    value = "dict:" + name_of(value.get("/S"))
                if key == "/BM" and isinstance(value, pikepdf.Array):
                    value = ",".join(str(v) for v in value)
                self.gstates["%s=%s" % (key[1:], str(value))] += 1

    def read_font(self, name, resources, depth):
        """Record a font: its subtype, base font, encoding, and the kind of its embedded program."""
        fonts = resources.get("/Font")
        if fonts is None or name not in fonts:
            return
        font = fonts[name]
        try:
            key = font.objgen
        except Exception:
            key = (id(font), 0)
        if key in self.fonts:
            return
        subtype = name_of(font.get("/Subtype"))
        base = name_of(font.get("/BaseFont"))
        descriptor = font.get("/FontDescriptor")
        if subtype == "/Type0":
            descendant = font["/DescendantFonts"][0]
            subtype = "/Type0/" + name_of(descendant.get("/Subtype"))[1:]
            descriptor = descendant.get("/FontDescriptor")
        program = "not embedded"
        if descriptor is not None:
            if "/FontFile" in descriptor:
                program = "FontFile (Type 1)"
            elif "/FontFile2" in descriptor:
                program = "FontFile2 (TrueType)"
            elif "/FontFile3" in descriptor:
                program = "FontFile3 (%s)" % name_of(descriptor["/FontFile3"].get("/Subtype"))
        if subtype == "/Type3":
            program = "Type 3 glyph procedures"
            glyphs = font.get("/CharProcs")
            if glyphs is not None:
                for glyph in glyphs.keys():
                    self.read(glyphs[glyph], font.get("/Resources", resources), depth + 1)
        self.fonts[key] = {"subtype": subtype, "base": base, "encoding": name_of(font.get("/Encoding"))
                           if not isinstance(font.get("/Encoding"), pikepdf.Dictionary) else "dict",
                           "program": program, "to_unicode": "/ToUnicode" in font}

    def read_xobject(self, name, resources, depth):
        """Follow a form XObject, or record an image XObject."""
        xobjects = resources.get("/XObject")
        if xobjects is None or name not in xobjects:
            return
        xobject = xobjects[name]
        subtype = name_of(xobject.get("/Subtype"))
        if subtype == "/Form":
            self.forms += 1
            if "/Group" in xobject:
                group = xobject["/Group"]
                self.gstates["transparency group (isolated=%s, knockout=%s)" % (group.get("/I"), group.get("/K"))] += 1
            self.read(xobject, xobject.get("/Resources", resources), depth + 1)
        elif subtype == "/Image":
            try:
                key = xobject.objgen
            except Exception:
                key = None
            filters = xobject.get("/Filter")
            if isinstance(filters, pikepdf.Array):
                filters = ",".join(str(f)[1:] for f in filters)
            else:
                filters = name_of(filters)[1:] if filters is not None else "none"
            cs = xobject.get("/ColorSpace")
            self.images.append({
                "key": list(key) if key else None,
                "width": int(xobject.get("/Width", 0)), "height": int(xobject.get("/Height", 0)),
                "bits": int(xobject.get("/BitsPerComponent", 0)) if "/BitsPerComponent" in xobject else None,
                "filter": filters,
                "colour_space": describe_colour_space(cs, resources) if cs is not None else
                ("mask" if xobject.get("/ImageMask") else "none"),
                "smask": "/SMask" in xobject, "bytes": len(xobject.read_raw_bytes()),
            })

    def read_pattern(self, name, resources, depth):
        """Record a pattern, and follow a tiling pattern's content."""
        patterns = resources.get("/Pattern")
        if patterns is None or name not in patterns:
            return
        pattern = patterns[name]
        kind = int(pattern.get("/PatternType", 0))
        self.patterns["tiling" if kind == 1 else "shading"] += 1
        if kind == 1:
            self.read(pattern, pattern.get("/Resources", resources), depth + 1)

    def summary(self):
        """The inventory as plain data."""
        unique_images = {}
        for image in self.images:
            unique_images[tuple(image["key"]) if image["key"] else id(image)] = image
        return {
            "operators": dict(self.ops.most_common()), "paths": self.paths, "path_segments": self.segments,
            "colours": dict(self.colours.most_common()), "graphics_states": dict(self.gstates.most_common()),
            "fonts": sorted(self.fonts.values(), key=lambda f: (str(f["base"]), str(f["subtype"]))),
            "images": sorted(unique_images.values(), key=lambda i: -i["bytes"]),
            "image_draws": len(self.images),
            "shadings": dict(self.shadings), "patterns": dict(self.patterns), "form_xobject_uses": self.forms,
            "inline_images": self.inline_images, "marked_content": dict(self.marked),
        }


def inventory(path):
    """Inventory the first page of a PDF."""
    with pikepdf.open(path) as pdf:
        info = pdf.docinfo
        page = pdf.pages[0]
        boxes = {}
        for box in ("/MediaBox", "/CropBox", "/TrimBox", "/BleedBox", "/ArtBox"):
            if box in page.obj:
                boxes[box[1:]] = [round(float(v), 3) for v in page.obj[box]]
        result = {
            "file": path, "pages": len(pdf.pages), "version": pdf.pdf_version,
            "creator": str(info.get("/Creator", "")), "producer": str(info.get("/Producer", "")),
            "rotate": int(page.obj.get("/Rotate", 0)), "boxes": boxes,
            "annotations": len(page.obj.get("/Annots", [])),
            "optional_content": "/OCProperties" in pdf.Root,
            "output_intents": len(pdf.Root.get("/OutputIntents", [])),
            "page_group": str(page.obj["/Group"].get("/CS")) if "/Group" in page.obj else None,
        }
        counter = Inventory()
        counter.read(page, page.obj.get("/Resources"))
        result.update(counter.summary())
        return result


def main(argv):
    """Inventory each PDF named, print a summary of each, and write all as JSON if asked."""
    out = None
    if len(argv) > 2 and argv[1] == "--json":
        out = argv[2]
        argv = argv[:1] + argv[3:]
    results = []
    for path in argv[1:]:
        try:
            r = inventory(path)
        except Exception as e:
            r = {"file": path, "error": "%s: %s" % (type(e).__name__, e)}
        results.append(r)
        print("==", path)
        if "error" in r:
            print("  error:", r["error"])
            continue
        print("  %s / %s, PDF %s, %d page(s), rotate %d, boxes %s, annots %d, OC %s, intents %d, page group %s" % (
            r["creator"], r["producer"], r["version"], r["pages"], r["rotate"], r["boxes"], r["annotations"],
            r["optional_content"], r["output_intents"], r["page_group"]))
        print("  operators:", r["operators"])
        print("  paths: %d, segments: %d" % (r["paths"], r["path_segments"]))
        print("  colours:", r["colours"])
        print("  graphics states:", r["graphics_states"])
        print("  fonts (%d):" % len(r["fonts"]), "; ".join("%s %s %s%s" % (f["base"], f["subtype"], f["program"],
              "" if f["to_unicode"] else " no ToUnicode") for f in r["fonts"]))
        print("  images (%d distinct, %d draws):" % (len(r["images"]), r["image_draws"]),
              "; ".join("%dx%d %s %s %sb%s %d B" % (i["width"], i["height"], i["filter"], i["colour_space"], i["bits"],
                                                   " smask" if i["smask"] else "", i["bytes"]) for i in r["images"][:6]))
        print("  shadings:", r["shadings"], "patterns:", r["patterns"], "forms:", r["form_xobject_uses"],
              "inline images:", r["inline_images"], "marked content:", r["marked_content"])
    if out:
        with open(out, "w") as f:
            json.dump(results, f, indent=1, sort_keys=True)


if __name__ == "__main__":
    main(sys.argv)

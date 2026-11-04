# Writes variants of one exported SVG that declare its colours in other forms, for scribus_colours.py,
# inkscape_colours.py and render_chromium.py to read, and lists the size of each.
#   python3 -I make_variants.py <export.svg> <out folder> <CMYK profile.icc> <small CMYK profile.icc>
# The first profile is linked or embedded whole; Purple Pen's is src/PurplePen/USWebCoatedSWOP.icc. The second is
# embedded in the candidates; make_profile.py writes one. The export writes each style as one class rule a line in its
# <style> sheet, and each colour twice, as sRGB and as the same sRGB followed by icc-color(cmyk,...), naming
# <color-profile name="cmyk"/>; that is variant as-now.
import base64
import gzip
import os
import re
import shutil
import sys
import urllib.parse

PROFILE_NOW = '<color-profile name="cmyk"/>'

# SVG 1.1 presentation attributes among the properties the export's classes set; mix-blend-mode is not one.
PRESENTATION_ATTRIBUTES = {"fill", "fill-rule", "fill-opacity", "stroke", "stroke-width", "stroke-miterlimit",
                           "stroke-linejoin", "stroke-linecap", "stroke-opacity", "stroke-dasharray",
                           "stroke-dashoffset", "font-size", "font-family", "font-weight", "font-style",
                           "clip-rule", "opacity", "image-rendering", "shape-rendering"}


# A document split into what comes before its class rules, the rules by class name, each a list of (property, value),
# and what comes after them.
def split_document(text):
    rules = {}
    lines = text.split("\n")
    head, tail, order = [], [], []
    inStyle = False
    for line in lines:
        match = re.fullmatch(r"\.([A-Za-z0-9_-]+)\{([^}]*)\}", line) if inStyle else None
        if match:
            rules[match.group(1)] = [tuple(declaration.split(":", 1)) for declaration in match.group(2).split(";")]
            order.append(match.group(1))
            continue
        if line == "<style>":
            inStyle = True
        if line == "</style>":
            inStyle = False
        (tail if order else head).append(line)
    return "\n".join(head), rules, order, "\n".join(tail)


# The document again from its parts; elements whose class lost all its rules lose the class attribute.
def join_document(head, rules, order, tail):
    ruleLines = [".%s{%s}" % (name, ";".join(p + ":" + v for p, v in rules[name])) for name in order if rules[name]]
    if not ruleLines:
        head = head.rstrip("\n")
    return head + "\n" + "\n".join(ruleLines) + ("\n" if ruleLines else "") + tail


# Whether a declaration paints with a colour, as against none or a pattern.
def is_colour_paint(property, value):
    return property in ("fill", "stroke") and value.startswith("#")


# The declarations with each paint written once, as its last declaration: the one with icc-color() under the CMYK
# colour model.
def last_paints(declarations):
    result = []
    for property, value in declarations:
        if is_colour_paint(property, value):
            result = [(p, v) for p, v in result if p != property]
        result.append((property, value))
    return result


def escape(value):
    return value.replace("&", "&amp;").replace('"', "&quot;").replace("<", "&lt;")


# Rewrite every class attribute of the body through the function, which gets the class name and returns the
# attributes to write in its place.
def rewrite_classes(text, function):
    return re.sub(r' class="([^"]+)"', lambda match: function(match.group(1)), text)


# Styles moved from the class rules to the elements: the declarations a test picks go into each element's style
# attribute, or into presentation attributes, and leave the class unless keepClass says so.
def move_declarations(text, picks, asAttributes, keepClass=False):
    head, rules, order, tail = split_document(text)
    moved = {}
    for name in order:
        moved[name] = [d for d in rules[name] if picks(*d)]
        if not keepClass:
            rules[name] = [d for d in rules[name] if not picks(*d)]

    def attributes(name):
        declarations = moved.get(name, [])
        written = ' class="%s"' % name if rules.get(name) else ""
        if not declarations:
            return written
        if asAttributes:
            return written + "".join(' %s="%s"' % (p, escape(v)) for p, v in last_paints(declarations))
        return written + ' style="%s"' % escape(";".join(p + ":" + v for p, v in declarations))

    return join_document(head, rules, order, rewrite_classes(tail, attributes))


# The class rules without the sRGB paint that each icc-color() paint repeats.
def without_fallback(text):
    head, rules, order, tail = split_document(text)
    for name in order:
        rules[name] = last_paints(rules[name])
    return join_document(head, rules, order, tail)


# A colour profile element in place of the export's.
def with_profile(text, element):
    if PROFILE_NOW not in text:
        raise ValueError("the export declares no profile as " + PROFILE_NOW)
    return text.replace(PROFILE_NOW, element)


def data_uri(path):
    return "data:application/vnd.iccprofile;base64," + base64.b64encode(open(path, "rb").read()).decode("ascii")


# The form Inkscape 1.2 writes for a colour of a linked profile: the profile named after its description, with
# characters other than letters, digits, "_", ":", "-" and "." made "-", linked by a file URI and identified by its
# path (DocumentProperties::linkSelectedProfile); each colour one declaration, "#rrggbb icc-color(name, c, m, y, k)"
# (SPColor::toString), in the element's style attribute.
def inkscape_form(text, profilePath, description):
    name = re.sub(r"[^A-Za-z0-9_:.-]", "-", description)
    if not re.match(r"[A-Za-z_:]", name):
        name = "_" + name
    path = os.path.abspath(profilePath)
    element = '<color-profile name="%s" xlink:href="%s" id="%s"/>' % (
        name, "file://" + urllib.parse.quote(path), escape(path))

    def inkscape_numbers(value):
        def numbers(match):
            values = [repr(float(v)).rstrip("0").rstrip(".") if float(v) else "0" for v in match.group(1).split(",")]
            return "icc-color(%s, %s)" % (name, ", ".join(("0" + v) if v.startswith(".") else v for v in values))
        return re.sub(r"icc-color\(cmyk,([^)]*)\)", numbers, value)

    head, rules, order, tail = split_document(text)
    for name_ in order:
        rules[name_] = [(p, inkscape_numbers(v)) for p, v in last_paints(rules[name_])]
    text = join_document(head, rules, order, tail)
    text = move_declarations(text, is_colour_paint, asAttributes=False)
    return with_profile(text, element)


def main():
    exportPath, outFolder, swopPath, smallPath = sys.argv[1:5]
    os.makedirs(outFolder, exist_ok=True)
    text = open(exportPath, encoding="utf-8").read()
    shutil.copy(swopPath, os.path.join(outFolder, os.path.basename(swopPath)))
    linked = '<color-profile id="cmyk" name="cmyk" xlink:href="%s"/>' % os.path.basename(swopPath)
    small = '<color-profile id="cmyk" name="cmyk" xlink:href="%s"/>' % data_uri(smallPath)
    anything = lambda property, value: True
    variants = {
        "as-now": text,
        "style-paint": move_declarations(text, is_colour_paint, asAttributes=False),
        "style-all": move_declarations(text, anything, asAttributes=False),
        "attr-paint": move_declarations(text, is_colour_paint, asAttributes=True),
        "attr-all": move_declarations(text, lambda p, v: p in PRESENTATION_ATTRIBUTES, asAttributes=True),
        "class-and-attr": move_declarations(text, lambda p, v: p in PRESENTATION_ATTRIBUTES, asAttributes=True,
                                            keepClass=True),
        "no-fallback": without_fallback(text),
        "profile-id": with_profile(text, '<color-profile id="cmyk" name="cmyk"/>'),
        "profile-linked-no-id": with_profile(text, '<color-profile name="cmyk" xlink:href="%s"/>'
                                             % os.path.basename(swopPath)),
        "profile-linked": with_profile(text, linked),
        "profile-embedded": with_profile(text, '<color-profile id="cmyk" name="cmyk" xlink:href="%s"/>'
                                         % data_uri(swopPath)),
        "profile-small": with_profile(text, small),
        "inkscape-form": inkscape_form(text, os.path.join(outFolder, os.path.basename(swopPath)),
                                       "U.S. Web Coated (SWOP) v2"),
        "style-all-profile-small": with_profile(move_declarations(text, anything, asAttributes=False), small),
        "class-and-attr-profile-small": with_profile(
            move_declarations(text, lambda p, v: p in PRESENTATION_ATTRIBUTES, asAttributes=True, keepClass=True),
            small),
    }
    base = len(text.encode("utf-8"))
    baseZipped = len(gzip.compress(text.encode("utf-8"), 9, mtime=0))
    print("%-30s %10s %8s %10s %8s" % ("variant", "bytes", "change", "gzip -9", "change"))
    for name, variant in variants.items():
        data = variant.encode("utf-8")
        with open(os.path.join(outFolder, name + ".svg"), "wb") as out:
            out.write(data)
        zipped = len(gzip.compress(data, 9, mtime=0))
        print("%-30s %10d %+7.1f%% %10d %+7.1f%%" % (name, len(data), 100.0 * (len(data) - base) / base,
                                                   zipped, 100.0 * (zipped - baseZipped) / baseZipped))


main()

# Counts the paints that reach the drawn elements of an export written with style attributes, as a reader that
# applies style attributes, their inheritance and <use> sees them: per element its fill and stroke, each as CMYK
# percentages, "none", or "?" and the value for a paint with no icc-color(). Pages show parts through <use>, so each
# use counts. Scribus should make as many items, filled and stroked alike, as summarise_scribus.py then shows.
#   python3 -I expected_paints.py <export.svg>
import collections, re, sys
import xml.etree.ElementTree as ET
SVG = "{http://www.w3.org/2000/svg}"; XLINK = "{http://www.w3.org/1999/xlink}"
root = ET.parse(sys.argv[1]).getroot()
ids = {e.get("id"): e for e in root.iter() if e.get("id")}
def paint(value):
    if value == "none":
        return "none"
    m = re.search(r"icc-color\(cmyk,([^)]*)\)", value)
    if m:
        return "/".join("%g" % round(float(v) * 100, 2) for v in m.group(1).split(","))
    return "?" + value
counts = collections.Counter()
def visit(element, inherited, inDefs):
    tag = element.tag.replace(SVG, "")
    if tag in ("defs", "metadata", "style", "clipPath", "pattern", "color-profile", "view") or tag.startswith("{"):
        return
    style = dict(inherited)
    for declaration in element.get("style", "").split(";"):
        if ":" in declaration:
            name, value = declaration.split(":", 1)
            if name in ("fill", "stroke"):
                style[name] = paint(value)
    if tag == "use":
        target = ids.get(element.get(XLINK + "href", "")[1:])
        if target is not None and target.tag != SVG + "image":
            visit(target, style, False)
            return
        counts[("image",)] += 1
        return
    if tag in ("path", "circle", "ellipse", "text"):
        counts[(tag if tag == "text" else "shape", "fill " + style.get("fill", "black"), "stroke " + style.get("stroke", "none"))] += 1
        return
    for child in element:
        visit(child, style, inDefs)
visit(root, {}, False)
total = sum(counts.values())
print("%d drawn elements" % total)
summary = collections.Counter()
for key, n in counts.items():
    if key[0] == "image":
        summary["image"] += n; continue
    summary[key[1]] += n; summary[key[2]] += n
for key, n in sorted(summary.items(), key=lambda kv: -kv[1]):
    print("%7d %s" % (n, key))

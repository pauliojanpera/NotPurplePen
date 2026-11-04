# Picks probes for inkscape_colours.py from an export written with style attributes. The reader names an object by
# the first element whose data-o is its number, and o<n>^ by that element's group, so only first elements count, and
# no group is probed twice. For each distinct CMYK paint, the first such element or group that carries it is probed;
# with --unset, also the first element whose style sits on its group, which Inkscape shows as unset.
#   python3 -I pick_probes.py <export.svg> <count> [--unset]
import re, sys
import xml.etree.ElementTree as ET
count = int(sys.argv[2])
root = ET.parse(sys.argv[1]).getroot()
parents = {child: parent for parent in root.iter() for child in parent}
firsts, seenNumbers = [], set()
for element in root.iter():
    number = element.get("data-o")
    if number is not None and number not in seenNumbers:
        seenNumbers.add(number)
        firsts.append((number, element))
seen, groups, probes, unset = set(), set(), [], None
for number, element in firsts:
    parent = parents.get(element)
    for holder, suffix in ((element, ""), (parent, "^")):
        if holder is None or (suffix and id(holder) in groups):
            continue
        style = holder.get("style", "")
        for prop in ("fill", "stroke"):
            m = re.search(prop + r":#[0-9a-f]+ icc-color\(cmyk,([^)]*)\)", style)
            if m and (prop, m.group(1)) not in seen and len(probes) < count:
                seen.add((prop, m.group(1)))
                if suffix:
                    groups.add(id(holder))
                values = "/".join("%g" % float(v) for v in m.group(1).split(","))
                probes.append(("o%s%s:%s" % (number, suffix, prop), values))
    if unset is None and "icc-color" not in element.get("style", "") and parent is not None and "icc-color" in parent.get("style", ""):
        prop = "fill" if re.search(r"fill:#", parent.get("style")) else "stroke"
        unset = ("o%s:%s" % (number, prop), "unset")
if "--unset" in sys.argv and unset:
    probes.insert(0, unset)
print(" ".join(p for p, _ in probes))
print("# " + " ".join("%s=%s" % p for p in probes))

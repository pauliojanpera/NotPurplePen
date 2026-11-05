# Picks probes for inkscape_colours.py from an export written with style attributes. The reader names an object by
# the first element whose data-o is its number, o<n>^ by that element's group, o<n>^^ by the group around that and so
# on, so only first elements count, and no group is probed twice. For each distinct CMYK paint, the first such element
# or group that carries it is probed; with --unset, also the first element whose style sits on a group around it, which
# Inkscape shows as unset.
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
    holders, holder = [(element, "")], parent
    while holder is not None and holder.tag.endswith("}g"):
        holders.append((holder, "^" * len(holders)))
        holder = parents.get(holder)
    for holder, suffix in holders:
        if suffix and id(holder) in groups:
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
    carrier = next((h for h, suffix in holders[1:] if "icc-color" in h.get("style", "")), None)
    if unset is None and "icc-color" not in element.get("style", "") and carrier is not None:
        prop = "fill" if re.search(r"fill:#", carrier.get("style")) else "stroke"
        unset = ("o%s:%s" % (number, prop), "unset")
if "--unset" in sys.argv and unset:
    probes.insert(0, unset)
print(" ".join(p for p, _ in probes))
print("# " + " ".join("%s=%s" % p for p in probes))

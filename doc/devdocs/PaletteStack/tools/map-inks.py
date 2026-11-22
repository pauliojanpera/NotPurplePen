"""Print the inks of a map file and the ink components of each of its palette entries.

Usage: python3 -I map-inks.py <map file>...

Reads OCAD 6 to 8 (the symbol header), OCAD 9 and later (string types 9 and 10) and Mapper (.omap, .xmap), and
prints them in one form, so that a file and the file that an export made of it can be compared with diff:

    ink <name> cmyk=<c>,<m>,<y>,<k> screen=<lines per inch>@<degrees>|none
    color <number> <name> cmyk=<c>,<m>,<y>,<k> overprint|knockout|- <composition>

The palette entries are in the order of the file: the top entry first. The composition is "process" for an entry
without a spot definition, "ink <name>" for a Mapper entry that is a spot ink as such, and "[<ink> <factor>, ...]"
for an entry composed of spot inks. A factor is between 0 and 1. A missing component is not printed, and a factor 0
is printed as 0. The flag is the file's own: OCAD's overprint flag, Mapper's knockout flag for an entry with a spot
definition, and "-" for a Mapper entry without one, which Mapper draws as a knockout.
"""
import struct
import sys
import xml.etree.ElementTree as ElementTree


def number(value):
    return "%g" % round(value, 4)


def cmyk(c, m, y, k):
    return "cmyk=" + ",".join(number(v) for v in (c, m, y, k))


def screen(frequency, angle):
    return "screen=" + ("%s@%s" % (number(frequency), number(angle)) if frequency > 0 else "none")


def composition(components):
    return "[" + ", ".join("%s %s" % (name, number(factor)) for name, factor in components) + "]"


def text(raw):
    try:
        return raw.decode("utf-8")
    except UnicodeDecodeError:
        return raw.decode("cp1252")


def delphi_string(data, offset, size):
    return text(data[offset + 1: offset + 1 + min(data[offset], size)])


def print_ocad8(data):
    count, separation_count = struct.unpack_from("<hh", data, 48)
    separation_count = min(separation_count, 24)
    process = struct.unpack_from("<8h", data, 52)
    print("process screens " + ", ".join("%s %s" % (ink, screen(process[2 * i] / 10, process[2 * i + 1] / 10))
                                          for i, ink in enumerate("CMYK")))
    separations = 72 + 256 * 72
    names = []
    for i in range(separation_count):
        offset = separations + i * 24
        names.append(delphi_string(data, offset, 15))
        frequency, angle = struct.unpack_from("<hh", data, offset + 20)
        print("ink", names[-1], cmyk(*(v / 200 for v in data[offset + 16: offset + 20])),
              screen(frequency / 10, angle / 10))
    for i in range(count):
        offset = 72 + i * 72
        color_number, overprint = struct.unpack_from("<hh", data, offset)
        shares = data[offset + 40: offset + 40 + separation_count]
        components = [(names[j], share / 200) for j, share in enumerate(shares) if share <= 200]
        print("color", color_number, delphi_string(data, offset + 8, 31),
              cmyk(*(v / 200 for v in data[offset + 4: offset + 8])),
              "overprint" if overprint else "knockout", composition(components) if components else "process")


def ocad_strings(data, wanted):
    block = struct.unpack_from("<i", data, 32)[0]
    while block:
        for i in range(256):
            position, length, string_type, _ = struct.unpack_from("<iiii", data, block + 4 + i * 16)
            if string_type in wanted:
                yield string_type, text(data[position: position + length].split(b"\0")[0])
        block = struct.unpack_from("<i", data, block)[0]


def fields(string):
    first, *rest = string.split("\t")
    return first, [(field[0], field[1:]) for field in rest if field]


def value(pairs, code, default):
    return next((float(v) for c, v in pairs if c == code and v), default)


def print_ocad9(data):
    inks = []
    colors = []
    for string_type, string in ocad_strings(data, {9, 10}):
        (inks if string_type == 10 else colors).append(fields(string))
    for name, pairs in sorted(inks, key=lambda ink: value(ink[1], "n", 0)):
        print("ink", name, cmyk(*(value(pairs, code, 0) / 100 for code in "cmyk")),
              screen(value(pairs, "f", 0) / 10, value(pairs, "a", 0)))
    for name, pairs in colors:
        components = []
        spot_name = None
        for code, field in pairs:
            if code == "s":
                spot_name = field
            elif code == "p" and spot_name is not None:
                components.append((spot_name, float(field) / 100))
                spot_name = None
        print("color", int(value(pairs, "n", 0)), name, cmyk(*(value(pairs, code, 0) / 100 for code in "cmyk")),
              "overprint" if value(pairs, "o", 0) else "knockout",
              composition(components) if components else "process")


def print_mapper(path):
    root = ElementTree.parse(path).getroot()
    namespace = root.tag[:root.tag.index("}") + 1] if root.tag.startswith("{") else ""
    colors = root.find(namespace + "colors").findall(namespace + "color")
    spot_names = {}
    for color in colors:
        named = color.find("%sspotcolors/%snamedcolor" % (namespace, namespace))
        if named is not None:
            spot_names[color.get("priority")] = named.text or ""
            print("ink", named.text or "", cmyk(*(float(color.get(code)) for code in "cmyk")),
                  screen(float(named.get("screen_frequency", 0)), float(named.get("screen_angle", 0))))
    for color in sorted(colors, key=lambda color: int(color.get("priority"))):
        spot = color.find(namespace + "spotcolors")
        described = "process"
        flag = "-"
        if spot is not None:
            named = spot.find(namespace + "namedcolor")
            components = [(spot_names.get(component.get("spotcolor"), "?" + component.get("spotcolor")),
                           float(component.get("factor")))
                          for component in spot.findall(namespace + "component")]
            if components:
                described = composition(components)
            elif named is not None:
                described = "ink " + (named.text or "")
            if components or named is not None:
                flag = "knockout" if spot.get("knockout") == "true" else "overprint"
        print("color", color.get("priority"), color.get("name"), cmyk(*(float(color.get(code)) for code in "cmyk")),
              flag, described)


def main():
    for path in sys.argv[1:]:
        print("#", path)
        with open(path, "rb") as file:
            data = file.read()
        if data[:2] == b"\xad\x0c":
            (print_ocad8 if struct.unpack_from("<h", data, 4)[0] <= 8 else print_ocad9)(data)
        else:
            print_mapper(path)


if __name__ == "__main__":
    sys.exit(main())

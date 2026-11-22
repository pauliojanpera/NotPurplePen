"""Tell how the parameter strings of OCAD files encode the characters outside ASCII, and which program wrote them.

Usage: python3 -I ocad-string-encodings.py [--summary] <OCAD file or folder>...

For each OCAD 9 or later file, prints the version, the program that probably wrote the file, and, for each
parameter string type whose strings hold a byte of 0x80 or more, whether the strings decode as UTF-8 or only as an
8-bit code page, with an example of each. A string of 8-bit text decodes as UTF-8 only by chance, so a file whose
strings all decode as UTF-8 is UTF-8. Folders are searched for .ocd files. With --summary, prints only the count of
files of each version, writer and encoding.

The writer comes from the strings that the file holds, and the program that saved the file last is the writer:
- OCAD writes string types that neither Mapper nor Purple Pen writes, and from version 12 on a blend mode (b) in
  colors.
- Mapper writes only types 8, 9, 10, 11, 1030, 1039 and 1061, a spot color as name, n, v, and a transparency (t) in
  each color.
- Purple Pen writes only types 8, 9, 10, 11, 21, 22, 27, 28, 1026, 1030 and 1039, and its print parameters (1026)
  begin with the codes a and r. With the opacity of palette entries (feature/opacity-and-blend-mode), it writes a
  transparency (t) in each color, and from version 12 on a blend mode (b), as OCAD does. Before, it wrote neither.
"""
import collections
import os
import struct
import sys

MAPPER_TYPES = {8, 9, 10, 11, 1030, 1039, 1061}
PURPLE_PEN_TYPES = {8, 9, 10, 11, 21, 22, 27, 28, 1026, 1030, 1039}
PRINT_PARAMETERS = 1026


def ocad_files(paths):
    for path in paths:
        if os.path.isdir(path):
            for folder, _, files in os.walk(path):
                for name in sorted(files):
                    if name.lower().endswith(".ocd"):
                        yield os.path.join(folder, name)
        else:
            yield path


def parameter_strings(data):
    block = struct.unpack_from("<i", data, 32)[0]
    visited = set()
    while 0 < block < len(data) and block not in visited:
        visited.add(block)
        for i in range(256):
            offset = block + 4 + i * 16
            if offset + 16 > len(data):
                break
            position, length, string_type, _ = struct.unpack_from("<iiii", data, offset)
            if string_type > 0 and 0 < position and position + length <= len(data):
                yield string_type, data[position: position + length].split(b"\0")[0]
        block = struct.unpack_from("<i", data, block)[0]


def codes(raw):
    return [field[:1] for field in raw.split(b"\t")[1:] if field]


def guess_writer(strings):
    types = set(string_type for string_type, _ in strings)
    color_codes = [codes(raw) for string_type, raw in strings if string_type == 9]
    spot_codes = [codes(raw) for string_type, raw in strings if string_type == 10]
    print_codes = [codes(raw) for string_type, raw in strings if string_type == PRINT_PARAMETERS]
    if types - (MAPPER_TYPES | PURPLE_PEN_TYPES):
        return "OCAD"
    if any(found[:2] == [b"a", b"r"] for found in print_codes):
        return "Purple Pen"
    if any(b"b" in found for found in color_codes):
        return "OCAD"
    if any(b"n" in found and b"v" in found and found.index(b"n") < found.index(b"v") for found in spot_codes):
        return "Mapper"
    if color_codes and all(b"t" in found for found in color_codes) and types <= MAPPER_TYPES:
        return "Mapper"
    return "unknown writer"


def main():
    arguments = sys.argv[1:]
    summary = "--summary" in arguments
    arguments = [argument for argument in arguments if argument != "--summary"]
    tally = collections.Counter()

    for path in ocad_files(arguments):
        try:
            with open(path, "rb") as file:
                data = file.read()
        except OSError as error:
            print("%s: %s" % (path, error))
            continue
        if data[:2] != b"\xad\x0c" or len(data) < 64:
            continue
        version = struct.unpack_from("<h", data, 4)[0]
        if version < 9:
            tally[(version, "-", "no parameter strings")] += 1
            if not summary:
                print("%s: OCAD %d, no parameter strings to tell" % (path, version))
            continue

        strings = list(parameter_strings(data))
        writer = guess_writer(strings)
        found = collections.defaultdict(collections.Counter)
        examples = {}
        for string_type, raw in strings:
            if not any(byte >= 0x80 for byte in raw):
                continue
            try:
                raw.decode("utf-8")
                kind = "UTF-8"
            except UnicodeDecodeError:
                kind = "8-bit"
            found[string_type][kind] += 1
            if (string_type, kind) not in examples:
                first = next(i for i, byte in enumerate(raw) if byte >= 0x80)
                examples[(string_type, kind)] = raw[max(0, first - 16): first + 16]

        kinds = " and ".join(sorted(set(kind for counter in found.values() for kind in counter))) or "ASCII only"
        tally[(version, writer, kinds)] += 1
        if summary:
            continue
        print("%s: OCAD %d, %s, %s" % (path, version, writer, kinds))
        for string_type in sorted(found):
            for kind, count in sorted(found[string_type].items()):
                print("    type %d: %d %s, for example %r" % (string_type, count, kind, examples[(string_type, kind)]))

    if summary:
        for (version, writer, kinds), count in sorted(tally.items()):
            print("%4d  OCAD %d, %s, %s" % (count, version, writer, kinds))


if __name__ == "__main__":
    sys.exit(main())

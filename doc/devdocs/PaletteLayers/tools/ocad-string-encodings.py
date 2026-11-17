"""Tell how the parameter strings of OCAD files encode the characters outside ASCII.

Usage: python3 -I ocad-string-encodings.py <OCAD file or folder>...

For each OCAD 9 or later file, prints the version and, for each parameter string type whose strings hold a byte
of 0x80 or more, whether the strings decode as UTF-8 or only as an 8-bit code page, with an example of each. A
string of 8-bit text decodes as UTF-8 only by chance, so a file whose strings all decode as UTF-8 is UTF-8.
Folders are searched for .ocd files. Files with only ASCII strings are listed as such.
"""
import collections
import os
import struct
import sys


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


def main():
    for path in ocad_files(sys.argv[1:]):
        with open(path, "rb") as file:
            data = file.read()
        if data[:2] != b"\xad\x0c" or len(data) < 64:
            continue
        version = struct.unpack_from("<h", data, 4)[0]
        if version < 9:
            print("%s: OCAD %d, no parameter strings to tell" % (path, version))
            continue

        found = collections.defaultdict(collections.Counter)
        examples = {}
        for string_type, raw in parameter_strings(data):
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

        if not found:
            print("%s: OCAD %d, ASCII only" % (path, version))
            continue
        kinds = set(kind for counter in found.values() for kind in counter)
        print("%s: OCAD %d, %s" % (path, version, " and ".join(sorted(kinds))))
        for string_type in sorted(found):
            for kind, count in sorted(found[string_type].items()):
                print("    type %d: %d %s, for example %r" % (string_type, count, kind, examples[(string_type, kind)]))


if __name__ == "__main__":
    sys.exit(main())

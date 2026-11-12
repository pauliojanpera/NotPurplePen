"""List the colours of OCAD files, version 9 and later, that are not fully opaque.

Usage: python3 -I ocad-colour-opacity.py <file.ocd>...

OCAD keeps each colour as a string of the string index, of record type 9: its name, then parameters, among them `t`,
the colour's opacity in percent, which OCAD 12 writes as `t100` for an opaque colour and OpenOrienteering Mapper reads
as the opacity 0.01 * t. Prints each file with its count of colours and of those whose `t` is set below 100, and each
such colour with its `t` and its overprint flag `o`.
"""
import struct
import sys


def strings(data):
    """The record type and text of each string of an OCAD file's string index."""
    version = struct.unpack_from("<h", data, 4)[0]
    block = struct.unpack_from("<i", data, 32)[0]
    seen = set()
    while block and block not in seen and block < len(data):
        seen.add(block)
        next_block = struct.unpack_from("<i", data, block)[0]
        for i in range(256):
            pos, length, rec_type, obj = struct.unpack_from("<iiii", data, block + 4 + 16 * i)
            if pos <= 0 or length <= 0 or pos + length > len(data):
                continue
            raw = data[pos:pos + length].split(b"\0", 1)[0]
            text = raw.decode("utf-8" if version >= 11 else "latin-1", errors="replace")
            yield rec_type, text
        block = next_block


def main():
    """List the colours below full opacity of each file named."""
    for path in sys.argv[1:]:
        with open(path, "rb") as f:
            data = f.read()
        if struct.unpack_from("<h", data, 0)[0] != 0x0CAD or struct.unpack_from("<h", data, 4)[0] < 9:
            continue
        version = struct.unpack_from("<h", data, 4)[0]
        colors = []
        for rec_type, text in strings(data):
            if rec_type != 9:
                continue
            fields = text.split("\t")
            params = {field[0]: field[1:] for field in fields[1:] if field}
            colors.append((fields[0], params))
        transparent = [(name, p) for name, p in colors if p.get("t", "100") not in ("", "100")]
        print("%s\tversion %d\t%d colours\t%d with t below 100" % (path, version, len(colors), len(transparent)))
        for name, p in transparent:
            print("\t%s\tt=%s\to=%s" % (name, p.get("t"), p.get("o")))


if __name__ == "__main__":
    main()

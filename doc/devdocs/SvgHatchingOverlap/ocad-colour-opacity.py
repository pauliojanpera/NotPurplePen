"""List the colour strings of OCAD files (version 9 and later) whose transparency parameter is set.

Usage: python3 -I ocad_colors.py <file.ocd>...
"""
import struct
import sys


def strings(data):
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

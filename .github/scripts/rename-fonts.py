# Copies every TrueType face it finds into one directory, rewriting the family
# name so that all of them answer to a single name. A private font collection
# can then be asked for that name and given each file in turn, which is the only
# way to put an arbitrary font through the same drawing path the map uses.
#
# Usage: rename-fonts.py <target family> <output dir> <input dir> [input dir ...]

import os
import struct
import sys


def checksum(data):
    if len(data) % 4:
        data += b'\0' * (4 - len(data) % 4)
    total = 0
    for i in range(0, len(data), 4):
        total = (total + struct.unpack_from('>I', data, i)[0]) & 0xFFFFFFFF
    return total


def rename(src, dst, new_name):
    d = bytearray(open(src, 'rb').read())
    if d[:4] == b'ttcf' or len(d) < 12:
        return False
    tag = bytes(d[:4])
    if tag not in (b'\x00\x01\x00\x00', b'true', b'OTTO'):
        return False

    num = struct.unpack_from('>H', d, 4)[0]
    dirs = {}
    for i in range(num):
        rec = 12 + i * 16
        if rec + 16 > len(d):
            return False
        name = bytes(d[rec:rec+4]).decode('latin-1')
        csum, off, ln = struct.unpack_from('>III', d, rec + 4)
        dirs[name] = (rec, off, ln)
    if 'name' not in dirs or 'glyf' not in dirs:
        return False

    rec, off, ln = dirs['name']
    fmt, count, str_off = struct.unpack_from('>3H', d, off)
    records = []
    for i in range(count):
        r = off + 6 + i * 12
        plat, enc, lang, nid, rl, ro = struct.unpack_from('>6H', d, r)
        value = bytes(d[off + str_off + ro: off + str_off + ro + rl])
        if nid in (1, 4, 16):
            value = new_name.encode('utf-16-be') if plat == 3 else new_name.encode('latin-1')
        records.append((plat, enc, lang, nid, value))

    pool = bytearray()
    placed = []
    for plat, enc, lang, nid, value in records:
        placed.append((len(pool), len(value)))
        pool += value
    header = 6 + 12 * len(records)
    table = bytearray(struct.pack('>3H', 0, len(records), header))
    for (plat, enc, lang, nid, value), (o, l) in zip(records, placed):
        table += struct.pack('>6H', plat, enc, lang, nid, l, o)
    table += pool
    length = len(table)
    while len(table) % 4:
        table += b'\0'

    out = bytearray(d)
    while len(out) % 4:
        out += b'\0'
    new_off = len(out)
    out += table
    struct.pack_into('>III', out, rec + 4, checksum(bytes(table)), new_off, length)

    if 'head' in dirs:
        head = dirs['head'][1]
        struct.pack_into('>I', out, head + 8, 0)
        struct.pack_into('>I', out, head + 8, (0xB1B0AFBA - checksum(bytes(out))) & 0xFFFFFFFF)

    open(dst, 'wb').write(bytes(out))
    return True


def main():
    family, outdir = sys.argv[1], sys.argv[2]
    os.makedirs(outdir, exist_ok=True)
    written = skipped = 0
    seen = set()
    for root in sys.argv[3:]:
        for dirpath, dirnames, filenames in os.walk(root):
            for fn in sorted(filenames):
                if not fn.lower().endswith(('.ttf', '.otf')):
                    continue
                key = fn.lower()
                if key in seen:
                    continue
                seen.add(key)
                src = os.path.join(dirpath, fn)
                dst = os.path.join(outdir, os.path.splitext(fn)[0] + '.ttf')
                try:
                    if rename(src, dst, family):
                        written += 1
                    else:
                        skipped += 1
                except Exception:
                    skipped += 1
    print(f'renamed {written}, skipped {skipped}, into {outdir}')


main()

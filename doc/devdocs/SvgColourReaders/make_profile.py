# Writes a small CMYK output profile, ICC version 2.1, whose A2B0 table converts CMYK to the PCS (CIELAB, D50).
#   python3 -I make_profile.py <swopsamples.dat | naive> <grid points> <out.icc>
# From swopsamples.dat, the table PurplePenCore embeds for SwopColorConverter (src/PurplePenCore/Resources), the
# profile converts as that converter does, by its own multilinear interpolation at <grid points> per channel, so that a
# reader converting the profile's CMYK to sRGB comes near the sRGB the export writes beside it. "naive" converts as
# device CMYK, R = (1 - C)(1 - K) and so on, which a grid of 2 holds exactly. The profile has no B2A0 table: a reader
# can show and convert CMYK values with it, but not convert other colours to CMYK.
import struct
import sys


# The CMYK-to-sRGB table of SwopColorConverter: (samples per channel, RGB bytes with K varying fastest).
# path: swopsamples.dat.
def read_swop_table(path):
    data = open(path, "rb").read()
    if data[:7] != b"CMYKRGB":
        raise ValueError("not a CMYK-to-RGB lookup table: " + path)
    version, samples, count = struct.unpack_from("<iiq", data, 7)
    if count != samples ** 4 or len(data) != 23 + 3 * count:
        raise ValueError("unexpected table size in " + path)
    return samples, data[23:]


# sRGB, each channel 0 to 1, as SwopColorConverter's lookup table gives it, interpolated as LookupTableColorConverter
# interpolates, without the converter's special cases for paper white and full black.
def swop_rgb(table, cmyk):
    samples, values = table
    lowers, fractions = [], []
    for channel in cmyk:
        position = channel * (samples - 1)
        lower = min(int(position), samples - 2)
        lowers.append(lower)
        fractions.append(position - lower)
    rgb = [0.0, 0.0, 0.0]
    for corner in range(16):
        weight = 1.0
        index = 0
        for axis in range(4):
            bit = (corner >> (3 - axis)) & 1
            weight *= fractions[axis] if bit else 1.0 - fractions[axis]
            index = index * samples + lowers[axis] + bit
        for channel in range(3):
            rgb[channel] += values[3 * index + channel] * weight
    return [value / 255.0 for value in rgb]


# sRGB of device CMYK, each channel 0 to 1.
def naive_rgb(cmyk):
    c, m, y, k = cmyk
    return [(1 - c) * (1 - k), (1 - m) * (1 - k), (1 - y) * (1 - k)]


# CIELAB under D50 of an sRGB colour, through the D50-adapted sRGB matrix that ICC sRGB profiles and Little CMS use.
def srgb_to_lab(rgb):
    linear = [value / 12.92 if value <= 0.04045 else ((value + 0.055) / 1.055) ** 2.4 for value in rgb]
    x = 0.4360747 * linear[0] + 0.3850649 * linear[1] + 0.1430804 * linear[2]
    y = 0.2225045 * linear[0] + 0.7168786 * linear[1] + 0.0606169 * linear[2]
    z = 0.0139322 * linear[0] + 0.0971045 * linear[1] + 0.7141733 * linear[2]

    def f(t):
        return t ** (1.0 / 3.0) if t > 216.0 / 24389.0 else (24389.0 / 27.0 * t + 16.0) / 116.0

    fx, fy, fz = f(x / 0.9642), f(y / 1.0), f(z / 0.8249)
    return 116.0 * fy - 16.0, 500.0 * (fx - fy), 200.0 * (fy - fz)


# A CIELAB colour in the 16-bit encoding of version 2 lut16 tables: L 0 to 100 as 0 to 0xFF00, a and b -128 to 127 as
# 0 to 0xFF00.
def encode_lab16(lab):
    lightness, a, b = lab
    clamp = lambda value: max(0, min(0xFFFF, int(round(value))))
    return clamp(lightness * 652.8), clamp((a + 128.0) * 256.0), clamp((b + 128.0) * 256.0)


def s15fixed16(value):
    return struct.pack(">i", int(round(value * 65536)))


def text_description(text):
    ascii = text.encode("ascii") + b"\0"
    return b"desc" + bytes(4) + struct.pack(">I", len(ascii)) + ascii + bytes(8) + bytes(3) + bytes(67)


def text(text):
    return b"text" + bytes(4) + text.encode("ascii") + b"\0"


def xyz(values):
    return b"XYZ " + bytes(4) + b"".join(s15fixed16(value) for value in values)


# A lut16Type table from CMYK to CIELAB: identity input and output curves of two entries, and a grid of the given
# number of points per channel, C varying slowest and K fastest.
def lut16_cmyk_to_lab(grid, convert):
    table = b"mft2" + bytes(4) + bytes([4, 3, grid, 0])
    table += b"".join(s15fixed16(value) for value in (1, 0, 0, 0, 1, 0, 0, 0, 1))
    table += struct.pack(">HH", 2, 2) + struct.pack(">HH", 0, 0xFFFF) * 4
    for index in range(grid ** 4):
        cmyk = [((index // grid ** (3 - axis)) % grid) / (grid - 1) for axis in range(4)]
        table += struct.pack(">HHH", *encode_lab16(srgb_to_lab(convert(cmyk))))
    return table + struct.pack(">HH", 0, 0xFFFF) * 3


# The bytes of the profile.
def profile(description, grid, convert):
    tags = [(b"desc", text_description(description)),
            (b"cprt", text("No copyright, use freely")),
            (b"wtpt", xyz((0.9642, 1.0, 0.8249))),
            (b"A2B0", lut16_cmyk_to_lab(grid, convert))]
    offset = 128 + 4 + 12 * len(tags)
    directory, body = b"", b""
    for signature, data in tags:
        directory += signature + struct.pack(">II", offset + len(body), len(data))
        body += data + bytes(-len(data) % 4)
    size = offset + len(body)
    header = struct.pack(">I", size) + bytes(4) + struct.pack(">I", 0x02100000) + b"prtrCMYKLab "
    header += struct.pack(">6H", 2026, 11, 4, 0, 0, 0) + b"acsp" + bytes(4 + 4 + 4 + 4 + 8 + 4)
    header += s15fixed16(0.9642) + s15fixed16(1.0) + s15fixed16(0.8249) + bytes(4 + 16 + 28)
    return header + struct.pack(">I", len(tags)) + directory + body


def main():
    source, grid, outPath = sys.argv[1], int(sys.argv[2]), sys.argv[3]
    if source == "naive":
        description, convert = "Device CMYK", naive_rgb
    else:
        table = read_swop_table(source)
        description, convert = "Purple Pen SWOP", lambda cmyk: swop_rgb(table, cmyk)
    with open(outPath, "wb") as out:
        out.write(profile(description, grid, convert))


main()

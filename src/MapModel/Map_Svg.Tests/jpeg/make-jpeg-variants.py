"""Make the JPEG variants of JpegDataTests: one test card, 160 by 120 pixels, coded in each of the ways the SVG target
tells apart when it decides whether to write a bitmap's JPEG data as it is or to encode the bitmap anew (B-3).

Usage: python3 -I make-jpeg-variants.py

The card has a hue gradient that darkens downwards, for the colour conversions, and edges, lines a pixel wide and a
checkerboard, for the upsampling of the colour; its shapes are not symmetric, so that a turn or mirroring shows. Each
variant is written here as <name>.jpg at quality 90 by Pillow's libjpeg-turbo:

    baseline     sequential, the colour subsampled 2:1 both ways, as most JPEG data is
    progressive  progressive
    gray         one channel
    rgb          three channels of RGB, not YCbCr, as an Adobe segment says
    cmyk         four channels of inverted CMYK, as Photoshop writes them, with black where the colour is dark
    upright      Exif data whose orientation leaves the image as it is coded
    turned       Exif data that turns the image a quarter clockwise, orientation 6
    mirrored     Exif data that mirrors it left to right, orientation 2
    profile      a colour profile of RGB primaries wider than sRGB's, Adobe RGB's, with a gamma of 2.2

The profile is written here, as no profile file's licence need then be weighed: version 2.1, a display profile of
matrix and curves, with the primaries adapted to D50 as the ICC's Adobe RGB (1998) profiles have them.

Needs Python 3 with Pillow 10 or later, for the RGB variant.
"""
import colorsys
import os
import struct

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
WIDTH, HEIGHT = 160, 120
QUALITY = 90
ORIENTATION_TAG = 0x0112


# The test card: the hue running from left to right, the lightness falling from the top down, and over it a black F,
# a white disc ringed in red, a black and white checkerboard of 2-pixel squares, and lines a pixel wide.
def test_card():
    card = Image.new("RGB", (WIDTH, HEIGHT))
    pixels = card.load()
    for y in range(HEIGHT):
        for x in range(WIDTH):
            red, green, blue = colorsys.hls_to_rgb(x / WIDTH, 0.85 - 0.6 * y / HEIGHT, 0.9)
            pixels[x, y] = (round(red * 255), round(green * 255), round(blue * 255))
    draw = ImageDraw.Draw(card)
    draw.rectangle((12, 10, 19, 59), fill=(0, 0, 0))
    draw.rectangle((12, 10, 45, 17), fill=(0, 0, 0))
    draw.rectangle((12, 30, 37, 37), fill=(0, 0, 0))
    draw.ellipse((100, 12, 141, 53), fill=(255, 255, 255), outline=(220, 0, 0), width=4)
    for row in range(10):
        for column in range(20):
            if (row + column) % 2 == 0:
                draw.rectangle((10 + 2 * column, 80 + 2 * row, 11 + 2 * column, 81 + 2 * row), fill=(0, 0, 0))
            else:
                draw.rectangle((10 + 2 * column, 80 + 2 * row, 11 + 2 * column, 81 + 2 * row), fill=(255, 255, 255))
    draw.line((70, 70, 150, 110), fill=(0, 0, 255), width=1)
    draw.line((70, 110, 150, 70), fill=(255, 255, 0), width=1)
    draw.line((60, 64, 60, 116), fill=(0, 128, 0), width=1)
    return card


# The card in CMYK, which Pillow writes inverted, as Photoshop does: black where the colour is dark, and cyan, magenta
# and yellow for the rest.
def cmyk_card(card):
    cmyk = Image.new("CMYK", card.size)
    source, target = card.load(), cmyk.load()
    for y in range(HEIGHT):
        for x in range(WIDTH):
            red, green, blue = source[x, y]
            black = 255 - max(red, green, blue)
            if black == 255:
                target[x, y] = (0, 0, 0, 255)
                continue
            scale = 255 / (255 - black)
            target[x, y] = (round((255 - red - black) * scale), round((255 - green - black) * scale), round((255 - blue - black) * scale), black)
    return cmyk


# Exif data whose first image file directory holds an orientation.
def exif_with_orientation(orientation):
    exif = Image.Exif()
    exif[ORIENTATION_TAG] = orientation
    return exif.tobytes()


def s15fixed16(value):
    return struct.pack(">i", round(value * 65536))


# An RGB display profile of matrix and curves, version 2.1: Adobe RGB's primaries adapted to D50, as in the ICC's
# Adobe RGB (1998) profiles, the D50 white, and a gamma of 2.2 for each channel.
def wide_gamut_profile():
    def xyz(values):
        return b"XYZ " + bytes(4) + b"".join(s15fixed16(value) for value in values)

    description = b"Wide gamut test profile\0"
    gamma = b"curv" + bytes(4) + struct.pack(">IH", 1, round(2.2 * 256)) + bytes(2)
    tags = [(b"desc", b"desc" + bytes(4) + struct.pack(">I", len(description)) + description + bytes(8) + bytes(3) + bytes(67)),
            (b"cprt", b"text" + bytes(4) + b"No copyright, use freely\0"),
            (b"wtpt", xyz((0.9642, 1.0, 0.8249))),
            (b"rXYZ", xyz((0.6097, 0.3111, 0.0195))),
            (b"gXYZ", xyz((0.2053, 0.6257, 0.0609))),
            (b"bXYZ", xyz((0.1492, 0.0632, 0.7446))),
            (b"rTRC", gamma), (b"gTRC", gamma), (b"bTRC", gamma)]
    offset = 128 + 4 + 12 * len(tags)
    directory, body = b"", b""
    for signature, data in tags:
        directory += signature + struct.pack(">II", offset + len(body), len(data))
        body += data + bytes(-len(data) % 4)
    header = struct.pack(">I", offset + len(body)) + bytes(4) + struct.pack(">I", 0x02100000) + b"mntrRGB XYZ "
    header += struct.pack(">6H", 2026, 11, 14, 0, 0, 0) + b"acsp" + bytes(4 + 4 + 4 + 4 + 8 + 4)
    header += s15fixed16(0.9642) + s15fixed16(1.0) + s15fixed16(0.8249) + bytes(4 + 16 + 28)
    return header + struct.pack(">I", len(tags)) + directory + body


def main():
    card = test_card()
    variants = {
        "baseline": (card, {}),
        "progressive": (card, {"progressive": True}),
        "gray": (card.convert("L"), {}),
        "rgb": (card, {"keep_rgb": True}),
        "cmyk": (cmyk_card(card), {}),
        "upright": (card, {"exif": exif_with_orientation(1)}),
        "turned": (card, {"exif": exif_with_orientation(6)}),
        "mirrored": (card, {"exif": exif_with_orientation(2)}),
        "profile": (card, {"icc_profile": wide_gamut_profile()}),
    }
    for name, (image, options) in variants.items():
        path = os.path.join(HERE, name + ".jpg")
        image.save(path, "JPEG", quality=QUALITY, **options)
        print(name, os.path.getsize(path))


main()

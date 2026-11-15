"""Write the documents of the pattern-sampling cases: one screen of white dots over yellow, drawn in each of the ways
a document can carry it, at a dot pitch given in pixels of the rendering.

Usage: python3 -I cases.py <out folder> [<pitch in pixels>...]

Each document is 200 by 200 pixels and holds a page of yellow under a screen of white dots, as a map's "White for
yellow" draws it over its yellow: a dot of 200 units in a lattice of 450, turned 45 degrees, which covers 62 % of the
area. The document's unit is scaled so that the lattice is the given pitch in pixels of the rendering; the default is
the 2.0 pixels that teanwest's screen has at the size the rendering comparison draws it. The ways:

  pattern          a <pattern> whose tile holds one dot, as the SVG target writes it (S-7, S-12);
  pattern-crisp    the same, with shape-rendering="crispEdges" on the dot inside the tile, so that the tile is
                   rasterized without antialiasing;
  pattern-ten      a <pattern> whose tile holds ten by ten dots, so that the tile is ten pitches across;
  pattern-scaled   a <pattern> whose tile is ten times larger in the document's units, scaled back by its
                   patternTransform, which asks whether an engine rasterizes the tile before that transform;
  pattern-image    a <pattern> whose tile is a bitmap of the dot at ten times the tile's size, drawn with
                   image-rendering:pixelated, which asks whether the tile can be point sampled;
  circles          every dot an element of its own, as "Draw area patterns as separate shapes" (A-2) draws them;
  uses             every dot a <use> of one defined dot, which is what symbol reuse (S-5) would write.
"""
import base64
import io
import os
import sys

from PIL import Image, ImageDraw

TILE = 450          # the lattice, in the document's units
RADIUS = 200        # the dot
SIZE = 200          # the rendering, in pixels each way
YELLOW = "#fff900"  # the ink under the screen
WAYS = ["pattern", "pattern-crisp", "pattern-ten", "pattern-scaled", "pattern-image", "circles", "uses"]


def document(scale, defs, body):
    """One document: the page of yellow in a frame whose unit is scaled as the map's is, and the screen over it."""
    side = int(SIZE / scale) + 1
    return ('<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" '
            'width="%d" height="%d" viewBox="0 0 %d %d">' % (SIZE, SIZE, SIZE, SIZE)
            + "<defs>" + defs + "</defs>"
            + '<g transform="matrix(%.9f 0 0 -%.9f 0 %d)">' % (scale, scale, SIZE)
            + '<rect x="0" y="0" width="%d" height="%d" fill="%s"/>' % (side, side, YELLOW)
            + body + "</g></svg>")


def tile(content, size=TILE, turn='patternTransform="rotate(45)"'):
    """A pattern of the given tile, with its middle at the origin, turned as the map's screen is turned."""
    half = size / 2
    return ('<pattern id="screen" patternUnits="userSpaceOnUse" x="%g" y="%g" width="%g" height="%g" '
            'viewBox="%g %g %g %g" %s>%s</pattern>' % (-half, -half, size, size, -half, -half, size, size, turn, content))


def lattice(scale):
    """The middles of the dots of a lattice turned 45 degrees about the middle of the page, far enough to cover it."""
    side = SIZE / scale
    reach = int(side * 1.5 / TILE) + 2
    return [(i * TILE, j * TILE) for i in range(-reach, reach + 1) for j in range(-reach, reach + 1)]


def dot_bitmap(pixels):
    """A bitmap of one tile, the dot white on nothing, at the given pixels a side, as a data URI."""
    drawn = pixels * 8
    image = Image.new("RGBA", (drawn, drawn), (255, 255, 255, 0))
    radius = RADIUS / TILE * drawn
    ImageDraw.Draw(image).ellipse([drawn / 2 - radius, drawn / 2 - radius, drawn / 2 + radius, drawn / 2 + radius],
                                  fill=(255, 255, 255, 255))
    image = image.resize((pixels, pixels), Image.LANCZOS)
    written = io.BytesIO()
    image.save(written, "PNG")
    return "data:image/png;base64," + base64.b64encode(written.getvalue()).decode("ascii")


def write(way, scale, folder):
    """Write one way's document, and return its path."""
    side = int(SIZE / scale) + 1
    fill = '<rect x="0" y="0" width="%d" height="%d" fill="url(#screen)"/>' % (side, side)
    half = TILE / 2
    if way == "pattern":
        text = document(scale, tile('<circle r="%d" fill="#fff"/>' % RADIUS), fill)
    elif way == "pattern-crisp":
        text = document(scale, tile('<circle r="%d" fill="#fff" shape-rendering="crispEdges"/>' % RADIUS), fill)
    elif way == "pattern-ten":
        dots = "".join('<circle cx="%d" cy="%d" r="%d"/>' % (i * TILE, j * TILE, RADIUS)
                       for i in range(-5, 6) for j in range(-5, 6))
        text = document(scale, tile('<g fill="#fff">%s</g>' % dots, TILE * 10), fill)
    elif way == "pattern-scaled":
        content = '<circle r="%d" fill="#fff"/>' % (RADIUS * 10)
        text = document(scale, tile(content, TILE * 10, 'patternTransform="rotate(45)scale(.1)"'), fill)
    elif way == "pattern-image":
        image = ('<image xlink:href="%s" x="%g" y="%g" width="%g" height="%g" image-rendering="pixelated" '
                 'style="image-rendering:pixelated"/>' % (dot_bitmap(20), -half, -half, TILE, TILE))
        text = document(scale, tile(image), fill)
    elif way in ("circles", "uses"):
        middle = SIZE / scale / 2
        turned = '<g transform="translate(%.1f %.1f)rotate(45)" fill="#fff">' % (middle, middle)
        if way == "circles":
            dots = "".join('<circle cx="%d" cy="%d" r="%d"/>' % (x, y, RADIUS) for x, y in lattice(scale))
            text = document(scale, "", turned + dots + "</g>")
        else:
            dots = "".join('<use xlink:href="#dot" x="%d" y="%d"/>' % (x, y) for x, y in lattice(scale))
            text = document(scale, '<circle id="dot" r="%d"/>' % RADIUS, turned + dots + "</g>")
    else:
        raise ValueError("no such way: " + way)
    path = os.path.join(folder, way + ".svg")
    with open(path, "w", encoding="utf-8") as written:
        written.write(text)
    return path


def main(arguments):
    if not arguments:
        sys.stderr.write(__doc__)
        return 2
    out = arguments[0]
    pitches = [float(pitch) for pitch in arguments[1:]] or [2.0]
    for pitch in pitches:
        folder = os.path.join(out, "pitch-%.3f" % pitch)
        os.makedirs(folder, exist_ok=True)
        for way in WAYS:
            write(way, pitch / TILE, folder)
        print("%s: %d documents, dot %.2f px in a lattice of %.2f px"
              % (folder, len(WAYS), RADIUS * 2 * pitch / TILE, pitch))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

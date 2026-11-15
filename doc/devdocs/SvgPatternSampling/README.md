# A fine pattern tile in the browsers

The measurements behind the design's section "A fine pattern tile in the browsers"
([`../SvgExportDesign.md`](../SvgExportDesign.md)): what Chromium, Firefox and WebKit make of a `<pattern>` whose tile
is about two pixels across, what the Skia reference makes of it, and what the same screen drawn element by element
comes to. They answer @Pauli's question of Nov 15, of teanwest's yellow screen: why the Skia rendering is grainy, with
white pixels in it, where all three browsers draw an even yellow.

The screen is a map's, not an invention: teanwest draws "White for yellow" as a pattern of white dots over its yellow,
a dot of 200 map units in a lattice of 450 turned 45 degrees, which covers 62 % of the area. At the size the rendering
comparison draws the map, the document's unit is 0.004442991 pixels, so the lattice is 2.000 pixels and the dot 1.78.

## How a rendering is judged

Nothing here is held to an opinion of what the screen should look like. The same document is rendered ten times
larger, where the dots are 17.8 pixels across and every engine resolves them, and each pixel of the rendering is held
to the mean of its hundred: that mean is what the pixel would hold if the engine sampled the drawing exactly.
Chromium, Firefox and WebKit agree on it within four levels, so the truth is no engine's own.

Of a patch, only the pixels that hold the screen and nothing else are measured: those whose hundred sub-pixels all lie
on the line between the paper and the ink the tile paints with. Four numbers describe each image of such a patch: the
mean of the channel that parts the two colours most, which says how much ink the screen carries; the spread of that
channel, which is the grain; and the shares of pixels that stayed paper white or full ink, which say whether anything
of the screen survived as itself.

## Needs

- A checkout of `feature/svg-target`, for the sentinel font the browsers are started with and for the reference image
  of the case in `src/MapModel/TestFiles/svg_reference`.
- A document the rendering comparison wrote, such as `svg/teanwest.svg` of an SVG loop run.
- Python 3 with NumPy and Pillow, and Node.
- Microsoft.Playwright 1.63.0 in the NuGet cache with its browsers, as `../SvgTargetProgress.md` ("Environment")
  installs them, named by `PLAYWRIGHT_MODULE` and `PLAYWRIGHT_BROWSERS_PATH`.
- About 10 minutes for a map of teanwest's size.

## Scripts

| Script | Does |
| --- | --- |
| `run-all.sh <checkout> <document.svg> <work folder>` | Everything below, into a work folder of its own: the ways of carrying one screen at a lattice of 2.0 and of 2.169 pixels, rendered and measured; then the given document's own screens, located, rendered and measured against its Skia reference. Writes `cases.txt` and `patches.txt` beside the measurements and the sheets. |
| `cases.py <out folder> [<pitch>...]` | Writes one document for each way of carrying a screen of white dots over yellow, at a lattice of the given pixels: `pattern` as the SVG target writes it (S-7, S-12), `pattern-crisp` with the tile rasterized without antialiasing, `pattern-ten` with ten by ten dots in the tile, `pattern-scaled` with a tile ten times larger in the document's units scaled back by its `patternTransform`, `pattern-image` with a bitmap of the dot as the tile, point sampled, `circles` with every dot an element of its own, as A-2's "Draw area patterns as separate shapes" draws them, and `uses` with every dot a `<use>` of one defined dot, as symbol reuse (S-5) would write it. |
| `render.mjs <svg> <w>x<h> <out> [--scale n] [--crop x,y,w,h] [--browsers ...] [--sentinel <font>]` | Renders a document in Chromium, Firefox and WebKit as the tests' `BrowserRenderer` renders it: the flags, one device pixel to a CSS pixel, the fontconfig file of the sentinel font alone without hinting, and two frames before Firefox's and WebKit's screenshots. `--scale` draws it that many times larger and `--crop` takes a rectangle of it, which together resolve what one pixel holds. |
| `measure-cases.py <out folder> [--font <ttf>]` | Measures each way and engine against the same way resolved tenfold, prints the table, and writes `measurements.tsv` and `cases.png`, the renderings beside each other. |
| `locate.py <svg> <out folder> [<pattern id>...]` | Writes the document with one pattern's tile painted magenta, for each pattern, so that the pixels that differ from the document's own rendering are the pixels that pattern draws. |
| `patches.py <folder of the probes' renderings> [--sizes ...]` | Chooses, for each pattern, the largest square patch that pattern alone draws on, and prints its id, left, top and side. |
| `measure-patches.py <work folder> <patches file> [--font <ttf>]` | Measures, in each patch, the Skia reference and the three browsers against the patch resolved tenfold, and writes `measurements.tsv` and `patches.png`. |

## What the run writes

- `cases/measurements.tsv`: every way, lattice and engine, with the tone, the spread, the shares and how far the tone
  lies from the resolved drawing; `cases/pitch-<pitch>/cases.png` has the renderings beside each other, a row for
  each way.
- `map/measurements.tsv`: the same for each screen of the document, the Skia reference among the images;
  `map/patches.png` has the patches beside each other, a row for each screen.
- `map/patches.tsv`: the patch `patches.py` chose for each pattern, and how many pixels of the rendering that
  pattern draws on in all.
- `cases.txt` and `patches.txt`: what the two measurements printed.

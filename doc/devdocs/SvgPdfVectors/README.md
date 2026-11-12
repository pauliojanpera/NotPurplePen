# A PDF base map as vectors: probes

The probes behind the design's section "A PDF base map as vectors" (`../SvgExportDesign.md`): what a map's PDF
holds, how the application draws it, what the SVG export writes of it today, and what it would write if the page went
in as vectors, either through a converter that writes SVG or through a reader whose drawing calls the SVG target
draws. They run on Linux; every figure in the section comes from `run-all.sh`.

## Needs

- A checkout of `feature/svg-target` at 267efc8 or later, for the application's projects and test files. A worktree of
  a clone that `git sparse-checkout` has touched will do, as the probe builds neither PdfSharp nor AvPurplePen, whose
  GitVersion cannot open such a clone.
- The .NET 10 SDK, and the tests' Playwright browsers: Chromium's headless shell, Firefox and WebKit, as
  `../SvgTargetProgress.md` ("Environment") says how to install them, found through `PLAYWRIGHT_BROWSERS_PATH` and the
  `Microsoft.Playwright` package in the NuGet cache.
- `poppler-utils` and `mupdf-tools` from apt (`pdftocairo` 24.02 and `mutool` 1.23.10 on Ubuntu 24.04), and a
  `fi_FI.UTF-8` locale for `converters.sh`'s second run (`locale-gen fi_FI.UTF-8`).
- Python 3 with NumPy, SciPy, Pillow and pikepdf (`pip install --break-system-packages pikepdf`).
- For `pdfjs-ops.mjs`, `npm install pdfjs-dist` in a folder of its own.

## Scripts

| Script | Does |
| --- | --- |
| `run-all.sh <checkout> <out> [<pdf>...]` | Everything below, on the PDFs that the test events use as maps (or on those given, as paths within `src/TestFiles`), into `<out>`: each PDF's folder holds its inventory, the reference, the SVGs, their renderings and difference images, and `<out>/summary.tsv` has a row for each PDF, way and browser. `<out>/measurements.jsonl` keeps every line the steps printed. It takes about 25 minutes. |
| `inventory.py [--json <out>] <pdf>...` | What the first page of a PDF holds that an SVG of it has to keep: its producer and boxes, its operators, its colours by colour space (spot colours by name), the graphics states that overprint, blend, set opacity or mask, its fonts and whether they are embedded, its images, shadings, patterns and form XObjects. Purple Pen reads page 0 alone. |
| `probe/` | A console program built against the checkout's projects, unchanged (`dotnet build probe/PdfVectorsProbe.csproj -p:AppSrc=<checkout>/src`): `reference <pdf> <ppi> <png>` draws the page as the application draws it, through `MapDisplay` and PdfConverter's PDFium bitmap of 600 dots per inch, with the Skia target; `bitmap-svg <pdf> <svg>` writes the page as the SVG export writes a PDF map today; `vector-svg <pdf> <svg> [--overprint-darken]` reads the page with PdfPig into the SVG target's drawing calls; `pdfium-objects <pdf>` reports what PDFium's page-object API gives of it. Each prints one line of JSON with what it measured. |
| `converters.sh <out> <pdf>...` | The converter way: `pdftocairo -svg` and `mutool convert -F svg` write each page, timed, twice, the second time in another locale, time zone and folder, and say whether the bytes repeat. |
| `render.mjs <jobs.json> <sentinel font>` | Renders SVGs in Chromium, Firefox and WebKit as the tests' `BrowserRenderer` renders them: the flags, the fontconfig file of the sentinel font alone without hinting, and two frames before Firefox's and WebKit's screenshots. |
| `compare.py [--text] [--diff <png>] <expected> <rendering>` | Holds a rendering to the reference by F-1's rule, `PixelTolerance.Default`, as `ImageComparison.MeasureExcess` measures it, and counts the pixels outside it. |
| `pdfjs-ops.mjs <pdf>...` | What pdf.js, the reader of a TypeScript port in a browser, keeps of a page in its operator list: the form of its colours, the graphics states' blending and overprinting, and its fonts as it converts them for the browser. Run with `PDFJS_MODULE` naming `pdfjs-dist/legacy/build/pdf.mjs`. |
| `determinism.sh <checkout> <out> <pdf>...` | Writes the vector way's SVG of each PDF under several runtime settings and locales, and says whether the bytes repeat. |

## The probe's reader

`probe/PdfPigReader.cs` is a probe, not a design of the reader the application would have: it reads what the test
maps hold and counts what it does not draw. Each path becomes `FillPath` and `DrawPath` with a CMYK brush or pen;
DeviceCMYK keeps its values, gray becomes black ink, and RGB goes through `CmykColor.FromRgb`, since a brush of the
target holds CMYK alone. Clips are pushed as the stream clips and popped as its graphics states are restored. Tiling
patterns become pattern brushes, one for each distinct cell, the cell drawn once in the tile and the fill drawn under
the pattern's matrix moved by half a cell, since the target's tile has its middle at the origin. Glyphs are filled
outlines. Images are decoded by PdfPig, a JPEG kept as it is, and drawn through `DrawBitmap`. With `--overprint-darken`
whatever overprints is drawn with darken blending, as COL-7 shows a map's overprinting. PdfPig 0.1.16 leaves a
graphics state's `op` unset when only `OP` is given, against ISO 32000-1's table 58, which the reader corrects.

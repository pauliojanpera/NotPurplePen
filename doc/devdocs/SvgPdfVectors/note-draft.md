## A PDF base map as vectors

Written on Nov 12 from `feature/svg-target` at 267efc8, as the task the handover gives (`SvgTargetProgress.md`, Tasks,
Nov 12). The requirements leave copying a PDF base map as vectors out of scope; this section finds out whether the SVG
could carry the page's vectors instead of the bitmap it carries now, and at what cost, and ends with the choice put to
@Pauli. Nothing of it is built. The probes are in [`SvgPdfVectors/`](SvgPdfVectors/README.md), whose `run-all.sh`
repeats every figure below on Linux.

### How the application handles a PDF map today

- **Loading.** `MapDisplay.SetMapFile(MapType.PDF, …)` has `CoreMapUtil.ValidatePdf` convert the PDF into a bitmap
  through `PdfMapFile`, which keeps the result as a PNG in `PurplePen` under the temporary folder, named after a SHA-1
  of the PDF's bytes, for 180 days. The conversion is `Services.PdfConverter`, in AvPurplePen `PdfConverterService`,
  which calls `PdfConversion.Convert` of the `PdfConverter` project: PDFium, through PDFiumCore 157.0.8100, renders
  page 0 alone at 600 dots per inch, at the size `FPDF_GetPageSizeByIndex` gives, its crop box, onto white, with no
  render flags, so without annotations, into an 8-bit RGB PNG that records 600 dots per inch.
- **Drawing.** From then on the map is a bitmap map. `MapBounds` is the bitmap's pixels at 600 dots per inch, its
  origin at the page's lower left corner, and `DrawBitmapMap` draws the whole bitmap under `BitmapTransform`, smoothed
  (`MediumQuality`) when anti-aliased, and by nearest neighbour when not, or when the bitmap has more than 50 million
  pixels, as an A2 page has at 600 dots per inch (139 million). A PDF map has no colours of its own, so
  `HasLayersAboveCourse` is false and the course is drawn above the whole map.
- **PDF export.** `CoursePdf` does not draw the bitmap. When PdfSharp can import the page (`CanReadPdfPage`), each page
  of the output starts as the PDF's page 0: copied whole by `BeginCopiedPage` when the course is at the map's scale
  and its print area is the whole page, and otherwise drawn by `BeginCopiedPartialPage` as a form XObject (`XPdfForm`)
  under a clip to the print rectangle and a transform from the map rectangle, measured from the crop box's corner at
  72 points to the inch, onto it. The map display's map is then set to none, and the course is drawn over the copy.
  The page's content is never interpreted: its colour spaces, spot colours, overprinting, transparency, fonts and
  images reach the output byte for byte. A page PdfSharp cannot import falls back to the bitmap.
- **SVG export.** `CourseSvg` draws the map's part through `MapDisplay.Draw(MapBelowCourse)`, which for a PDF map is
  `DrawBitmapMap`: one `DrawBitmap` of the whole page's bitmap, whatever area the pages show, under the map's label
  and outside any colour run (M-5). The target writes it once (B-5), through the bitmap's `WriteToStream`, as PNG,
  the bitmap's own format (B-3), which ImageSharp's encoder writes with an alpha channel the converter's PNG did not
  have, and embeds it as base64 (B-4). Above 50 million pixels the use is pixelated (B-2), as the application draws it.
### Two ways, and where the reading can live (C-2, C-3)

- **A converter writes the page as SVG**, and the export takes its document into the map's part. The target then
  holds content that none of its calls made: a new call of `IStructuredGraphicsTarget`, say one that places an SVG
  fragment under a transform, would have to rename the fragment's ids to keep them unique (L-5), and could neither
  write its numbers in the target's steps (S-1 to S-3), nor give its colours their CMYK values (COL-1), nor be read
  back by the geometry check (V-10). C-2 and C-3 allow such a call, as the fragment is a plain string; the converter
  itself, a program of its own, runs in the application.
- **The application reads the page into drawing calls**, and the target draws them as it draws a map: paths filled
  and stroked with CMYK brushes and pens, clips, pattern brushes and bitmaps, all of them calls that the target fully
  implements today (DL-5). The target references `Graphics2D` alone (C-2) and takes data only through its interfaces
  and plain values (C-3), so the reader cannot live in it: it lives in the application, in `PurplePenCore` beside
  `MapDisplay` or in a project of its own that references `Graphics2D` and the reading library, as the map code lives
  beside the targets. `MapDisplay` would call it for a PDF map in place of `DrawBitmapMap`, for a target that asks for
  vectors, as `PatternBrushesAreVectors` asks for pattern fills (S-7). Every other target could draw a PDF map so too.
  What C-3 does not carry today: a colour that is not CMYK (a `CmykColor` holds CMYK alone, and `FromRgb` invents
  CMYK values that COL-1 would then write), a font program of the PDF's own (`IFontSubsetProvider` gives the faces of
  a family the application can open), and an image's original JPEG data (`WriteToStream` encodes anew).

### The candidates

| Candidate | Way | Licence | Purple Pen and the fork could ship it | Windows, Linux, macOS | Browser |
| --- | --- | --- | --- | --- | --- |
| Poppler with Cairo, `pdftocairo -svg` | Converter | Poppler GPL-2 or GPL-3, Cairo LGPL-2.1 or MPL-1.1 | Not linked into the BSD application; only as a separate program beside it, under the GPL's terms for that program, with its source | Builds exist for each, outside the package registries on Windows | Unofficial WebAssembly builds |
| MuPDF, `mutool convert -F svg`, or its device interface | Converter, or reader | AGPL-3.0-or-later, or Artifex's commercial licence | Not linked without a commercial licence; as a separate program under the AGPL's terms | Yes | MuPDF.js, AGPL |
| PDFium's page objects, through PDFiumCore | Reader | BSD-3-Clause, the bindings and binaries Apache-2.0 | Yes: the application ships them already, for PdfConverter | Yes | A WebAssembly build of bblanchon/pdfium-binaries |
| PdfPig 0.1.16 | Reader | Apache-2.0 | Yes, with its notice | Yes: managed code alone, .NET Standard 2.0 and .NET 8 | Only inside a .NET application compiled to WebAssembly; the TypeScript port would translate it |
| pdf.js 6.5 (`pdfjs-dist`) | Reader, for the TypeScript port | Apache-2.0 | Not needed by the .NET application | In Node | Yes: it is Firefox's PDF viewer |

Each was run on the test maps (below): the two converters as programs, PDFium's page-object API through PDFiumCore
(`pdfium-objects`), PdfPig through a reader written for the probe (`vector-svg`, see the README), and pdf.js through
its operator list (`pdfjs-ops.mjs`).

- **The converters** write every colour as sRGB, which they convert from CMYK each its own way: Poppler with its own
  formula, MuPDF 1.23 through its default CMYK profile. Neither writes an overprint, and both write text as outlines
  of its glyphs, Cairo as symbols it uses, MuPDF as paths. Cairo draws a tiling pattern as an SVG pattern. Both keep a
  JPEG's data as it is. Their numbers are points with six decimals (Cairo) or four significant digits (MuPDF).
- **PDFium's page objects** give each object's fill and stroke as 8-bit RGBA after PDFium's own conversion, which is
  the one the application's bitmap shows; no colour space, spot colour, overprint, blend mode (there is a setter and
  no getter) or soft mask; a tiling pattern only as a bitmap of it (`FPDFPageObj_GetRenderedFillPattern`,
  experimental). It does give paths, clip paths, glyph outlines, font programs, images with their filters, and marked
  content, OCAD's layer names among it.
- **PdfPig** gives the content stream's operators with their graphics state: DeviceCMYK, gray and RGB colours as given,
  a separation's name and alternate space, overprint and its mode, blend mode, constant opacity and soft mask, the
  CTM and the clipping operators, glyph outlines of every font kind, and images, a JPEG's data as it is. It is a
  reader, not a renderer: what it reads, the reader has to draw. Its 0.1.16 sets a graphics state's non-stroking
  overprint only from `op`, where an absent `op` takes `OP`'s value (ISO 32000-1, table 58), which the probe's reader
  corrects. Its reading part is about 89,000 lines of C#.
- **pdf.js** gives an operator list whose colours are already sRGB, as `#rrggbb` strings: its evaluator converts
  DeviceCMYK, through the CMYK profile it now ships. Its graphics states keep opacity and blend mode, as the canvas
  operation names them (`darken`, `source-over`), and drop overprinting. It converts each embedded font into an
  OpenType font a browser takes, 11 to 71 KB for the Arial subsets of Lincoln-CMYK, and reads tiling patterns as
  patterns.
### What each keeps of what the SVG has to keep

| What | The bitmap, today | A converter (Cairo, MuPDF) | A reader on PDFium's page objects | A reader on PdfPig, as probed | A reader on pdf.js, for the port |
| --- | --- | --- | --- | --- | --- |
| CMYK values in `icc-color()` (COL-1) | No: the page is pixels | No: sRGB alone | No: 8-bit RGB | Yes, for DeviceCMYK, which OCAD's, PrimoPDF's and Purple Pen's own PDFs use | No: sRGB alone |
| RGB colours, as OpenOrienteering Mapper writes them | As pixels | Yes, as sRGB | Yes | Only as CMYK that `CmykColor.FromRgb` invents, which COL-1 writes and the CMYK model converts back otherwise: a change of `Graphics2D` is needed | Yes, as sRGB |
| Spot colours | No | No | No | The separation's name and alternate are read; the target has nowhere to write the name (none of the test maps has one) | No |
| Overprinting (COL-7) | No: PDFium does not show it | No | No | Yes, read; drawn with darken blending (D-8) if chosen | No: dropped |
| Opacity and blend modes | As pixels | As SVG opacity and blending | Opacity only | Constant opacity as a colour's alpha (COL-5); darken alone among the blend modes (D-8); soft masks and transparency groups with their own opacity not drawn (none in the test maps) | Opacity and blend modes |
| Clipping | As pixels | Yes, Cairo with a copy of the clip for each drawing | Yes | Yes, as clip paths (D-2) | Yes |
| Tiling patterns | As pixels | Yes | As bitmaps alone | Yes, as pattern brushes (D-7), one for each distinct cell | Yes |
| Text (T-6, T-7) | As pixels | Outlines of its glyphs, each glyph defined once | Outlines, or the font programs | Outlines, as filled paths; the font programs are readable, but embedding them needs a converter to web fonts and a way past C-3 | Fonts converted to OpenType, which a browser takes |
| Images (B-3) | The page is one image | Kept, a JPEG as its data | Raw data and filters | Kept, a JPEG as its data, but `WriteToStream` encodes it anew at the export's quality | Decoded |
| A place in the map's structure (M-5) | Under the map's label, outside any run | A fragment under the map's label | Under the map's label, outside any run | Under the map's label, outside any run; OCAD's PDFs also name each colour of their table as a layer (marked content `/Layer` with its `/Title`, a transparency group each), which could become colour runs (M-3) | As the port draws it |
### Proposal

The choice is @Pauli's; nothing is built before it.

- **(a) Keep the bitmap.** The export stays as it is: the SVG shows a PDF map as the application draws it, and the
  requirements keep copying it as vectors out of scope. The page stays pixels without CMYK, written whole at 600 dots
  per inch with an alpha channel it does not need, whatever area the pages show.
- **(b) Keep the bitmap, written smaller.** As (a), but the export writes only the part of the bitmap that its pages
  show (`DrawBitmapPart`), and as RGB without the alpha channel that ImageSharp adds. It changes no requirement and
  keeps the drawing the application's; it saves what the print area leaves out and about a tenth of the rest, and
  leaves every other cost of (a).
- **(c) A converter writes the page as SVG.** Not recommended: both converters lose the CMYK values (COL-1) and the
  overprinting, draw the colours otherwise than the application does, which puts a third of St Pauls's pixels and more
  outside F-1, ship as programs under the GPL or the AGPL beside the BSD application, and put into the document content
  that none of the target's rules governs. pdftocairo also wrote 1.86 GB for the A2 map.
- **(d) The application reads the page with PDFium's page objects**, which it ships already. Not recommended: its API
  gives 8-bit RGB alone, no overprinting or blend mode, and tiling patterns only as bitmaps, so OCAD's screens would be
  pixels again and COL-1 unmet.
- **(e) The application reads the page with PdfPig** into drawing calls, which the target draws as it draws a map, and
  which every target could draw. It keeps a CMYK map's values, its overprinting, clips and patterns, writes far smaller
  files for the maps OCAD and OpenOrienteering Mapper export, and draws them within F-1 of the application's drawing
  but for fine hatching. Building it takes, beside the reader, a paint for RGB colours in `Graphics2D` (C-3), a JPEG
  passed through as its data, and a requirement of its own in place of the line that leaves it out of scope; the
  bitmap stays as the fallback for a page the reader cannot draw, as PDF export falls back to it for a page PdfSharp
  cannot import.

Within (e), three questions are @Pauli's too:

- **Overprinting.** Drawn with darken blending, as COL-7 shows a map's overprinting and as a viewer showing overprints
  would show it, or left as the application's own drawing shows it, without.
- **OCAD's layers.** The page under the map's label outside any run (M-5), as a bitmap map is now, or each of OCAD's
  layers, which name the colours of its table, as a colour run (M-3).
- **The setting.** Vectors as the export's default for a PDF map, or as a setting named for what it does, "Draw a PDF
  map as vectors" (A-2), with the bitmap as the default.

**Recommendation: (e)**, with overprinting drawn darker, OCAD's layers as colour runs, and vectors as the default with
the bitmap as the fallback. TBD reasons.

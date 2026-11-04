# What Scribus and Inkscape need to read the export's CMYK colours (F-6)

Scripts that read, without a person at the screen, what Scribus 1.6.1 and Inkscape 1.2.2 (Ubuntu 24.04's packages)
make of an SVG's colours, the variants of an export they were run on, and what came out (task 5, Nov 4). The export
writes each colour in a class rule of its `<style>` sheet, as sRGB and then as the same sRGB with
`icc-color(cmyk,c,m,y,k)`, naming `<color-profile name="cmyk"/>`, which has neither an id nor an ICC profile behind it.

## Readers

- `scribus_colours.py` runs in Scribus's scripter under Xvfb: it places each file on a page of a new document, as
  File > Import > Get Vector File does, saves the document as SLA, and lists from it every colour with its colour space
  and values and every item with its fill colour, line colour and line width. `summarise_scribus.py` sums that up a
  line a file.
- `inkscape_colours.py` starts Inkscape with its GUI under Xvfb and a D-Bus session of its own, with the CMS page as
  its colour page, opens Fill and Stroke and selects each element through Inkscape's actions on D-Bus
  (`dialog-open`, `select-clear`, `select-by-id`), captures the dialog and reads the profile, the channels and their
  values with Tesseract. It keeps each captured dialog as a PNG. An element of the export is named by its `data-o`,
  and `o<n>^` names its group: Inkscape shows a style set on a group on the group, and its children as unset.
- `render_chromium.py` draws each file in Playwright's Chromium, with the flags `ChromiumRenderer` passes, and counts
  the pixels that differ from the first file's drawing.
- `make_variants.py` writes the variants below of one export and their sizes, plain and gzipped; `make_profile.py`
  writes a small CMYK output profile (below); `run-all.sh <work> <export.svg> <probes>` runs everything.
- `variants/` holds the variants of OverprintTestLowerPurple-Course12.svg from the gallery, all but the embedded
  SWOP profile (743 KB) and Inkscape's form (which names a path of the machine it was made on), with
  `device-cmyk.icc`, the device CMYK profile, as the small profile; `profile-linked.svg` needs
  `src/PurplePen/USWebCoatedSWOP.icc` beside it.

Each reader was checked on `known-answers.svg`, which paints one rectangle in each form a colour can take, with
distinct values: a presentation attribute, a style attribute with one declaration and with two, Inkscape's spacing
`icc-color(cmyk, 0.14, ...)`, a class with sRGB only, with `icc-color()` only and with both, a stroke class, a group's
style, an element in `<defs>` and a `<use>` of it. Scribus gave every sRGB rectangle its RGB value and every
`icc-color()` it read its CMYK values; Inkscape, with a CMYK profile behind the name, gave each the values written, the
group's on the group and the `<defs>` element's on that element, while it shows the group's rectangle and the `<use>`
as unset.

## What each reads

**Scribus 1.6.1** applies no class rule of a `<style>` sheet, not even one with sRGB alone. Every element of the export
gets SVG's initial style, a black fill and no stroke: OverprintTestLowerPurple's Course 12, mapexchange1's Course 5 and
printarea2's Course 1 come in as black shapes, strokes included (all of 17, 640 and 20,438 items). It applies a style
attribute, a presentation attribute, a group's style and a `<use>`. It reads an `icc-color()` with four numbers after
the profile name as CMYK whatever the profile, ignores `<color-profile>`, and takes the last paint declaration it can
read, so `fill:#a00000;fill:#a00000 icc-color(cmyk,...)` is CMYK. The values pass through Qt's 8-bit `QColor`: 0.14
comes in as 14.12 %, 0.68 as 67.84 %, within 0.2 % of the map's values. A colour equal to one of Scribus's own, such as
black 0/0/0/100 or magenta 0/100/0/0, takes that colour's name. It reports `stroke-miterlimit` and `mix-blend-mode` as
unsupported, so overprinting is not shown.

**Inkscape 1.2.2** reads `icc-color()` from every form: class, style attribute, presentation attribute, group, an
element in `<defs>`. What its CMS page shows depends on the `<color-profile>` alone:

| `<color-profile>` | CMS page |
| --- | --- |
| `name="cmyk"`, as now | the profile `cmyk`, and no channels |
| with an `id`, no `xlink:href` | R, G and B: the first three numbers, without K (what @Pauli saw on Nov 4) |
| with an `xlink:href`, no `id` | the profile `cmyk`, and no channels |
| with an `id` and an `xlink:href` to a CMYK profile: a file name beside the SVG, a `file:` URI or a `data:` URI | C, M, Y and K, the values written |

Inkscape's profile manager learns of a `<color-profile>` at load only when it has an id (`SPDocument::addResource`
signals only objects with ids), so the name finds nothing; a profile without ICC data defaults to an RGB input profile
(`ColorProfileImpl`), whose channels are R, G and B. Inkscape draws with the sRGB value whatever the profile; the
profile serves the CMS page, its gamut and ink warnings and its Fix button, which offers to replace the sRGB with the
profile's conversion when the two differ.

For a profile it links itself (Document Properties > Color), Inkscape writes
`<color-profile name="U.S.-Web-Coated--SWOP--v2" xlink:href="file:///.../USWebCoatedSWOP.icc" id="/.../USWebCoatedSWOP.icc"/>`,
naming it after the profile's description with every character but letters, digits, `_`, `:`, `-` and `.` made `-`,
and a colour as one declaration, `fill:#ab499c icc-color(U.S.-Web-Coated--SWOP--v2, 0.35, 0.85, 0, 0)`, in the
element's style attribute (`DocumentProperties::linkSelectedProfile`, `SPColor::toString`). It does not read that
name back: from a style attribute or a class rule, an `icc-color()` naming `U.S.-Web-Coated--SWOP--v2` comes in with
no profile (`<none>`), and only a presentation attribute keeps it (`profile-names.svg`, which needs
`USWebCoatedSWOP.icc` beside it). A plain name such as `cmyk` reads from every form. Any colour Inkscape rewrites,
as when ungrouping moves a group's style onto its children, it writes in the same way, as one declaration that a
browser drops: `fill:#100000 icc-color(cmyk, 0.19, 0.29, 0.39, 0.09)`.

**Chromium** keeps the first declaration of a pair and drops a paint it cannot parse, so a variant draws as now as
long as an sRGB paint comes before or beneath each `icc-color()`.

## The variants

Of OverprintTestLowerPurple's Course 12 from the gallery, with the sizes of the four gallery files @Pauli opened on
Nov 4: OverprintTestLowerPurple-Course12 (7,521 bytes), mapexchange1-Course5 (51,995),
printarea2-Course1 (1,731,272) and LincolnParkPrintAreas2 (4,970,821), each as a change from now, plain and with
`gzip -9`. "Small profile" is `<color-profile id="cmyk" name="cmyk" xlink:href="data:...">` holding the 516-byte device
CMYK profile, 751 bytes more than now, which the sizes count; the readers ran on the export with the grid-5 profile
below, and on the known answers with both, which read alike.

| Variant | Scribus | Inkscape | Chromium | Size, plain | Size, gzip |
| --- | --- | --- | --- | --- | --- |
| as-now | every item black, no strokes | `cmyk`, no channels | — | — | — |
| style-paint: the paints in style attributes, both declarations | colours CMYK; strokes also filled black, no widths | as now | same | +6.5, +81, +57, +69 % | +0.9, +3.2, +2.8, +3.8 % |
| style-all: every class as a style attribute | everything: fills, strokes, widths, `fill:none` | as now | same | +7.9, +126, +69, +71 % | −0.1, +3.6, +3.6, +4.4 % |
| style-groups: every class as a style attribute, on the innermost group whose children all share it | everything | as now, the colour on the group | same | +5.0, −4.0, −3.7, −13.7 % | +0.5, −0.4, +0.1, −0.5 % |
| class-groups: the same with classes, for comparison | every item black | as now | same | −0.3, −11.5, −11.0, −15.0 % | +0.4, −0.6, −0.8, −0.8 % |
| attr-paint, attr-all: paints as presentation attributes, `fill="#ab499c icc-color(...)"` | colours (attr-all: everything) | as now | paints lost, 8,140 pixels differ | | |
| class-and-attr: classes as now, and every declaration again as presentation attributes | everything | as now | same | +19, +121, +63, +61 % | +5.7, +5.1, +3.4, +2.7 % |
| no-fallback: the classes without the sRGB declaration | every item black | as now | paints lost | | |
| profile-id, profile-linked-no-id | every item black | R, G, B / no channels | same | +10, +33 bytes | |
| profile-linked: id and `USWebCoatedSWOP.icc` beside the file | every item black | C, M, Y, K | same | +43 bytes | |
| profile-embedded: id and Purple Pen's SWOP profile as a `data:` URI | every item black | C, M, Y, K | same | +743 KB | +435 KB |
| profile-small | every item black | C, M, Y, K | same | +751 bytes: +10, +1.4, 0.0, 0.0 % | +9.7, +3.7, +0.1, 0.0 % |
| inkscape-form: Inkscape's own form, above | colours; strokes filled black | no profile, `<none>`: the name with dots is lost from style attributes | paints lost | +12.8 % | |
| style-groups with the small profile | everything | C, M, Y, K, on the group a style sits on | same | +15, −2.5, −3.6, −13.7 % | +10.1, +3.2, +0.2, −0.4 % |

"Same" is pixel for pixel, on the first page of each of the four files.

## The profile behind `cmyk`

`make_profile.py` writes a CMYK output profile with only an A2B0 table, to the PCS in CIELAB: as device CMYK,
R = (1 − C)(1 − K) and so on, which a grid of 2 holds exactly in 516 bytes, or from the table
`SwopColorConverter` converts with (`swopsamples.dat`, 11 samples a channel), at a grid of 3 (912 bytes), 5 (4,176) or
11 (88,272). Converted by Little CMS against that table over 407 colours, sRGB levels differ by at most 115 (mean 35)
for device CMYK, 68 (11) at a grid of 3, 36 (4.5) at 5, 9 (1.0) at 11, and 12 (1.1) for Adobe's profile itself. Both
kinds give Inkscape the same C, M, Y and K; they differ only in what its Fix button and its CMYK sliders compute.

Purple Pen's converter is a table sampled from `src/PurplePen/USWebCoatedSWOP.icc`, Adobe's "U.S. Web Coated (SWOP)
v2" (557,168 bytes, "Copyright 2000 Adobe Systems, Inc."), which the WinForms build copies to its output and the old
installer shipped; the Avalonia application ships only the table, embedded in PurplePenCore. Adobe's colour profile
licence lets the profiles be used, embedded in image files and bundled with application software, but not modified
([text](https://scancode-licensedb.aboutcode.org/adobe-color-profile-bundling.html)). Whether a table sampled from its
conversion, as `swopsamples.dat` and the grid profiles are, counts as a modification is open. The device CMYK profile
owes nothing to it.

## Proposal

Every option gives the profile an id and an ICC profile, which is what Inkscape needs:
`<color-profile id="cmyk" name="cmyk" xlink:href="data:application/vnd.iccprofile;base64,..."/>` holding the 516-byte
device CMYK profile, 751 bytes in the document. The name stays `cmyk`. This changes COL-3, whose document "embeds or
references no ICC profile", and gives F-3's `color-profile` an `id` and an `xlink:href`. The colour table (M-7) is
unchanged.

1. **The profile alone.** The classes stay as they are. Inkscape shows C, M, Y and K; Scribus still draws every item
   black, so F-6 stays unmet for Scribus; Chromium draws as now. +751 bytes: +10 % on the 7.5 KB test page, +1.4 % on
   mapexchange1, nothing measurable on the maps.
2. **The profile, and the styles as style attributes on groups (recommended).** Each style becomes a `style` attribute
   on the innermost group whose elements all share it, otherwise on the element, and the `<style>` sheet keeps only
   `@font-face` and `text`. A paint keeps its two declarations, `fill:#ab499c;fill:#ab499c icc-color(cmyk,.35,.85,0,0)`.
   This changes S-4 (styles are no longer classes, and are shared by sitting on a group; elements carry a style
   attribute where their group does not), COL-1 (the pair is in a style attribute), COL-3 and F-3 as in 1; S-8's group
   tidying would treat a style as a group's attribute. Scribus reads every colour as CMYK and every stroke with its
   width, but not blending or miter limits; Inkscape shows C, M, Y and K, on the group the style sits on, with its
   elements unset until the group is ungrouped, which moves the style onto them; Chromium draws as now. Size, plain:
   −13.7 % on Lincoln Park, −3.6 % on printarea2, −2.5 % on mapexchange1, +15 % on the test page; gzipped, −0.4 %
   to +10 %. Classes moved onto the same groups would be 1.5 % to 8.5 % smaller than style attributes, and under 1 %
   gzipped: that is what Scribus costs.
3. **The profile, and style attributes as an export choice.** The export writes option 1's document, and option 2's
   when asked to, for Scribus. This changes COL-3 and F-3 as in 1, and S-4 only for the choice, which needs a setting,
   its own baselines and a word in the dialog. Scribus reads only documents written with the choice.

Classes with every declaration repeated as presentation attributes beside them also read in all three (class-and-attr),
but cost +19 % to +121 % plain, more than option 2, and keep each style twice.

Option 2 is recommended: one document meets F-6 in both programs and draws in Chromium as now, and map-sized exports
come out smaller than today's. For the profile, the device CMYK one is recommended: it carries the map's uncalibrated
values as COL-3 means them, and owes nothing to Adobe's licence. The grid-5 profile sampled from `swopsamples.dat`
(4,176 bytes, 5.6 KB in the document) would let Inkscape's Fix button and CMYK sliders compute colours near Purple
Pen's (4.5 levels apart on average, against 35 for device CMYK), if a table sampled from Adobe's profile may be shipped.

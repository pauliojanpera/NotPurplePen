# Overlapping hatchings

@Pauli's case of Nov 12, for task 2 of [the handover](../SvgTargetProgress.md): where two areas of one symbol overlap,
their horizontal hatching looks thicker and darker. @Pauli saw it in Chrome's drawing of NotPurplePen's SVG and PDF
exports, in a PDF viewer and in OpenOrienteering Mapper's own drawing on screen, with the export's pattern fills and
with "Draw area patterns as separate shapes" alike, and in NotPurplePen's PNG export at 100 dpi, though not at the
higher resolution he exported at first. He agreed that his files go into this repository (Nov 12).

`case/` holds:

- `suo.ocd`: the map, with three overlapping marsh areas of symbol 308.0 (Suo), georeferenced in ETRS-TM35FIN
  (EPSG:3067);
- `suo.ppen`: its event, whose page of all controls prints at 1:10,000;
- `suo-notpurplepen-export.svg`: that page as NotPurplePen's SVG export wrote it, with "Draw area patterns as separate
  shapes";
- `suo-notpurplepen-export.pdf`: that page as NotPurplePen's PDF export wrote it;
- `suo2.ocd`: a second map, in which overlapping rectangular marsh areas, none of them turned, show the same, so the
  artifact does not depend on the areas' rotation (@Pauli, Nov 12).

## The investigation

Task 2's first step found out whether the map code can draw a pattern once for all the areas of a symbol that share
its frame in a colour's run, for every output, and how the SVG would carry it; its findings, its measurements and the
choice put to @Pauli are the section "Overlapping patterns, and a colour's layer" of
[`SvgExportDesign.md`](../SvgExportDesign.md). The prototype is `proto/overlapping-patterns` (00983b4), rooted at
upstream's 98c168b, and its run of the Windows suite was `experiment/overlapping-patterns-record-all`.

| Script | Does |
| --- | --- |
| `run-all.sh <clone> <work folder>` | Repeats every measurement below on Linux, from a checkout of the prototype it makes with `../SvgDevLoop/app-checkout.sh`, into a work folder of its own: the groups of overlapping areas, the union's clip against the Skia target's own, the case drawn before and with the change, the ink of the overlaps at seven resolutions, the time of teanaway's drawing, the browsers', Inkscape's and Scribus's unions, and the colours below full opacity. About 15 minutes. |
| `harness/` | A console program built against the prototype's `MapModel` and `Map_SkiaStd` (`-p:CheckoutRoot=<checkout>`). `info <map>` lists, for each area symbol with hatchings or patterns, its areas, the frames they lay their patterns out in and the groups whose bounding boxes overlap or touch in one frame, and how much more the groups' boxes cover than their areas'. `render <map> <png> <pixels a mm> <pattern brushes 0/1> [left top right bottom]` draws a map with the Skia target, anti-aliased on white. `time <map> <pixels a mm> <repeats> <pattern brushes 0/1>` draws it whole again and again and prints the times. `clips <map> <pixels a mm>` checks, for each area of a map, that the prototype's union of the area alone clips to the very pixels the Skia target's own clip to the area does. `PURPLEPEN_PATTERNS_ONCE=0` draws each area's patterns with the area, as before the change, and `PURPLEPEN_UNION_CLIP=pathops` unites the clip's paths with Skia's path operations instead of regions. |
| `overlap-ink.py <folder> <pixels a mm at 1 dpi> <dpi>...` | Measures, at each resolution, how much more ink the overlaps carry drawn as before than drawn once. |
| `browser-union.mjs <folder>` | Writes test documents that fill or hatch areas that overlap, touch, or touch in strips of 42 and 43 slices and a grid of 144 triangles: merged into one path, under a clip path of their union, under one of `<use>`s of ghosts (unpainted, hidden or transparent), or each under a clip of its own; and draws them in Chromium, Firefox and WebKit as the tests start them. |
| `compare-union.py <folder> [--readers]` | Measures each drawing against the merged path's inside the areas, away from their outline: seams lighter, doubled patterns darker. With `--readers`, Inkscape's and Scribus's drawings. |
| `readers-union.sh <folder>` | Draws two layouts of `browser-union.mjs` with Inkscape and Scribus, and lists how they read the ghosts. |
| `moved-baselines.py <clone> <ref> <artifact folder>...` | Measures each rendering a Tests run left of a failed comparison (a `rendering-differences-<group>` artifact, fetched with `../SvgDevLoop/fetch-artifact.sh`) against its baseline in a commit, such as `origin/base`. |
| `ocad-colour-opacity.py <file.ocd>...` | Lists the colours of OCAD files whose opacity, the colour string's `t`, is below 100. |

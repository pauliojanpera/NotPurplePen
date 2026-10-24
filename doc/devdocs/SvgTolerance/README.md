# Evaluating the pixel tolerance (F-1)

Scripts that measured the rule `PixelTolerance` holds, and chose its edge criterion: a pixel outside the band at an
edge passes when the blurred images differ there by at most 40 levels (`PixelTolerance.Default`), or 80 in a drawing
that draws text (`PixelTolerance.Text`), and at most 0.4 of the contrast around it. They need Python 3 with NumPy,
SciPy and Pillow, and are run with `python3 -I`.

## Data

A folder holding `references/<case>.png` and `renderings/<case>.png` for each drawn case: a checkout of
`svg-results/svg-target`, with the recorded references copied in from `src/MapModel/TestFiles/svg_reference`, and
Chromium's images of the cases that passed rendered with `../SvgGallery/render-cases.mjs`, which matches the
loop's renderings pixel for pixel. For a case that draws text it matches them only when `FONTCONFIG_FILE` names a
fontconfig file that offers the sentinel font alone, as `ChromiumRenderer` writes one: a `<dir>` holding a copy of
`Map_Svg.Tests/fonts/Sentinel.ttf`, and a `<cachedir>`.

## Scripts

- `collect.py <data> <out> [worker] [workers]` measures, for each case's rendering and for copies of it with errors
  planted in them (shifted 2 px, features 2 px wider, smallest solid feature removed, ink 6 levels redder), the
  pixels outside the band with no edge within reach, and for each pixel outside the band at an edge its blurred
  difference, signed and not, and the contrast around it. It writes one `.npz` file a case, and can be run as
  several workers.
- `evaluate.py <collected>` compares edge rules on those measurements: every real rendering must pass, and each
  rule's catches of the planted errors are set against the current rule's.
- `sweep.py <collected>` sweeps rules of share, cap and floor, and lists for each that passes the real renderings
  what it catches and which removals it passes; `shares.py <collected>` sets each case's planted errors beside its
  real rendering, by their largest share of the contrast at an edge and the levels there.
- `direction.py <collected> [cases]` evaluates rules that allow a rendering more where it is darker than its
  reference than where it is lighter.
- `check.py <collected> <share>` checks one share of the contrast; `margins.py <collected>` finds the smallest share
  that passes every real rendering and the largest that still fails the faint cases shifted.
- `measure.py <data>` and `excess.py <data>` report, over the real renderings, the blurred differences and contrasts
  at rescued pixels, the largest band excess and the largest channel difference.
- `synthetic.py` checks the grey-level images of `PixelToleranceTests` against the rule.

## Last results

Over the 106 drawn cases of loop run 39985693403, the 101 of phase 2 and the five that draw bitmaps: every
rendering passes, and the rule fails all 106 shifted by two pixels, all 106 with features two pixels wider, 96 of the
97 with a solid feature removed (irregular3 passes), and 93 of the 105 with ink to recolor with their ink six levels
redder; the other 12 hold their ink in strokes and dots too thin to have an inside. The five bitmap cases fail with
every planted error. Pixels outside the band at an edge occur in 57 cases, template and template2 among them; there
the blurred images differ by at most 34.6 levels, and by at most 0.36 of the contrast (multisymonedash). Run them
again whenever a phase brings new cases.

Over the 81 cases that draw text, rendered on Oct 24 from `feature/svg-text` with the sentinel font, no pixel lies
outside the band away from an edge, but at the edges of glyphs, and of the frames drawn around them, the blurred
images differ by up to 75 levels (frametext3), against 34.6 before: Chromium shades a glyph's edges otherwise than
Skia does, a whole glyph at a time. The share of the contrast stays at most 0.30 there, and reaches 0.37 only where
the contrast is faint (teanwest, 11 levels). The differences are not of one sign: at plain glyphs Chromium is
darker, but at framed text up to 75 levels lighter too, so a rule that allows darker more than lighter (direction.py)
fails nine of the real renderings. Text's edge levels are 80, with the default's share: every text rendering passes,
and the rule fails all 81 shifted by two pixels and all 81 with features two pixels wider, 63 of the 80 with a solid
feature removed and 62 of the 81 with their ink six levels redder. Every rule of this form that passes the 81 passes
15 of those removals, whose blurred differences are no larger than real ones; the other two fail only under a share
of 0.38 (Text), a hundredth above teanwest's real 0.37, or below 0.34 with a floor of 18 levels or more (combospace),
no margin at all over lksamm3 and its kin, whose real renderings reach 0.35 of the contrast at 18 levels.

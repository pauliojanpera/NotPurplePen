# Evaluating the pixel tolerance (F-1)

Scripts that measured the rule `PixelTolerance` holds, and chose its edge criterion: a pixel outside the band at an
edge passes when the blurred images differ there by at most 40 levels and at most 0.4 of the contrast around it.
They need Python 3 with NumPy, SciPy and Pillow, and are run with `python3 -I`.

## Data

A folder holding `references/<case>.png` and `renderings/<case>.png` for each drawn case: a checkout of
`svg-results/svg-target`, with the recorded references copied in from `src/MapModel/TestFiles/svg_reference`, and
Chromium's images of the cases that passed rendered with `../SvgGallery/render-cases.mjs`, which matches the
loop's renderings pixel for pixel.

## Scripts

- `collect.py <data> <out> [worker] [workers]` measures, for each case's rendering and for copies of it with errors
  planted in them (shifted 2 px, features 2 px wider, smallest solid feature removed, ink 6 levels redder), the
  pixels outside the band with no edge within reach, and for each pixel outside the band at an edge its blurred
  difference and the contrast around it. It writes one `.npz` file a case, and can be run as several workers.
- `evaluate.py <collected>` compares edge rules on those measurements: every real rendering must pass, and each
  rule's catches of the planted errors are set against the current rule's.
- `check.py <collected> <share>` checks one share of the contrast; `margins.py <collected>` finds the smallest share
  that passes every real rendering and the largest that still fails the faint cases shifted.
- `measure.py <data>` and `excess.py <data>` report, over the real renderings, the blurred differences and contrasts
  at rescued pixels, the largest band excess and the largest channel difference.
- `synthetic.py` checks the grey-level images of `PixelToleranceTests` against the rule.

## Last results

Over the 101 drawn cases of loop run 39820482035: every rendering passes, and the rule fails all 101 shifted by
two pixels, all 101 with features two pixels wider, 91 of the 92 with a solid feature removed (irregular3 passes),
and 88 of the 100 with ink to recolor with their ink six levels redder; the other 12 hold their ink in strokes and
dots too thin to have an inside. Run them again whenever a phase brings new cases, such as text or bitmaps.

# Baselines of the SVG export

`CourseSvgBaselineTests` in `Map_Svg.Tests` exports each event named there through the application's SVG export and
compares every file with the baseline here, `<event>/<file>.svg.txt`, so that any change to what the export writes
shows in review (V-2). The renderings of the SVG tests' comparison decide whether a drawing is right; these show what
changed in the text.

A baseline is the file's text with the data of each embedded font and bitmap replaced by the SHA-256 hash of its base64
text, `data:font/ttf;base64,sha256:...`, which keeps it small and still changes when the data does. It is not an SVG a
browser can draw.

The tests draw text in a fixed set of fonts and fix the culture, so a baseline is the same on every platform: the SVG
loop runs the tests on Linux, and the Windows test job runs them too, against the same baselines (V-4).

When the export changes on purpose, the test fails, and the run's results hold each new or changed baseline under
`svg_export/<event>/`: on the SVG loop's results branch, or locally in the folder `SVG_LOOP_RESULTS` names. Check the
change, copy the files here and commit them with the change that moved them.

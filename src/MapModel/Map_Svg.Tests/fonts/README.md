# The fonts of the SVG tests

The SVG tests draw text in these fonts, whatever fonts the machine has, so that a text case's reference image comes
out the same on every machine and does not move when the runner's font packages change.

- `Liberation*.ttf`: Liberation Sans and Liberation Serif 2.1.5, under the SIL Open Font License in `OFL.txt`. They
  have the metrics of Arial and Times New Roman, and the tests register them under those names.
- Roboto and Roboto Condensed come from `Map_Skia.Tests/fonts`.

Every other family the test maps name falls back to Arial, as the Skia target falls back to its default family.

- `Sentinel.ttf`: the only font of the system's that the tests' Chromium sees on Linux, made for these tests and
  released to the public domain (CC0 1.0). Every glyph it draws is a filled box, so that a character a browser would
  draw in a font of the machine's, rather than in the faces the document embeds, shows as a box and fails.

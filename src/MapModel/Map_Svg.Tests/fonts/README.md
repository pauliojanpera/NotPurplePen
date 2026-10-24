# The fonts of the SVG tests

The SVG tests draw text in these fonts, whatever fonts the machine has, so that a text case's reference image comes
out the same on every machine and does not move when the runner's font packages change.

- `Liberation*.ttf`: Liberation Sans and Liberation Serif 2.1.5, under the SIL Open Font License in `OFL.txt`. They
  have the metrics of Arial and Times New Roman, and the tests register them under those names.
- Roboto and Roboto Condensed come from `Map_Skia.Tests/fonts`.

Every other family the test maps name falls back to Arial, as the Skia target falls back to its default family.

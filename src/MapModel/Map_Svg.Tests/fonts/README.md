# The fonts of the SVG tests

The SVG tests draw text in these fonts alone, whatever fonts the machine has, so that a text case's reference image
and every document come out the same on every machine, Windows or Linux, and do not move when the runner's font
packages change. `TestFonts` registers them and makes them a fixed font set (`SkiaFontManager.RegisteredFontsOnly`):
no font of the machine's is looked up, for a family or for a character a face lacks.

- `Liberation*.ttf`: Liberation Sans and Liberation Serif 2.1.5, under the SIL Open Font License in `OFL.txt`. They
  have the metrics of Arial and Times New Roman, and the tests register them under those names.
- `Caladea-*.ttf` and `Carlito-*.ttf`: Caladea 1.001 and Carlito 1.104, under the SIL Open Font License in
  `Caladea-OFL.txt` and `Carlito-OFL.txt`. They have the metrics of Cambria and Calibri, and the tests register them
  under those names: a machine's fontconfig maps those two families to them where they are installed, and to nothing
  where they are not, so unregistered they drew the test maps in different faces on different machines.
- Roboto and Roboto Condensed come from `Map_Skia.Tests/fonts`.

Every other family the test maps name falls back to Arial, as the Skia target falls back to its default family.

- `fallback/`: the faces the Skia target falls back to for a character a text's face lacks. The fixed set looks for
  such a character in the faces above first and then in these, in the order of their files' names, and never in a face
  of the machine's, so that fontfallback draws the same on every machine; the tests' Chromium likewise sees the
  sentinel alone. They are Noto faces under the SIL Open Font License in `fallback/OFL.txt`, cut down to the
  characters in `fallback/characters.txt`, the characters the test maps draw that no face above has;
  `fallback/make-fallback-fonts.py` fetches them at pinned commits and makes them anew, and says how to add
  characters. A character none of them has is drawn as a box.

- `variable/RobotoVariable-Test.ttf`: Roboto's variable face, with its axes of weight and width, cut down to the
  printable characters of ASCII, under the SIL Open Font License in `variable/OFL.txt`. The tests register it as
  Roboto Variable, regular and bold, which, loaded from its file, draws its default instance whichever is asked for:
  the subsetter must make it static there, where a browser would draw the bold heavier.
  `variable/make-variable-font.py` fetches it at a pinned commit and makes it anew.

- `Sentinel.ttf`: the only font of the system's that the tests' Chromium sees on Linux, made for these tests and
  released to the public domain (CC0 1.0). Every glyph it draws is a filled box, so that a character a browser would
  draw in a font of the machine's, rather than in the faces the document embeds, shows as a box and fails.

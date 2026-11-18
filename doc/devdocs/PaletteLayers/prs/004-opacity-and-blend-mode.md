## General description
A palette entry (`SymColor`) gets an opacity, from 0 to 1, and a blend mode: Normal, Darken or Multiply, the blend
modes that OCAD knows. They are apart from the CMYK value of the color, the alpha of that value, and the overprint
flag. The OCAD and Mapper imports read them, and the exports write them. The drawing does not use them yet.

Before, the OCAD import skipped the transparency and the blend mode of a color. The Mapper import read the opacity of
a color and did not keep it, and the Mapper export wrote the alpha of the CMYK value, which is always 1, as the
opacity.

## Changes in detail
* `ColorBlendMode`, and `SymColor.Opacity` and `SymColor.BlendMode`, with the defaults 1 and Normal. `CopyFrom`
  copies them, so `Map.Clone` and the undo of the color table keep them. `SymColor.BoundOpacity` bounds an opacity
  that a file gives to the range from 0 to 1.
* OCAD 9 and later: the import reads the transparency (`t`) of a color string as the opacity in percent, and the
  blend mode (`b`) by its name (`OcadBlendMode`). A missing field is 100 or Normal, and an unknown name is Normal. The
  export writes `t` in each color string, in whole percents, and `b` from OCAD 12 on, after the overprint flag (`o`),
  as OCAD writes them.
* OCAD 6 to 8 have neither. The import gives the defaults, and the export writes nothing of them.
* Mapper: the import reads the `opacity` attribute of a color, and a missing one is 1. The export writes the opacity.
  Mapper has no blend mode.
* `OpacityAndBlendModeTests` reads the transparency of `LordHill` (OCAD 9 and 11), the blend modes of
  `wholestructure` (OCAD 12) and of the ISSprOM 2019 symbol set (OCAD 2018), and the defaults of OCAD 6 to 8 maps. It
  checks the color strings that the OCAD export writes, the `opacity` attribute of the Mapper export, and a round trip
  through each format. No test draws.
* `MapFileTestUtil` holds the helpers of `InkTests` that read and write maps, for both test classes.
* The expected dumps of the OCAD 9 and 11 course files of the `CreateOcadFiles` tests in `ControllerTests` (30 files in
  `controller/ocad_create2`, `8`, `9` and `10`) have the transparency `t100` in each color. Nothing else in them
  changes, and no image baseline moves.

## Issue ticket number
[004-opacity-and-blend-mode](../tickets/004-opacity-and-blend-mode.md)

## Missing functionality
* The drawing does not use the opacity or the blend mode yet. To composite the colors by palette entry is a separate
  ticket.
* No test map is a Mapper map that Mapper saved with an opacity below 1. The tests make such a color and write it.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.

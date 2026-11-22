## General description
The Mapper export gives each color the number of its place in the color list of the file. Mapper finds the color
of a number at that place, for the color of a symbol and for the spot color of a color composition. The export gave
each color the number of its place in the map. When the top color is registration black, which the export does not
write, each number was one more than the place of its color, and the last number pointed past the list.

## Changes in detail
* `OpenMapperExport.WriteColorsElement` counts the colors that it writes and gives each color the count as its number.
* `OpenMapperImportTests.ExportNumbersColorsByPlace` writes `teanaway.omap` and checks that the number of each color is
  its place in the file, and that the color of each line symbol is the color of its line symdef. The test fails
  without the change.

## Issue ticket number
[002-mapper-export-color-numbers](../tickets/002-mapper-export-color-numbers.md)

## Missing functionality
None.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia. Vika koskee vain karttaa, jonka ylin väri on rekisterimusta, kuten Mapper-kartan
tuonnin jälkeen. Sovelluksen ratatiedostoissa ylin väri on radan väri.

## General description
The map model gets inks. An ink is a printing colorant: a name, the CMYK value of its appearance, and a halftone
screen where one is defined. The process inks cyan, magenta, yellow and black always exist, and a map adds its spot
inks. A color is made of inks. A color without a spot definition is the four process inks, with its CMYK values as
factors. A color with a spot definition is a spot ink as such, at full tone, or a composition of spot inks with
factors from 0 to 1. A missing component is not the same as a component with the factor 0. The CMYK value of a color
does not change.

The OCAD and Mapper imports read the spot inks and the components, and the exports write them. Before, the imports
did not keep them, and the exports wrote none.

The Mapper import made each color without a `spotcolors` element overprint. Mapper applies knockout only to a color
that has a spot definition, and draws each other color as a knockout. The import now makes a color overprint only
when the color has a spot definition and its knockout is off. Nothing else in the drawing changes.

## Changes in detail
* `Ink`, `InkScreen` and `InkComponent`, in the new file `Ink.cs`.
* `SymColor`: `InkComponents`, `HasSpotDefinition`, `SpotInk`, `SetSpotInk`, `SetSpotComposition`, and a constructor
  that takes the name, the OCAD id, the CMYK value and the overprint flag.
* `Map`: `AllInks`, `AddSpotInk` and `RemoveSpotInk`, with undo; `AddColor` and `AddColorAtIndex` that take a
  `SymColor`; `IsColorUsed`. `Clone` with `CloneColors` copies the inks and the spot definitions.
* OCAD 9 and later: the spot color strings (type 10) are spot inks, with the frequency in tenths of lines per inch
  and the angle in degrees. The `s` and `p` pairs of a color string are its components.
* OCAD 6 to 8: the separations of the symbol header, at most 24, are spot inks. The shares of a color are its
  components: 0 to 200 is a factor in half percents, and 255 is no component. The symbol header that the map keeps for
  round trips no longer holds the separations.
* Mapper: a `namedcolor` is a spot ink, with `screen_frequency` and `screen_angle`. A `component` element is a
  component, and a component whose spot color is not a spot color as such is left out, as Mapper leaves it out.
* Between the formats, as Mapper does: the OCAD export writes a spot ink as such that is below all other colors and
  that no symbol uses as a separation only, and the Mapper export gives a spot color to each spot ink that no color is.
* `InkTests` reads the Hakola test map in Mapper, OCAD 12 and OCAD 8 formats, writes it in each format and between the
  formats, and checks that two colors that are not consecutive share one ink object. `MapperOverprinting` in
  `Map_Skia.Tests` draws the Mapper map with the overprint effect. The test maps have no text.
* No existing baseline moves. `skia_render/hakolaoverprinting.png` is the baseline of the new Skia test.
* The branch includes `fix/mapper-export-color-numbers`, which gives the colors of a Mapper export the numbers that the
  components refer to.

## Issue ticket number
[001-inks](../tickets/001-inks.md), with [002-mapper-export-color-numbers](../tickets/002-mapper-export-color-numbers.md)

## Missing functionality
* The drawing does not composite colors by their inks yet. That is a separate ticket.
* The import does not read the opacity of a color.
* The process inks have no halftone screens. OCAD 6 to 8 files keep the screens of their process inks in the symbol
  header.
* The names outside ASCII in the OCAD 12 test map read wrong until [003-ocad11-utf8-strings](../tickets/003-ocad11-utf8-strings.md).
  The tests find the colors by names in ASCII.

## Changes to be logged
Mapper-karttojen päällepainatusvaikutus ei enää sekoita värejä, joilla ei ole spottimäärittelyä: ne peittävät alla
olevan kuten Mapperissa.

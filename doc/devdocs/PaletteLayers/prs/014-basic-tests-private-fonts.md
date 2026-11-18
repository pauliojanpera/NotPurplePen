## General description
The font matching tests of `BasicTests` clear the private fonts, and the static constructor of `RenderingTests`
registers its Roboto font files once. So the rendering tests found Roboto only when that constructor ran after
`BasicTests`. When it ran before, the drawings used Arial in place of Roboto on a machine without Roboto, and
`RenderingTests.Roboto` failed. The tear-down of `BasicTests` now registers the font files of the rendering tests
again after each test.

## Changes in detail
* `RenderingTests.AddFontFiles` registers the Roboto font files as private fonts. The static constructor of
  `RenderingTests` calls it.
* `BasicTests.RegisterFontsOfTheRenderingTests`, the tear-down of each test of `BasicTests`, calls it.
  `SkiaFontManager.AddFontFile` leaves a font that is registered already as it is.
* Without the change, `RenderingTests.Roboto` fails on a machine without Roboto when the static constructor of
  `RenderingTests` runs before `BasicTests`, as it does when a source of test cases runs it while NUnit makes the
  tests. With the change, the same set of tests passes.

## Issue ticket number
[014-basic-tests-private-fonts](../tickets/014-basic-tests-private-fonts.md)

## Missing functionality
None.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.

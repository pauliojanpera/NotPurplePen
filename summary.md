SVG loop: 318 passed, 0 failed, 5 pending

Commit 3a30e5ef840ff17b89bf5fb7fcaf59bd46d0d9c1, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/40120104427. Results are on svg-results/svg-subsetter.

## Since the previous run

Compared with the results svg-results/svg-subsetter held before: SVG loop run 40118293953 of 6c43f16c3deba95236c9e6e40df6a701efe4b96b. The SVGs come to 99,605,397 bytes, against 99,599,685 before.

85 tests or drawings changed; the first 30 are below, and changes.md lists each.

| Test or drawing | What changed |
| --- | --- |
| ADocumentThatDrawsTextNamesItsLanguageAsUndetermined | new test, Passed |
| ABrowserInARomanianLocaleDrawsTheTextInTheFormsTheApplicationDraws | new test, Passed |
| aiimport | SVG 14,825 -> 14,840 bytes |
| aiimport_light | SVG 14,831 -> 14,846 bytes |
| alllinetext | SVG 10,321 -> 10,336 bytes |
| arialnarrow | SVG 18,849 -> 18,864 bytes |
| Cambria | SVG 4,566 -> 4,581 bytes |
| Cambriabold | SVG 4,564 -> 4,579 bytes |
| charspace | SVG 62,572 -> 62,587 bytes |
| combospace | SVG 58,980 -> 58,995 bytes |
| dashline | SVG 8,886 -> 8,901 bytes |
| dashline9 | SVG 8,887 -> 8,902 bytes |
| differentnewlinetypes | SVG 24,256 -> 24,271 bytes |
| fences | SVG 14,515 -> 14,530 bytes |
| fences9 | SVG 14,516 -> 14,531 bytes |
| fontfallback | SVG 266,630 -> 266,645 bytes |
| frametext1 | SVG 42,485 -> 42,500 bytes |
| frametext2 | SVG 52,706 -> 52,721 bytes |
| frametext3 | SVG 30,806 -> 30,821 bytes |
| framing_ocad6 | SVG 6,449 -> 6,464 bytes |
| framing_ocad7 | SVG 10,559 -> 10,574 bytes |
| framing_ocad8 | SVG 7,714 -> 7,729 bytes |
| FtCasey | SVG 53,877 -> 53,892 bytes |
| hiddensymbols | SVG 4,744 -> 4,759 bytes |
| hiddensymbols9 | SVG 4,745 -> 4,760 bytes |
| isompoints | SVG 18,549 -> 18,564 bytes |
| isompoints_9 | SVG 18,551 -> 18,566 bytes |
| isompoints_light | SVG 18,678 -> 18,693 bytes |
| justify | SVG 19,873 -> 19,888 bytes |
| kern_text_outline | SVG 15,258 -> 15,273 bytes |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 2 |
| Checkout source | success | 44 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 8 |
| Cache NuGet packages | success | 9 |
| Build | success | 18 |
| Cache the Playwright browser | success | 2 |
| Install Chromium | success | 23 |

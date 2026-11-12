SVG loop: 783 passed, 0 failed, 2 pending, 2 of them on calls the target never honors, 15 set apart for a browser

Commit 22ac16dbff564a1057957e98080652db4c4778b3, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/43193401818. Results are on svg-results/svg-browsers.

## Since the previous run

Compared with the results svg-results/svg-browsers held before: SVG loop run 43064709418 of 2336b7acb0b0b8ec40c08c71bccc0c3c56839bf5. The SVGs come to 32,357,795 bytes, against 32,357,795 before.

No test changed its outcome, and no SVG or measurement changed.

## Geometry (geometry/)

The geometry check read 142,980 drawings of 193 cases back from their SVGs. No point or length lies further from where its call puts it than 1.362 steps (simpletext_light), and none takes more than 0.662 of the allowance its rounding gives it (lksamm12_4).

## Firefox and WebKit (browsers/)

Each document is rendered in the browser and compared with Chromium's image of it, within the tolerance of the rendering comparison.

| Browser | Documents | Match Chromium | Set apart | Failed | Pending |
| --- | ---: | ---: | ---: | ---: | ---: |
| Firefox | 205 | 202 | 3 | 0 | 0 |
| WebKit | 205 | 193 | 12 | 0 | 0 |

Set apart, each still compared, and failing once it matches or differs more:

| Browser | Document | Pixels outside the tolerance | Why |
| --- | --- | ---: | --- |
| Firefox | fontfallback | 1 | F-5: Firefox draws the lower part of a cluster of Bengali, below the headline, a pixel lower than Chromium |
| Firefox | teanwest | 31026 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
| Firefox | teanwest11 | 31019 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
| WebKit | Bitmap | 4 | F-5: WebKit draws a bitmap scaled down to a fraction of its size without smoothing it, whatever its image-rendering |
| WebKit | fontfallback | 829 | F-5: WebKit draws nothing of a text whose characters are all Runic, in any face and in SVG alone; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | frametext1 | 44 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | frametext3 | 35 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | hiddensymbols | 7 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | hiddensymbols9 | 7 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | ocad11templatealign | 95575 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels |
| WebKit | page-LincolnPark-Short | 6 | F-5: WebKit draws a bitmap scaled down to a fraction of its size without smoothing it, whatever its image-rendering |
| WebKit | page-printarea2-Course1 | 2 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | PenistoneHill | 13 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | teanwest | 133 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | teanwest11 | 133 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |

## Sizes of the reference exports (export-sizes.tsv)

The 23 files the SVG export writes of the reference events come to 6,426,906 bytes, 3,207,077 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 105,426 | 19,864 | the same |
| mapexchange1 | Course 1.svg | 13,422 | 5,402 | the same |
| mapexchange1 | Course 2-1.svg | 5,673 | 2,550 | the same |
| mapexchange1 | Course 2-2.svg | 6,335 | 2,833 | the same |
| mapexchange1 | Course 3.svg | 15,268 | 5,782 | the same |
| mapexchange1 | Course 4B.svg | 15,525 | 5,834 | the same |
| mapexchange1 | Course 4G.svg | 14,398 | 5,591 | the same |
| mapexchange1 | Course 5-1.svg | 13,533 | 5,303 | the same |
| mapexchange1 | Course 5-2.svg | 9,255 | 3,832 | the same |
| mapexchange1 | Course 5-3.svg | 7,426 | 3,218 | the same |
| mapexchange1 | Course 5-4.svg | 9,117 | 3,709 | the same |
| Overprint test | All controls.svg | 9,123 | 3,753 | the same |
| Overprint test | Course 12.svg | 8,882 | 3,418 | the same |
| Overprint test lower purple | All controls.svg | 9,173 | 3,773 | the same |
| Overprint test lower purple | Course 12.svg | 8,932 | 3,437 | the same |
| Lincoln Park | All controls.svg | 1,705,347 | 1,266,613 | the same |
| Lincoln Park | Short.svg | 1,705,293 | 1,266,453 | the same |
| printarea2 | Course 1.svg | 930,365 | 297,724 | the same |
| Madrona Permanent11 | Course 1.svg | 329,501 | 106,932 | the same |
| testproj2 | All controls.svg | 9,186 | 4,106 | the same |
| Overprint test lower purple in one file | Overprint test.svg | 13,523 | 4,719 | the same |
| mapexchange1 Course 5 in a file | Course 5.svg | 26,630 | 6,930 | the same |
| Madrona Permanent11 with area patterns as separate shapes | Course 1.svg | 1,455,573 | 175,301 | the same |

## Pending cases by the calls they need (2 cases; pending.tsv lists each)

2 of them wait only on calls the target will never honor, and will never be compared.

| Call | Never honored | Cases that need it | Cases that need only it |
| --- | --- | ---: | ---: |
| DrawPolyline | D-5, a line of zero width, which the SVG leaves out where Skia draws a hairline | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 35 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 7 |
| Cache NuGet packages | success | 12 |
| Build | success | 24 |
| Cache the Playwright browsers | success | 6 |
| Install the browsers | success | 45 |
| Test | success | 735 |

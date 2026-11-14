SVG loop: 868 passed, 0 failed, 2 pending, 2 of them on calls the target never honors, 16 set apart for a browser

Commit 9b80d6d60d8c5f23bb152ebe4c9e4217d7ac0483, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/43563468407. Results are on svg-results/svg-jpeg-data.

## Since the previous run

svg-results/svg-jpeg-data held no results before this run.

## Geometry (geometry/)

The geometry check read 143,614 drawings of 193 cases back from their SVGs. No point or length lies further from where its call puts it than 1.362 steps (simpletext_light), and none takes more than 0.662 of the allowance its rounding gives it (lksamm12_4).

## Firefox and WebKit (browsers/)

Each document is rendered in the browser and compared with Chromium's image of it, within the tolerance of the rendering comparison.

| Browser | Documents | Match Chromium | Set apart | Failed | Pending |
| --- | ---: | ---: | ---: | ---: | ---: |
| Firefox | 215 | 212 | 3 | 0 | 0 |
| WebKit | 215 | 202 | 13 | 0 | 0 |

Set apart, each still compared, and failing once it matches or differs more:

| Browser | Document | Pixels outside the tolerance | Why |
| --- | --- | ---: | --- |
| Firefox | fontfallback | 1 | F-5: Firefox draws the lower part of a cluster of Bengali, below the headline, a pixel lower than Chromium |
| Firefox | teanwest | 32165 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
| Firefox | teanwest11 | 32167 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
| WebKit | Bitmap | 4 | F-5: WebKit draws a bitmap scaled down to a fraction of its size without smoothing it, whatever its image-rendering |
| WebKit | fontfallback | 829 | F-5: WebKit draws nothing of a text whose characters are all Runic, in any face and in SVG alone; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | frametext1 | 44 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | frametext3 | 35 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | hiddensymbols | 7 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | hiddensymbols9 | 7 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | marymoor11_lowerlayers | 3 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels |
| WebKit | ocad11templatealign | 95575 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels |
| WebKit | page-LincolnPark-Short | 6 | F-5: WebKit draws a bitmap scaled down to a fraction of its size without smoothing it, whatever its image-rendering |
| WebKit | page-printarea2-Course1 | 2 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | PenistoneHill | 13 | F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | teanwest | 133 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |
| WebKit | teanwest11 | 133 | F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile the document gives, and otherwise as the pattern's phase falls on the pixels; and WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering or its own settings say, while Chromium draws them unhinted |

## Sizes of the reference exports (export-sizes.tsv)

The 25 files the SVG export writes of the reference events come to 8,877,004 bytes, 4,815,674 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 105,426 | 19,864 |  |
| mapexchange1 | Course 1.svg | 13,422 | 5,402 |  |
| mapexchange1 | Course 2-1.svg | 5,673 | 2,550 |  |
| mapexchange1 | Course 2-2.svg | 6,335 | 2,833 |  |
| mapexchange1 | Course 3.svg | 15,268 | 5,782 |  |
| mapexchange1 | Course 4B.svg | 15,525 | 5,834 |  |
| mapexchange1 | Course 4G.svg | 14,398 | 5,591 |  |
| mapexchange1 | Course 5-1.svg | 13,533 | 5,303 |  |
| mapexchange1 | Course 5-2.svg | 9,255 | 3,832 |  |
| mapexchange1 | Course 5-3.svg | 7,426 | 3,218 |  |
| mapexchange1 | Course 5-4.svg | 9,117 | 3,709 |  |
| Overprint test | All controls.svg | 9,123 | 3,753 |  |
| Overprint test | Course 12.svg | 8,882 | 3,418 |  |
| Overprint test lower purple | All controls.svg | 9,173 | 3,773 |  |
| Overprint test lower purple | Course 12.svg | 8,932 | 3,437 |  |
| Lincoln Park | All controls.svg | 1,705,347 | 1,266,613 |  |
| Lincoln Park | Short.svg | 1,705,293 | 1,266,453 |  |
| JPEG data | All controls.svg | 1,223,115 | 803,915 |  |
| JPEG data | Photos.svg | 1,223,540 | 803,986 |  |
| printarea2 | Course 1.svg | 933,808 | 298,420 |  |
| Madrona Permanent11 | Course 1.svg | 329,501 | 106,932 |  |
| testproj2 | All controls.svg | 9,186 | 4,106 |  |
| Overprint test lower purple in one file | Overprint test.svg | 13,523 | 4,719 |  |
| mapexchange1 Course 5 in a file | Course 5.svg | 26,630 | 6,930 |  |
| Madrona Permanent11 with area patterns as separate shapes | Course 1.svg | 1,455,573 | 175,301 |  |

## Pending cases by the calls they need (2 cases; pending.tsv lists each)

2 of them wait only on calls the target will never honor, and will never be compared.

| Call | Never honored | Cases that need it | Cases that need only it |
| --- | --- | ---: | ---: |
| DrawPolyline | D-5, a line of zero width, which the SVG leaves out where Skia draws a hairline | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 2 |
| Checkout source | success | 41 |
| Name the results | success | 1 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 0 |
| Cache NuGet packages | success | 10 |
| Build | success | 22 |
| Cache the Playwright browsers | success | 0 |
| Install the browsers | success | 53 |
| Test | success | 476 |

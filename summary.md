SVG loop: 882 passed, 0 failed, 2 pending, 2 of them on calls the target never honors, 16 set apart for a browser

Commit 7420ac6434b32c8d079795d52279c4a4612c87a4, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/45632754426. Results are on svg-results/svg-encoding.

## Since the previous run

Compared with the results svg-results/svg-encoding held before: SVG loop run 45630366944 of 24bfbb2bcf1bfb9c10248bf7a196a215afe13467. The SVGs come to 33,150,532 bytes, against 33,146,409 before.

2 tests or drawings changed.

| Test or drawing | What changed |
| --- | --- |
| ThePlaceOfACoveragePrototypeNamesTheComponentDefinitionAtEachLevelOfACombination | new test, Passed |
| encoding-combination | new SVG, 4,123 bytes |

## Geometry (geometry/)

The geometry check read 137,911 drawings of 196 cases back from their SVGs. No point or length lies further from where its call puts it than 1.362 steps (simpletext9), and none takes more than 0.993 of the allowance its rounding gives it (LordHill).

## Firefox and WebKit (browsers/)

Each document is rendered in the browser and compared with Chromium's image of it, within the tolerance of the rendering comparison.

| Browser | Documents | Match Chromium | Set apart | Failed | Pending |
| --- | ---: | ---: | ---: | ---: | ---: |
| Firefox | 218 | 215 | 3 | 0 | 0 |
| WebKit | 218 | 205 | 13 | 0 | 0 |

Set apart, each still compared, and failing once it matches or differs more:

| Browser | Document | Pixels outside the tolerance | Why |
| --- | --- | ---: | --- |
| Firefox | fontfallback | 1 | F-5: Firefox draws the lower part of a cluster of Bengali, below the headline, a pixel lower than Chromium |
| Firefox | teanwest | 32198 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
| Firefox | teanwest11 | 32198 | F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the document gives; each lies within F-1's tolerance of the reference |
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

The 25 files the SVG export writes of the reference events come to 8,534,265 bytes, 4,811,674 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 91,578 | 17,661 | the same |
| mapexchange1 | Course 1.svg | 14,637 | 5,648 | the same |
| mapexchange1 | Course 2-1.svg | 5,726 | 2,608 | the same |
| mapexchange1 | Course 2-2.svg | 6,897 | 2,916 | the same |
| mapexchange1 | Course 3.svg | 16,707 | 6,075 | the same |
| mapexchange1 | Course 4B.svg | 17,014 | 6,123 | the same |
| mapexchange1 | Course 4G.svg | 15,775 | 5,861 | the same |
| mapexchange1 | Course 5-1.svg | 14,679 | 5,544 | the same |
| mapexchange1 | Course 5-2.svg | 10,162 | 3,998 | the same |
| mapexchange1 | Course 5-3.svg | 8,160 | 3,336 | the same |
| mapexchange1 | Course 5-4.svg | 10,076 | 3,876 | the same |
| Overprint test | All controls.svg | 9,520 | 3,856 | the same |
| Overprint test | Course 12.svg | 9,261 | 3,532 | the same |
| Overprint test lower purple | All controls.svg | 9,582 | 3,880 | the same |
| Overprint test lower purple | Course 12.svg | 9,323 | 3,556 | the same |
| Lincoln Park | All controls.svg | 1,705,439 | 1,266,659 | the same |
| Lincoln Park | Short.svg | 1,705,349 | 1,266,532 | the same |
| JPEG data | All controls.svg | 1,223,434 | 803,975 | the same |
| JPEG data | Photos.svg | 1,223,873 | 804,094 | the same |
| printarea2 | Course 1.svg | 850,418 | 295,552 | the same |
| Madrona Permanent11 | Course 1.svg | 317,279 | 106,897 | the same |
| testproj2 | All controls.svg | 9,609 | 4,151 | the same |
| Overprint test lower purple in one file | Overprint test.svg | 14,035 | 4,820 | the same |
| mapexchange1 Course 5 in a file | Course 5.svg | 31,036 | 7,255 | the same |
| Madrona Permanent11 with area patterns as separate shapes | Course 1.svg | 1,204,696 | 173,269 | the same |

## Pending cases by the calls they need (2 cases; pending.tsv lists each)

2 of them wait only on calls the target will never honor, and will never be compared.

| Call | Never honored | Cases that need it | Cases that need only it |
| --- | --- | ---: | ---: |
| StrokedShape | D-5, a line of zero width, which the SVG leaves out where Skia draws a hairline | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 38 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 9 |
| Build | success | 32 |
| Cache the Playwright browsers | success | 3 |
| Install the browsers | success | 27 |
| Test | success | 888 |

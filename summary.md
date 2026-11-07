SVG loop: 381 passed, 0 failed, 5 pending, 5 of them on calls the target never honors

Commit 77cafabacc47c30a265ae91343b7212a79fa3b64, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/42321275386. Results are on svg-results/svg-loop-improvements.

## Since the previous run

Compared with the results svg-results/svg-loop-improvements held before: SVG loop run 42313692039 of 41dbd1bfa64597cf920e53cd1c5ee281768e1b20. The SVGs come to 29,499,213 bytes, against 29,499,213 before.

202 tests or drawings changed; the first 30 are below, and changes.md lists each.

| Test or drawing | What changed |
| --- | --- |
| TextChanged,"'Geometrie'" | new test, Passed |
| ArcTurnedTheOtherWay,"DrawArc, call 2" | new test, Passed |
| PolygonLeftOut,"FillRectangle, call 6 draws" | new test, Passed |
| DocumentAgreesWithItsCalls | new test, Passed |
| CurveBent,"FillPath, call 4" | new test, Passed |
| LineEndMoved,"DrawLine, call 1" | new test, Passed |
| EllipseWider,"DrawEllipse, call 3" | new test, Passed |
| ClipMoved,"PushClip" | new test, Passed |
| BitmapMoved,"DrawBitmap" | new test, Passed |
| TextMoved,"DrawText" | new test, Passed |
| TileDotMoved,"of a pattern's tile" | new test, Passed |
| PenWider,"length 1" | new test, Passed |
| aiimport | drawings compared  -> 47, largest deviation in steps  -> 0.827, largest share of an allowance  -> 0.418 |
| aiimport_light | drawings compared  -> 47, largest deviation in steps  -> 0.827, largest share of an allowance  -> 0.418 |
| alllinetext | drawings compared  -> 130, largest deviation in steps  -> 1.169, largest share of an allowance  -> 0.617 |
| angledashes9 | drawings compared  -> 858, largest deviation in steps  -> 0.708, largest share of an allowance  -> 0.415 |
| arialnarrow | drawings compared  -> 65, largest deviation in steps  -> 0.661, largest share of an allowance  -> 0.311 |
| badpointyends | drawings compared  -> 4, largest deviation in steps  -> 0.855, largest share of an allowance  -> 0.501 |
| Bitmap | drawings compared  -> 3, largest deviation in steps  -> 0.036, largest share of an allowance  -> 0.013 |
| BitmapPart | drawings compared  -> 2, largest deviation in steps  -> 0.001, largest share of an allowance  -> 0 |
| bizzarro_dash11 | drawings compared  -> 152, largest deviation in steps  -> 0.668, largest share of an allowance  -> 0.391 |
| borderedarea9 | drawings compared  -> 27, largest deviation in steps  -> 0.603, largest share of an allowance  -> 0.353 |
| Cambria | drawings compared  -> 2, largest deviation in steps  -> 0.585, largest share of an allowance  -> 0.342 |
| Cambriabold | drawings compared  -> 2, largest deviation in steps  -> 0.464, largest share of an allowance  -> 0.272 |
| charspace | drawings compared  -> 697, largest deviation in steps  -> 0.947, largest share of an allowance  -> 0.225 |
| Clipping | drawings compared  -> 4, largest deviation in steps  -> 0, largest share of an allowance  -> 0 |
| combospace | drawings compared  -> 548, largest deviation in steps  -> 1.285, largest share of an allowance  -> 0.308 |
| ComplexLines | drawings compared  -> 5, largest deviation in steps  -> 0.362, largest share of an allowance  -> 0.212 |
| corner_ends | drawings compared  -> 33, largest deviation in steps  -> 0.793, largest share of an allowance  -> 0.465 |
| corner_ends9 | drawings compared  -> 33, largest deviation in steps  -> 0.793, largest share of an allowance  -> 0.465 |

## Geometry (geometry/)

The geometry check read 140,067 drawings of 190 cases back from their SVGs. No point or length lies further from where its call puts it than 1.362 steps (simpletext), and none takes more than 0.662 of the allowance its rounding gives it (lksamm11_4).

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

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

5 of them wait only on calls the target will never honor, and will never be compared.

| Call | Never honored | Cases that need it | Cases that need only it |
| --- | --- | ---: | ---: |
| CreatePen | D-5, a hairline pen of no width, which a document has no unit for | 3 | 3 |
| DrawBitmap | B-6, a bitmap drawn lighter, which no element of a document can draw | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 39 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 13 |
| Build | success | 26 |
| Cache the Playwright browser | success | 1 |
| Install Chromium | success | 13 |

SVG loop: 386 passed, 0 failed, 2 pending, 2 of them on calls the target never honors

Commit cc462aada9ca7ef23d047ea29da22549de4244d9, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/42976324697. Results are on svg-results/svg-rule-review.

## Since the previous run

Compared with the results svg-results/svg-rule-review held before: SVG loop run 42975527993 of 366707d8f5e0b68af83fb10bdb30400d54249e5d. The SVGs come to 32,357,557 bytes, against 29,826,654 before.

6 tests or drawings changed.

| Test or drawing | What changed |
| --- | --- |
| ABitmapBelowFullIntensityIsDrawnThroughAFilterThatLightensIt | new test, Passed |
| AClippedMiterJoinIsDrawnAsAMiter | new test, Passed |
| template2_light | NotExecuted -> Passed; new SVG, 1,418,865 bytes; new measurements |
| template_light | NotExecuted -> Passed; new SVG, 1,112,038 bytes; new measurements |
| ClippedJoinsAreNotHonored | test gone, was Passed |
| ABitmapDrawnBelowFullIntensityRaisesAnErrorNamingTheCall | test gone, was Passed |

## Geometry (geometry/)

The geometry check read 142,980 drawings of 193 cases back from their SVGs. No point or length lies further from where its call puts it than 1.362 steps (simpletext_light), and none takes more than 0.662 of the allowance its rounding gives it (lksamm12_4).

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
| Checkout source | success | 28 |
| Name the results | success | 0 |
| Merge what this is built on | success | 2 |
| Setup .NET | success | 10 |
| Cache NuGet packages | success | 11 |
| Build | success | 24 |
| Cache the Playwright browser | success | 4 |
| Install Chromium | success | 16 |

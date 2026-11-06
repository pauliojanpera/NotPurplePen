SVG loop: 368 passed, 0 failed, 5 pending

Commit c4968cf15443b244d3236d030d0c3b768c9609f6, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/42284263477. Results are on svg-results/svg-symbol-reuse.

## Since the previous run

svg-results/svg-symbol-reuse held no results before this run.

## Sizes of the reference exports (export-sizes.tsv)

The 23 files the SVG export writes of the reference events come to 6,426,906 bytes, 3,207,077 gzip-compressed.

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
| printarea2 | Course 1.svg | 930,365 | 297,724 |  |
| Madrona Permanent11 | Course 1.svg | 329,501 | 106,932 |  |
| testproj2 | All controls.svg | 9,186 | 4,106 |  |
| Overprint test lower purple in one file | Overprint test.svg | 13,523 | 4,719 |  |
| mapexchange1 Course 5 in a file | Course 5.svg | 26,630 | 6,930 |  |
| Madrona Permanent11 with area patterns as separate shapes | Course 1.svg | 1,455,573 | 175,301 |  |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 32 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 9 |
| Build | success | 24 |
| Cache the Playwright browser | success | 2 |
| Install Chromium | success | 22 |
| Test | success | 275 |

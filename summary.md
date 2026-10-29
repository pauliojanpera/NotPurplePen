SVG loop: 339 passed, 0 failed, 5 pending

Commit a462ba4a7732b004bbd2a789f09373848ea042af, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/40851833019. Results are on svg-results/svg-export.

## Since the previous run

Compared with the results svg-results/svg-export held before: SVG loop run 40847995925 of bd0979baa10a2065795fd65092071b8807410550. The SVGs come to 109,887,689 bytes, against 109,887,689 before.

No test changed its outcome, and no SVG or measurement changed.

## Sizes of the reference exports (export-sizes.tsv)

The 20 files the SVG export writes of the reference events come to 7,134,602 bytes, 3,135,626 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 118,816 | 19,869 | the same |
| mapexchange1 | Course 1.svg | 19,203 | 5,439 | the same |
| mapexchange1 | Course 2-1.svg | 4,462 | 2,128 | the same |
| mapexchange1 | Course 2-2.svg | 11,911 | 2,886 | the same |
| mapexchange1 | Course 3.svg | 21,144 | 5,843 | the same |
| mapexchange1 | Course 4B.svg | 21,295 | 5,874 | the same |
| mapexchange1 | Course 4G.svg | 20,232 | 5,632 | the same |
| mapexchange1 | Course 5-1.svg | 19,378 | 5,359 | the same |
| mapexchange1 | Course 5-2.svg | 15,027 | 3,904 | the same |
| mapexchange1 | Course 5-3.svg | 13,116 | 3,307 | the same |
| mapexchange1 | Course 5-4.svg | 14,900 | 3,780 | the same |
| Overprint test | All controls.svg | 7,671 | 3,342 | the same |
| Overprint test | Course 12.svg | 7,451 | 3,008 | the same |
| Overprint test lower purple | All controls.svg | 7,741 | 3,364 | the same |
| Overprint test lower purple | Course 12.svg | 7,521 | 3,030 | the same |
| Lincoln Park | All controls.svg | 1,704,046 | 1,266,162 | the same |
| Lincoln Park | Short.svg | 1,704,049 | 1,266,012 | the same |
| printarea2 | Course 1.svg | 1,731,272 | 345,561 | the same |
| Madrona Permanent11 | Course 1.svg | 1,676,879 | 177,443 | the same |
| testproj2 | All controls.svg | 8,488 | 3,683 | the same |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 49 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 10 |
| Build | success | 28 |
| Cache the Playwright browser | success | 3 |
| Install Chromium | success | 17 |
| Test | success | 198 |

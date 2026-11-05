SVG loop: 353 passed, 0 failed, 5 pending

Commit 010704a96d70e4339e1376b462a6ecde10dc7b5a, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/42064035635. Results are on svg-results/svg-style-groups.

## Since the previous run

Compared with the results svg-results/svg-style-groups held before: SVG loop run 41983743998 of 9fe17bd5cf1e3b15592d692f80d1519474ea3c1a. The SVGs come to 102,291,992 bytes, against 114,374,247 before.

188 tests or drawings changed; the first 30 are below, and changes.md lists each.

| Test or drawing | What changed |
| --- | --- |
| AStyleSitsOnTheOutermostGroupWhoseElementsAllShareIt | new test, Passed |
| aiimport | SVG changed, 18,326 bytes as before |
| aiimport_light | SVG changed, 18,324 bytes as before |
| alllinetext | SVG 10,991 -> 10,901 bytes |
| angledashes9 | SVG changed, 41,911 bytes as before |
| bizzarro_dash11 | SVG 19,334 -> 11,169 bytes |
| borderedarea9 | SVG changed, 45,077 bytes as before |
| Cambria | SVG changed, 5,592 bytes as before |
| Cambriabold | SVG changed, 5,590 bytes as before |
| charspace | SVG 57,464 -> 56,654 bytes |
| combospace | SVG 55,403 -> 54,560 bytes |
| corner_ends | SVG changed, 6,396 bytes as before |
| corner_ends9 | SVG changed, 6,397 bytes as before |
| cornerglyphs6 | SVG 5,300 -> 4,167 bytes |
| cornerglyphs9 | SVG 5,300 -> 4,167 bytes |
| course-export-Course 1 | SVG 4,922,389 -> 4,682,468 bytes |
| cutareaborder9 | SVG 9,049 -> 8,889 bytes |
| cutcircles | SVG 3,873 -> 3,564 bytes |
| cutdoublesides6 | SVG 10,859 -> 10,745 bytes |
| cutdoublesides9 | SVG 10,859 -> 10,745 bytes |
| dashlengths6 | SVG 15,664 -> 15,349 bytes |
| dashlengths9 | SVG 15,664 -> 15,349 bytes |
| dashline | SVG 9,170 -> 8,970 bytes |
| dashline9 | SVG 9,171 -> 8,971 bytes |
| dashmin | SVG changed, 8,652 bytes as before |
| dbldashlengths7 | SVG 25,562 -> 24,944 bytes |
| dbldashlengths9 | SVG 25,562 -> 24,944 bytes |
| decrease | SVG 43,992 -> 38,918 bytes |
| decreasesymbols | SVG 74,167 -> 48,486 bytes |
| decreasesymbols6 | SVG 97,480 -> 72,105 bytes |

## Sizes of the reference exports (export-sizes.tsv)

The 22 files the SVG export writes of the reference events come to 6,863,687 bytes, 3,153,798 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 125,853 | 20,622 | 138,944 / 20,731 |
| mapexchange1 | Course 1.svg | 18,774 | 5,816 | 19,787 / 5,836 |
| mapexchange1 | Course 2-1.svg | 5,606 | 2,519 | the same |
| mapexchange1 | Course 2-2.svg | 11,562 | 3,252 | 11,775 / 3,256 |
| mapexchange1 | Course 3.svg | 20,599 | 6,213 | 21,912 / 6,228 |
| mapexchange1 | Course 4B.svg | 20,841 | 6,263 | 22,154 / 6,280 |
| mapexchange1 | Course 4G.svg | 19,735 | 6,018 | 20,948 / 6,032 |
| mapexchange1 | Course 5-1.svg | 18,889 | 5,723 | 19,802 / 5,736 |
| mapexchange1 | Course 5-2.svg | 14,653 | 4,261 | 15,066 / 4,266 |
| mapexchange1 | Course 5-3.svg | 12,709 | 3,657 | 12,922 / 3,665 |
| mapexchange1 | Course 5-4.svg | 14,516 | 4,144 | 14,929 / 4,150 |
| Overprint test | All controls.svg | 9,055 | 3,732 | 9,110 / 3,739 |
| Overprint test | Course 12.svg | 8,813 | 3,393 | 8,868 / 3,399 |
| Overprint test lower purple | All controls.svg | 9,105 | 3,752 | 9,160 / 3,760 |
| Overprint test lower purple | Course 12.svg | 8,863 | 3,415 | 8,918 / 3,418 |
| Lincoln Park | All controls.svg | 1,705,273 | 1,266,578 | 1,705,512 / 1,266,577 |
| Lincoln Park | Short.svg | 1,705,226 | 1,266,419 | 1,705,439 / 1,266,418 |
| printarea2 | Course 1.svg | 1,597,686 | 344,813 | 1,667,946 / 345,178 |
| Madrona Permanent11 | Course 1.svg | 1,464,679 | 175,641 | 1,479,782 / 175,805 |
| testproj2 | All controls.svg | 9,323 | 4,078 | 10,463 / 4,088 |
| Overprint test lower purple in one file | Overprint test.svg | 13,382 | 4,662 | 13,437 / 4,668 |
| mapexchange1 Course 5 in a file | Course 5.svg | 48,545 | 8,827 | 50,497 / 8,828 |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 30 |
| Name the results | success | 0 |
| Merge what this is built on | success | 3 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 14 |
| Build | success | 34 |
| Cache the Playwright browser | success | 3 |
| Install Chromium | success | 523 |
| Test | success | 261 |

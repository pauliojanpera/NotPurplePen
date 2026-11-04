SVG loop: 339 passed, 0 failed, 5 pending

Commit d2cdc7f8b1daabc178f4c35a4a724e32bb888c1c, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/41802608613. Results are on svg-results/svg-target.

## Since the previous run

Compared with the results svg-results/svg-target held before: SVG loop run 40129320105 of 24276e8026725c0c39dd7a8297a88a0dcd93f667. The SVGs come to 109,887,689 bytes, against 99,605,397 before.

21 tests or drawings changed.

| Test or drawing | What changed |
| --- | --- |
| printarea2 | new test, Passed; new test, Passed |
| printarea2-Course1 | new test, Passed |
| Overprint test lower purple | new test, Passed; new test, Passed |
| Madrona Permanent11 | new test, Passed; new test, Passed |
| Overprint test | new test, Passed; new test, Passed |
| OverprintTestLowerPurple-Course12 | new test, Passed |
| testproj2-AllControls | new test, Passed |
| mapexchange1 | new test, Passed; new test, Passed |
| testproj2 | new test, Passed; new test, Passed |
| AnEventIsWrittenAsAFileForEachPageTheSameEachTime | new test, Passed |
| MadronaPermanent11-Course1 | new test, Passed |
| Lincoln Park | new test, Passed; new test, Passed |
| LincolnPark-Short | new test, Passed |
| mapexchange1-Course5-2 | new test, Passed |
| course-export-Course 1 | new SVG, 5,139,056 bytes |
| page-LincolnPark-Short | new SVG, 1,704,049 bytes; new measurements |
| page-MadronaPermanent11-Course1 | new SVG, 1,676,879 bytes; new measurements |
| page-mapexchange1-Course5-2 | new SVG, 15,027 bytes; new measurements |
| page-OverprintTestLowerPurple-Course12 | new SVG, 7,521 bytes; new measurements |
| page-printarea2-Course1 | new SVG, 1,731,272 bytes; new measurements |
| page-testproj2-AllControls | new SVG, 8,488 bytes; new measurements |

## Sizes of the reference exports (export-sizes.tsv)

The 20 files the SVG export writes of the reference events come to 7,134,602 bytes, 3,135,626 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 118,816 | 19,869 |  |
| mapexchange1 | Course 1.svg | 19,203 | 5,439 |  |
| mapexchange1 | Course 2-1.svg | 4,462 | 2,128 |  |
| mapexchange1 | Course 2-2.svg | 11,911 | 2,886 |  |
| mapexchange1 | Course 3.svg | 21,144 | 5,843 |  |
| mapexchange1 | Course 4B.svg | 21,295 | 5,874 |  |
| mapexchange1 | Course 4G.svg | 20,232 | 5,632 |  |
| mapexchange1 | Course 5-1.svg | 19,378 | 5,359 |  |
| mapexchange1 | Course 5-2.svg | 15,027 | 3,904 |  |
| mapexchange1 | Course 5-3.svg | 13,116 | 3,307 |  |
| mapexchange1 | Course 5-4.svg | 14,900 | 3,780 |  |
| Overprint test | All controls.svg | 7,671 | 3,342 |  |
| Overprint test | Course 12.svg | 7,451 | 3,008 |  |
| Overprint test lower purple | All controls.svg | 7,741 | 3,364 |  |
| Overprint test lower purple | Course 12.svg | 7,521 | 3,030 |  |
| Lincoln Park | All controls.svg | 1,704,046 | 1,266,162 |  |
| Lincoln Park | Short.svg | 1,704,049 | 1,266,012 |  |
| printarea2 | Course 1.svg | 1,731,272 | 345,561 |  |
| Madrona Permanent11 | Course 1.svg | 1,676,879 | 177,443 |  |
| testproj2 | All controls.svg | 8,488 | 3,683 |  |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 35 |
| Name the results | success | 1 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 1 |
| Cache NuGet packages | success | 12 |
| Build | success | 29 |
| Cache the Playwright browser | success | 0 |
| Install Chromium | success | 22 |

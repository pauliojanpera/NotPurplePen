SVG loop: 364 passed, 0 failed, 5 pending

Commit 49279c77a35451231efba6a75ed69d538cba655f, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/42185888713. Results are on svg-results/svg-area-patterns.

## Since the previous run

Compared with the results svg-results/svg-area-patterns held before: SVG loop run 42129380215 of 451690e32aacdb06172dee073d48fddce91a131f. The SVGs come to 30,086,082 bytes, against 31,039,039 before.

66 tests or drawings changed; the first 30 are below, and changes.md lists each.

| Test or drawing | What changed |
| --- | --- |
| hatchingorigins | new test, Passed; new SVG, 3,711 bytes; new measurements |
| EachAreaTurnsAHatchingThatTurnsAboutItsPatternOriginAndLeavesOneThatNeverTurns | new test, Passed |
| Madrona Permanent11 with area patterns as separate shapes | new test, Passed; new test, Passed |
| AHatchingIsAPatternFillOfOneOfItsLinesInATileAsHighAsItsSpacing | new test, Passed |
| AsSeparateShapesEveryPatternIsDrawnElementByElementAndEveryHatchingLineByLine | new test, Passed |
| aiimport | SVG 18,326 -> 16,714 bytes; beyond 0 25431 -> 52014, beyond 8 7831 -> 39598, beyond 32 1648 -> 15964, beyond 64 294 -> 481 |
| aiimport_light | SVG 18,324 -> 16,712 bytes; beyond 0 22802 -> 50384, beyond 8 2929 -> 26772, beyond 32 526 -> 527 |
| course-export-Course 1 | SVG 2,336,759 -> 2,229,791 bytes |
| holes | SVG 11,622 -> 8,351 bytes; largest difference 112 -> 110, beyond 0 89442 -> 100669, beyond 8 33532 -> 39462, beyond 32 133 -> 92 |
| holes9 | SVG 11,623 -> 8,352 bytes; largest difference 112 -> 110, beyond 0 89442 -> 100669, beyond 8 33532 -> 39462, beyond 32 133 -> 92 |
| isomarea | SVG 20,583 -> 20,069 bytes; largest difference 87 -> 102, beyond 0 26697 -> 47415, beyond 8 12403 -> 29181, beyond 32 3291 -> 9460, beyond 64 91 -> 382 |
| isomarea_light | SVG 20,861 -> 20,356 bytes; largest difference 35 -> 41, beyond 0 23906 -> 44750, beyond 8 6047 -> 16471, beyond 32 7 -> 23 |
| isomarea_nopatbm | SVG 20,591 -> 20,077 bytes; largest difference 87 -> 102, beyond 0 26697 -> 47415, beyond 8 12403 -> 29181, beyond 32 3291 -> 9460, beyond 64 91 -> 382 |
| isomarea_nopatbm_light | SVG 20,869 -> 20,364 bytes; largest difference 35 -> 41, beyond 0 23906 -> 44750, beyond 8 6047 -> 16471, beyond 32 7 -> 23 |
| isomarea9 | SVG 20,584 -> 20,070 bytes; largest difference 87 -> 102, beyond 0 26697 -> 47415, beyond 8 12403 -> 29181, beyond 32 3291 -> 9460, beyond 64 91 -> 382 |
| isomarea9_light | SVG 20,862 -> 20,357 bytes; largest difference 35 -> 41, beyond 0 23906 -> 44750, beyond 8 6047 -> 16471, beyond 32 7 -> 23 |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark | SVG 550,978 -> 534,884 bytes |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark-1 | beyond 0 245364 -> 254740, beyond 8 114855 -> 127725, beyond 32 32178 -> 34797 |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark-2 | beyond 0 171958 -> 195366, beyond 8 73496 -> 93883, beyond 32 10903 -> 16698 |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark-3 | beyond 0 418920 -> 454302, beyond 8 196797 -> 197367, beyond 32 49488 -> 49486, beyond 64 8840 -> 8841 |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark-4 | beyond 0 155469 -> 163363, beyond 8 71260 -> 81168, beyond 32 14818 -> 16854, beyond 64 2656 -> 2659 |
| layout-LincolnParkPrintAreas2-OneFile-LincolnPark-5 | beyond 0 217215 -> 224453, beyond 8 105179 -> 115775, beyond 32 29093 -> 31257, beyond 64 6722 -> 6724 |
| layout-mapexchange1-FilePerCourse-Allcontrols | SVG 125,853 -> 120,250 bytes |
| layout-mapexchange1-FilePerCourse-Allcontrols-1 | beyond 0 57264 -> 66258, beyond 8 20488 -> 30674, beyond 32 5587 -> 5680 |
| layout-mapexchange1-FilePerCourse-Course1 | SVG 18,774 -> 13,308 bytes |
| layout-mapexchange1-FilePerCourse-Course1-1 | beyond 0 13088 -> 22937, beyond 8 3777 -> 13113, beyond 32 400 -> 404 |
| layout-mapexchange1-FilePerCourse-Course2 | SVG 14,169 -> 8,840 bytes |
| layout-mapexchange1-FilePerCourse-Course2-2 | beyond 0 6880 -> 18280, beyond 8 282 -> 10331, beyond 32 13 -> 119 |
| layout-mapexchange1-FilePerCourse-Course3 | SVG 20,599 -> 15,133 bytes |
| layout-mapexchange1-FilePerCourse-Course3-1 | beyond 0 17338 -> 24887, beyond 8 3980 -> 13634, beyond 32 420 -> 595 |

## Sizes of the reference exports (export-sizes.tsv)

The 23 files the SVG export writes of the reference events come to 6,482,588 bytes, 3,207,829 gzip-compressed.

| Event | File | Bytes | Gzip bytes | Before |
| --- | --- | ---: | ---: | --- |
| mapexchange1 | All controls.svg | 120,250 | 20,197 | 125,853 / 20,622 |
| mapexchange1 | Course 1.svg | 13,308 | 5,350 | 18,774 / 5,816 |
| mapexchange1 | Course 2-1.svg | 5,606 | 2,519 | the same |
| mapexchange1 | Course 2-2.svg | 6,233 | 2,792 | 11,562 / 3,252 |
| mapexchange1 | Course 3.svg | 15,133 | 5,733 | 20,599 / 6,213 |
| mapexchange1 | Course 4B.svg | 15,375 | 5,780 | 20,841 / 6,263 |
| mapexchange1 | Course 4G.svg | 14,269 | 5,543 | 19,735 / 6,018 |
| mapexchange1 | Course 5-1.svg | 13,423 | 5,257 | 18,889 / 5,723 |
| mapexchange1 | Course 5-2.svg | 9,187 | 3,803 | 14,653 / 4,261 |
| mapexchange1 | Course 5-3.svg | 7,380 | 3,199 | 12,709 / 3,657 |
| mapexchange1 | Course 5-4.svg | 9,050 | 3,679 | 14,516 / 4,144 |
| Overprint test | All controls.svg | 9,055 | 3,732 | the same |
| Overprint test | Course 12.svg | 8,813 | 3,393 | the same |
| Overprint test lower purple | All controls.svg | 9,105 | 3,752 | the same |
| Overprint test lower purple | Course 12.svg | 8,863 | 3,415 | the same |
| Lincoln Park | All controls.svg | 1,705,273 | 1,266,578 | the same |
| Lincoln Park | Short.svg | 1,705,226 | 1,266,419 | the same |
| printarea2 | Course 1.svg | 953,989 | 298,033 | 976,836 / 300,426 |
| Madrona Permanent11 | Course 1.svg | 338,607 | 107,409 | 349,484 / 108,570 |
| testproj2 | All controls.svg | 9,323 | 4,078 | the same |
| Overprint test lower purple in one file | Overprint test.svg | 13,382 | 4,662 | the same |
| mapexchange1 Course 5 in a file | Course 5.svg | 27,059 | 6,865 | 48,545 / 8,827 |
| Madrona Permanent11 with area patterns as separate shapes | Course 1.svg | 1,464,679 | 175,641 | new |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 38 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 0 |
| Cache NuGet packages | success | 12 |
| Build | success | 45 |
| Cache the Playwright browser | success | 2 |
| Install Chromium | success | 22 |

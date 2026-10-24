SVG loop: 290 passed, 0 failed, 5 pending

Commit d63be38257b505719f3754c6a1b6f1c6810e4eac, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/40091560160. Results are on svg-results/svg-text.

## Since the previous run

Compared with the results svg-results/svg-text held before: SVG loop run 40089777192 of d7aeeed41948b6864dbd6c55a2fb43d78e814ae3. The SVGs come to 168,073,600 bytes, against 168,248,364 before.

18 tests or drawings changed.

| Test or drawing | What changed |
| --- | --- |
| CharactersScaledOrInAnotherFontAreNotMergedIntoALineText | new test, Passed |
| ShapingSetsLettersOfTheSimpleScriptsAndTheirPunctuationAloneButNotMarksJoinersOrJoiningScripts | new test, Passed |
| CharactersThatShapingSetsByTheirNeighboursKeepTextsOfTheirOwnBetweenLineTexts | new test, Passed |
| CharactersDrawnOneByOneAlongALineMergeIntoOneTextWithAPositionAndATurnEach | new test, Passed |
| alllinetext | SVG 562,391 -> 553,769 bytes; largest difference 137 -> 139, beyond 0 12188 -> 12234, beyond 8 8566 -> 8642, beyond 32 3768 -> 3860, beyond 64 356 -> 469 |
| frametext1 | SVG 2,134,456 -> 2,132,801 bytes; beyond 0 40982 -> 40979, beyond 8 32901 -> 32900, beyond 32 19156 -> 19158 |
| frametext3 | SVG 2,124,770 -> 2,121,122 bytes; beyond 0 44541 -> 44540, beyond 32 20687 -> 20689 |
| linetext_6 | SVG 586,193 -> 559,189 bytes; beyond 0 26209 -> 26326, beyond 8 17790 -> 17947, beyond 32 5844 -> 6080, beyond 64 580 -> 793 |
| linetext_9 | SVG 586,193 -> 559,189 bytes; beyond 0 26209 -> 26326, beyond 8 17790 -> 17947, beyond 32 5844 -> 6080, beyond 64 580 -> 793 |
| linetext_mid | SVG 590,679 -> 559,251 bytes; beyond 0 25716 -> 25790, beyond 8 17824 -> 17915, beyond 32 6143 -> 6275, beyond 64 789 -> 882 |
| linetext_top | SVG 2,224,669 -> 2,208,382 bytes; largest difference 134 -> 160, beyond 0 26194 -> 26217, beyond 8 17999 -> 17990, beyond 32 8308 -> 8356, beyond 64 771 -> 796 |
| linetext2_6 | SVG 2,221,597 -> 2,207,770 bytes; largest difference 110 -> 118, beyond 0 19103 -> 19100, beyond 8 14068 -> 14066, beyond 32 6375 -> 6376, beyond 64 421 -> 412 |
| linetext2_9 | SVG 2,221,597 -> 2,207,770 bytes; largest difference 110 -> 118, beyond 0 19103 -> 19100, beyond 8 14068 -> 14066, beyond 32 6375 -> 6376, beyond 64 421 -> 412 |
| linetext2_mid | SVG 2,224,368 -> 2,208,119 bytes; largest difference 119 -> 123, beyond 0 21157 -> 21210, beyond 8 15371 -> 15422, beyond 32 7025 -> 7091, beyond 64 547 -> 637 |
| linetextspacing | SVG 558,447 -> 551,906 bytes; largest difference 114 -> 113, beyond 0 6487 -> 6500, beyond 8 4506 -> 4492, beyond 32 1921 -> 1902, beyond 64 122 -> 129 |
| newlines | SVG 1,083,693 -> 1,080,690 bytes; beyond 0 15000 -> 15015, beyond 8 12053 -> 12067, beyond 64 2012 -> 2039 |
| punchbox11 | SVG 572,707 -> 568,977 bytes; beyond 0 22884 -> 22959, beyond 8 11510 -> 11628, beyond 32 4440 -> 4612, beyond 64 1174 -> 1324 |
| punchbox9 | SVG 565,381 -> 563,442 bytes |

## Pending cases by the calls they need (5 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| CreatePen | 3 | 3 |
| DrawBitmap | 2 | 2 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 1 |
| Checkout source | success | 42 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 10 |
| Cache NuGet packages | success | 10 |
| Build | success | 16 |
| Cache the Playwright browser | success | 1 |
| Install Chromium | success | 28 |
| Test | success | 91 |

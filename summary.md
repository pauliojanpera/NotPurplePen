SVG loop: 219 passed, 0 failed, 192 pending

Commit aa0a5d82599fd784cfb30492ccad55b8b624dbd2, run https://github.com/pauliojanpera/NotPurplePen/actions/runs/39682696133. Results are on svg-results/diagnose-recording.

## Pending cases by the calls they need (192 cases; pending.tsv lists each)

| Call | Cases that need it | Cases that need only it |
| --- | ---: | ---: |
| PushAntiAliasing | 184 | 0 |
| PopAntiAliasing | 183 | 0 |
| CreatePen | 173 | 0 |
| PushTransform | 161 | 0 |
| PopTransform | 159 | 0 |
| DrawPolyline | 155 | 0 |
| CreateSolidBrush | 148 | 0 |
| CreateFont | 84 | 0 |
| DrawText | 81 | 0 |
| FillPolygon | 80 | 0 |
| FillEllipse | 71 | 0 |
| DrawPath | 67 | 0 |
| FillPath | 61 | 0 |
| CreatePath | 55 | 0 |
| PopClip | 54 | 0 |
| PushClip | 54 | 0 |
| DrawLine | 43 | 0 |
| DrawEllipse | 42 | 0 |
| DrawPolygon | 39 | 0 |
| Intensity | 17 | 0 |
| DrawBitmap | 7 | 2 |
| DrawTextOutline | 5 | 0 |
| FillRectangle | 3 | 0 |
| CreatePatternBrush | 2 | 0 |
| DrawArc | 2 | 0 |
| PopBlending | 2 | 0 |
| PushBlending | 2 | 0 |
| DrawRectangle | 1 | 0 |

## Steps

| Step | Outcome | Seconds |
| --- | --- | ---: |
| Set up job | success | 2 |
| Checkout source | success | 25 |
| Name the results | success | 0 |
| Merge what this is built on | skipped | 0 |
| Setup .NET | success | 9 |
| Cache NuGet packages | success | 0 |
| Build | success | 22 |
| Cache the Playwright browser | success | 0 |
| Install Chromium | success | 19 |
| Test | success | 17 |

# Six Skia text cases drawn in a wider face

Evidence of the failure of `Map_Skia.Tests`' TestWest, TestWest11, ArialNarrow and FramingText1 to 3, kept here because
the runs' artifacts expire.

- Oct 29, 2026: Tests run 40808105871 on `wip/linux/svg-export` (a2ca4fc), job Test (Skia); its artifact
  `rendering-differences-skia` expires on Jan 27, 2027.
- Nov 4, 2026: Tests run 41896080144 on `wip/linux/svg-colours` (81272f2), job Test (Skia) on a windows-11-arm runner;
  attempt 2 failed the same six cases. The artifact of attempt 1 expires on Feb 2, 2027.
- The twelve images in `actual/` and `diff/` are byte for byte the same in both runs' artifacts; one copy is kept.
- `expected/` holds the baselines at 81272f2 (`src/MapModel/TestFiles/skia_render/`), and `side-by-side/` each case's
  expected image, the image the failed runs drew, and their difference, one above another.
- On Nov 4 the full run of bac13f5 (`wip/linux/svg-pages`, on the same `base`) passed these cases; 81272f2 differs from
  it only in `src/MapModel/Map_Svg`, `src/MapModel/Map_Svg.Tests` and `src/TestFiles`.

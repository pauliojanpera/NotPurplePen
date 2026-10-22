# EPSG codes of OCAD's grids (G-3)

Research for G-3 of [the requirements](../SvgTargetRequirements.md): the table of OCAD grid ids in
`OcadToProj4Projection` (`src/MapModel/MapModel/MapProjection.cs`) is to give the EPSG code of each grid that has
one. Nothing here is in the code yet; the codes go into the table only once checked, and none is guessed.

## Sources

- The list of OCAD grid ids with their EPSG codes that the table appears to have been made from:
  <https://github.com/doppelmeter/OCAD-Grid-ID_to_EPSG>, commit `842398f` ("list revised"),
  `ocad_grid_id_2_epsg.csv`, separated by semicolons: grid id, CRS code, catalog, name, comment. It carries no
  licence, so it is not copied here; clone it to repeat the comparison.
- The repository's `AuthorityCodeToProj4.txt`, the EPSG definitions as proj4 strings that the map model already
  ships.
- PROJ's EPSG-derived database: `projected_crs.sql`, `conversion.sql`, `geodetic_crs.sql`, `geodetic_datum.sql`
  and `ellipsoid.sql` from `data/sql/` of <https://github.com/OSGeo/PROJ>.

## Tools and results

- `compare_epsg.py <MapProjection.cs> <the list's csv> <AuthorityCodeToProj4.txt> <compare.json>` compares each
  grid's proj4 string in the table with the repository's definition of the EPSG code the list gives it, ignoring
  `no_defs`, `towgs84`, `wktext`, `nadgrids` and `type`, with numbers equal within 1e-7. `compare.json` holds its
  groups: 595 confirmed, 8 mismatched, 29 whose code the repository does not define, 10 whose list entry names no
  EPSG code, none missing from the list.
- `check_with_proj.py <PROJ's data/sql folder> <MapProjection.cs> <compare.json> <verdicts.json>` checks every code
  of the confirmed, mismatched and undefined groups against PROJ's database: the projection method's parameters and the ellipsoid must agree with the
  table's proj4 string. `verdicts.json` gives, for each of the 632 grids, its code, group, EPSG name, whether EPSG
  has deprecated the code, and the problems found.
- Of the 632, 105 have problems listed, and 10 codes are deprecated: 7001 to 7003, 7005 to 7008, 46002, 47002 and
  50050. Many of the problems are the checker's own: it compares a false easting in feet with one in metres, as
  for the Arizona grids (213360 m against 700000 ft), and does not compare Krovak's parameters. The 8 mismatches
  are grids such as the Irish Grid (9001, 9002, 34000), the Dutch RD (41001, 41004), EOV (49000) and two of
  Malaysia's (54001, 54002), whose definitions differ in their datum shifts.

## What remains

Take into the table the codes confirmed without problems, check the rest one by one (or teach the checker units and
Krovak first), leave out whatever cannot be confirmed, test the lookup in the map model's tests of `MapProjection`,
and list the left-out grids in the progress file.

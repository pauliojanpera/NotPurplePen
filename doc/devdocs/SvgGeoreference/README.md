# EPSG codes of OCAD's grids (G-3)

Research for G-3 of [the requirements](../SvgTargetRequirements.md): the table of OCAD grid ids in
`OcadToProj4Projection` (`src/MapModel/MapModel/MapProjection.cs`) is to give the EPSG code of each grid that has
one. The codes went into the table only once checked, and none is guessed: af9f9df on `feature/svg-target` gives
624 of the table's 642 grids a code, from `verdicts-pyproj.json`.

## Sources

- The list of OCAD grid ids with their EPSG codes that the table appears to have been made from:
  <https://github.com/doppelmeter/OCAD-Grid-ID_to_EPSG>, commit `842398f` ("list revised"),
  `ocad_grid_id_2_epsg.csv`, separated by semicolons: grid id, CRS code, catalog, name, comment. It carries no
  licence, so it is not copied here; clone it to repeat the comparison.
- The repository's `AuthorityCodeToProj4.txt`, the EPSG definitions as proj4 strings that the map model already
  ships.
- PROJ's EPSG-derived database: `projected_crs.sql`, `conversion.sql`, `geodetic_crs.sql`, `geodetic_datum.sql`
  and `ellipsoid.sql` from `data/sql/` of <https://github.com/OSGeo/PROJ>.
- The same database as pyproj carries it: pyproj 3.8.0 with PROJ 9.8.1, from PyPI
  (`pip install --break-system-packages pyproj`).

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

- `check_with_pyproj.py <MapProjection.cs> <compare.json> <verdicts.json> <verdicts-pyproj.json>` settles each
  grid's code by where points land rather than by parameters, which takes care of units and Krovak:
  - Points spread over the code's area of use are projected by the table's proj4 string and by PROJ's definition
    of the code, each on its own ellipsoid, without a datum shift.
  - They must land within 1 mm of each other, in metres, with the same axis unit.
  - Grids of longitude and latitude need the same ellipsoid, prime meridian and unit instead.
  - A code EPSG has deprecated gives way to the first replacement EPSG names that passes.

  It catches a false easting 1 cm off, a scale factor 1e-7 off and an origin 1e-5° off. `verdicts-pyproj.json`
  gives each grid its code, if any, and the problems and notes found.
- Of the 632 grids the list gives a code, 624 take one, 10 of them as the replacement of a deprecated code (7001 to
  7003 and 7005 to 7008 to the NTF Lambert zones 27561 to 27564, 27571, 27573 and 27574, 46002 and 47002 to 3333,
  and 64000 to 3857). The problems the earlier check found in the others were its own. The Irish Grid's three grids
  carry a note: PROJ's `mod_airy` has a semi-minor axis 2 mm off EPSG's Airy Modified 1849, which moves no point by
  1 mm.
- Eight are left without a code:
  - 17008 (Nahrwan 1934 / UTM zone 38N, 7006), 57000 (Abidjan 1987 / UTM zone 30N, 2041) and 68000 (Arc 1960 /
    UTM zone 37S, 21037). Their proj4 strings take PROJ's Clarke 1880 mod. where EPSG has Clarke 1880 (RGS), and
    land 10, 3.4 and 3.9 cm away.
  - 50050 (NAD83 / Kentucky North), 66044 and 66045 (NAD83 / Indiana East and West, ftUS) and 66098 (NAD83 /
    South Dakota North, ftUS). They follow 26979, 2244, 2245 and 3454, which EPSG withdrew as wrong. Their
    replacements 2205, 2965, 2966 and 4457 land 29 m, 635 m, 635 m and 170 km away: the table's strings for these
    grids place points that far from where EPSG's corrected systems put them.
  - 63011 (ETRS89 / UTM zone 38N), whose code 25838 EPSG deprecated without naming a replacement.
- The ten grids of the list's `not_epsg` group have no EPSG code to give: 16001, 31005, 31006, 66001, 66002,
  66068, 66069, 66070, 66109 and 66123.

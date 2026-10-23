"""Check each OCAD grid's EPSG code against PROJ's EPSG database, as pyproj carries it (G-3).

Usage: python3 -I check_with_pyproj.py <MapProjection.cs> <compare.json> <verdicts.json> <out.json>

For every grid of OcadToProj4Projection's table that the list gives an EPSG code (the confirmed, mismatch and
not_in_authority groups of compare.json, with their codes from verdicts.json), the table's proj4 string and the
EPSG definition must describe the same coordinates.

A projected grid and a projected code agree when they have:

- the same unit on the easting axis;
- the same projection: points spread over the code's area of use, 7 by 7, projected from their latitude and
  longitude by each definition on its own ellipsoid, with no datum shift between them, land within 1 mm of each
  other, both taken in metres.

Where they land decides. Their ellipsoids are compared too, and a difference that moves no point by 1 mm is noted
rather than counted against the code: PROJ's mod_airy, for one, holds a semi-minor axis rounded 2 mm away from
EPSG's Airy Modified 1849. A grid of longitude and latitude agrees with a geographic code when both have the same
prime meridian and angular unit and ellipsoids within 1 mm in both axes.

A datum shift (towgs84, nadgrids) does not change where a point lands in the grid, so it is not compared; the
proj4 strings carry the shift they were given, and the EPSG code names the coordinate system they are given in.

A code that EPSG has deprecated is not kept: each replacement EPSG names is checked in its place, and the first that
passes is the grid's code. Writes, per grid, the code it takes, if any, and the problems found.
"""
import json
import math
import re
import sys

import numpy as np
from pyproj import CRS, Transformer
from pyproj.exceptions import CRSError

TOLERANCE_METRES = 0.001
SAMPLES = 7


def table_rows(source_path):
    """The grid ids and proj4 strings of OcadToProj4Projection's table, in its order."""
    text = open(source_path, encoding="utf-8-sig").read()
    return [(int(grid), proj4) for grid, proj4 in
            re.findall(r'new OcadToProj4Projection\((-?\d+), @"([^"]*)"', text)]


def projected(crs):
    """The projected coordinate system itself, without a datum shift bound to it."""
    return crs.source_crs if crs.is_bound else crs


def metres_per_unit(crs):
    """How many metres one unit of the easting axis is."""
    return crs.axis_info[0].unit_conversion_factor


def sample_points(area):
    """Longitudes and latitudes spread over an area of use, which may cross the antimeridian."""
    west, south, east, north = area.west, area.south, area.east, area.north
    if west > east:
        east += 360.0
    # Keep clear of the poles, where a projection may be undefined.
    south, north = max(south, -89.0), min(north, 89.0)
    lons, lats = np.meshgrid(np.linspace(west, east, SAMPLES), np.linspace(south, north, SAMPLES))
    lons = (lons + 180.0) % 360.0 - 180.0
    return lons.ravel(), lats.ravel()


def project(crs, lons, lats):
    """Project longitudes and latitudes on the system's own datum, as eastings and northings in metres."""
    transformer = Transformer.from_crs(crs.geodetic_crs, crs, always_xy=True)
    x, y = transformer.transform(lons, lats, errcheck=True)
    factor = metres_per_unit(crs)
    return np.asarray(x) * factor, np.asarray(y) * factor


def ellipsoid_difference(table, epsg):
    """How the two systems' ellipsoids differ, as text, or None when their axes agree within 0.1 mm."""
    a, b = table.ellipsoid, epsg.ellipsoid
    if abs(a.semi_major_metre - b.semi_major_metre) <= 1e-4 and abs(a.semi_minor_metre - b.semi_minor_metre) <= 1e-4:
        return None
    return "ellipsoid: table %s (a %.4f, b %.4f), EPSG %s (a %.4f, b %.4f)" % (
        a.name, a.semi_major_metre, a.semi_minor_metre, b.name, b.semi_major_metre, b.semi_minor_metre)


def compare_geographic(table, epsg):
    """The problems found between two geographic systems; none when they agree."""
    problems = []
    a, b = table.ellipsoid, epsg.ellipsoid
    if abs(a.semi_major_metre - b.semi_major_metre) > 0.001 or abs(a.semi_minor_metre - b.semi_minor_metre) > 0.001:
        problems.append(ellipsoid_difference(table, epsg))
    if abs(table.prime_meridian.longitude - epsg.prime_meridian.longitude) > 1e-12:
        problems.append("prime meridian: table %s, EPSG %s" % (table.prime_meridian.name, epsg.prime_meridian.name))
    if abs(table.axis_info[0].unit_conversion_factor - epsg.axis_info[0].unit_conversion_factor) > 1e-15:
        problems.append("unit: table %s, EPSG %s" % (table.axis_info[0].unit_name, epsg.axis_info[0].unit_name))
    return problems


def compare(proj4, code, notes=None):
    """The problems found between a grid's proj4 string and an EPSG code; none when they agree.
    notes: a list that differences which move no point by the tolerance are added to, if given."""
    try:
        table = projected(CRS.from_proj4(proj4))
    except CRSError as error:
        return ["the table's proj4 string is not one PROJ reads: %s" % error]
    try:
        epsg = CRS.from_epsg(code)
    except CRSError:
        return ["PROJ knows no EPSG code %d" % code]
    if table.is_geographic and epsg.is_geographic:
        return compare_geographic(table, epsg)
    if not (table.is_projected and epsg.is_projected):
        return ["the table's system is %s and EPSG %d's is %s" % (
            "projected" if table.is_projected else "not projected", code,
            "projected" if epsg.is_projected else "not projected")]

    problems = []
    difference = ellipsoid_difference(table, epsg)
    unit_table, unit_epsg = table.axis_info[0], epsg.axis_info[0]
    if abs(unit_table.unit_conversion_factor - unit_epsg.unit_conversion_factor) > 1e-12:
        problems.append("unit: table %s, EPSG %s" % (unit_table.unit_name, unit_epsg.unit_name))

    lons, lats = sample_points(epsg.area_of_use)
    try:
        x1, y1 = project(table, lons, lats)
        x2, y2 = project(epsg, lons, lats)
    except Exception as error:
        problems.append("projecting the area of use failed: %s" % error)
        return problems
    distance = float(np.max(np.hypot(x1 - x2, y1 - y2)))
    if not math.isfinite(distance) or distance > TOLERANCE_METRES:
        problems.append("projection: points land up to %.4f m apart over the area of use" % distance)
        if difference:
            problems.append(difference)
    elif difference and notes is not None:
        notes.append("%s; points land within %.4f m of each other" % (difference, distance))
    return problems


def main():
    source_path, compare_path, verdicts_path, out_path = sys.argv[1:5]
    groups = json.load(open(compare_path))
    verdicts = json.load(open(verdicts_path))
    group_of = {}
    for group, entries in groups.items():
        for entry in entries:
            group_of[str(entry[0])] = group

    out = {}
    for grid, proj4 in table_rows(source_path):
        verdict = verdicts.get(str(grid))
        if verdict is None:
            out[str(grid)] = {"code": None, "listed": None, "group": group_of.get(str(grid)), "problems": []}
            continue
        listed = verdict["code"]
        record = {"listed": listed, "group": verdict["group"], "name": verdict.get("name"), "code": None,
                  "deprecated": False, "problems": [], "notes": []}
        problems = compare(proj4, listed, record["notes"])
        try:
            listed_crs = CRS.from_epsg(listed)
            record["deprecated"] = bool(listed_crs.is_deprecated)
        except CRSError:
            listed_crs = None
        if not problems and not record["deprecated"]:
            record["code"] = listed
        elif listed_crs is not None and listed_crs.is_deprecated:
            record["replacements"] = {}
            listed_notes, record["notes"] = record["notes"], []
            for replacement in listed_crs.get_non_deprecated():
                authority, number = replacement.to_authority() or ("?", "0")
                if authority != "EPSG":
                    continue
                replacement_notes = []
                replacement_problems = compare(proj4, int(number), replacement_notes)
                record["replacements"][number] = replacement_problems
                if not replacement_problems and record["code"] is None:
                    record["code"] = int(number)
                    record["notes"] = replacement_notes
            if record["code"] is None:
                record["notes"] = listed_notes
            record["problems"] = problems
        else:
            record["problems"] = problems
        out[str(grid)] = record

    with open(out_path, "w", encoding="utf-8") as file:
        json.dump(out, file, indent=1, ensure_ascii=False)
        file.write("\n")

    coded = [r for r in out.values() if r.get("code")]
    print("table rows:", len(out), "| with a code from the list:", sum(1 for r in out.values() if r.get("listed")))
    print("take a code:", len(coded), "| of them a replacement of a deprecated code:",
          sum(1 for r in coded if r["code"] != r["listed"]))
    print("left out:", sum(1 for r in out.values() if r.get("listed") and not r.get("code")))


if __name__ == "__main__":
    main()

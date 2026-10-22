"""Check the EPSG codes that the OCAD grid list gives against PROJ's EPSG-derived database: each code's projection
method parameters and ellipsoid must agree with the table's proj4 definition of the grid. Prints a verdict per code."""
import json
import math
import re
import sys

proj_dir, table_path, compare_path, out_path = sys.argv[1:5]

def rows(path, table):
    """Yield the value tuples of a table's INSERT statements."""
    pattern = re.compile(r'^INSERT INTO "' + table + r'" VALUES\((.*)\);$')
    for line in open(path, encoding="utf-8"):
        match = pattern.match(line.strip())
        if match:
            yield split_values(match.group(1))

def split_values(text):
    """Split an SQL value list, honoring quoted strings with doubled quotes."""
    values, current, quoted, index = [], "", False, 0
    while index < len(text):
        char = text[index]
        if quoted:
            if char == "'" and index + 1 < len(text) and text[index + 1] == "'":
                current += "'"; index += 1
            elif char == "'":
                quoted = False
            else:
                current += char
        elif char == "'":
            quoted = True
        elif char == ",":
            values.append(current); current = ""
        else:
            current += char
        index += 1
    values.append(current)
    return [None if value == "NULL" else value for value in values]

projected = {int(r[1]): r for r in rows(proj_dir + "/projected_crs.sql", "projected_crs") if r[0] == "EPSG"}
conversions = {int(r[1]): r for r in rows(proj_dir + "/conversion.sql", "conversion") if r[0] == "EPSG"}
geodetic = {int(r[1]): r for r in rows(proj_dir + "/geodetic_crs.sql", "geodetic_crs") if r[0] == "EPSG"}
datums = {int(r[1]): r for r in rows(proj_dir + "/geodetic_datum.sql", "geodetic_datum") if r[0] == "EPSG"}
ellipsoids = {int(r[1]): r for r in rows(proj_dir + "/ellipsoid.sql", "ellipsoid") if r[0] == "EPSG"}

PARAMETER_NAMES = {8801: "lat_0", 8802: "lon_0", 8805: "k", 8806: "x_0", 8807: "y_0", 8821: "lat_0", 8822: "lon_0",
                   8823: "lat_1", 8824: "lat_2", 8826: "x_0", 8827: "y_0", 8811: "lat_0", 8812: "lonc", 8813: "alpha",
                   8814: "gamma", 8815: "k", 8816: "x_0", 8817: "y_0"}
ELLIPSOIDS = {"GRS80": (6378137.0, 298.257222101), "WGS84": (6378137.0, 298.257223563), "intl": (6378388.0, 297.0),
              "bessel": (6377397.155, 299.1528128), "mod_airy": (6377340.189, 299.3249646), "airy": (6377563.396, 299.3249646),
              "GRS67": (6378160.0, 298.247167427), "krass": (6378245.0, 298.3), "clrk66": (6378206.4, 294.978698213898),
              "clrk80": (6378249.145, 293.465)}

def to_degrees(value, unit):
    """Convert an angle in an EPSG unit to degrees."""
    value = float(value)
    if unit in (9102, 9122):
        return value
    if unit == 9110:  # sexagesimal DMS: DDD.MMSSsss
        sign = -1 if value < 0 else 1
        value = abs(value)
        degrees = math.floor(value)
        minutes_seconds = (value - degrees) * 100
        minutes = math.floor(minutes_seconds + 1e-9)
        seconds = (minutes_seconds - minutes) * 100
        return sign * (degrees + minutes / 60 + seconds / 3600)
    if unit == 9105:
        return value * 0.9
    if unit == 9101:
        return math.degrees(value)
    raise ValueError("angle unit %d" % unit)

def parse_proj4(proj4):
    """Parse a proj4 string into its parameters, expanding a UTM zone into its Transverse Mercator parameters."""
    params = {}
    for token in proj4.split():
        if token.startswith("+"):
            key, _, value = token[1:].partition("=")
            params[key] = value
    if params.get("proj") == "utm":
        zone = int(params["zone"])
        params.update({"lat_0": "0", "lon_0": str(-183 + 6 * zone), "k": "0.9996", "x_0": "500000",
                       "y_0": "10000000" if "south" in params else "0"})
    if "k_0" in params:
        params["k"] = params.pop("k_0")
    if params.get("proj") == "somerc":
        params["lonc"] = params.get("lon_0", "0")
        params.setdefault("alpha", "90")
        params.setdefault("gamma", "90")
    return params

def ellipsoid_of(params):
    """The semi-major axis and inverse flattening of a proj4 definition's ellipsoid."""
    if "ellps" in params:
        return ELLIPSOIDS.get(params["ellps"])
    if params.get("datum") == "WGS84":
        return ELLIPSOIDS["WGS84"]
    if "a" in params and "b" in params:
        a, b = float(params["a"]), float(params["b"])
        return (a, a / (a - b) if a != b else float("inf"))
    return None

def check(code, proj4):
    """Return a list of the differences between an EPSG code's definition and a proj4 string."""
    problems = []
    crs = projected.get(code)
    if crs is None:
        if code in geodetic:
            return check_geographic(code, proj4)
        return ["code not in PROJ's EPSG data"], None, None
    name, deprecated = crs[2], crs[11] == "1"
    conversion = conversions.get(int(crs[9]))
    params = parse_proj4(proj4)
    method = int(conversion[5])
    if method in (9819, 1041):
        problems.append("Krovak parameters not compared")
    for slot in range(7 if method not in (9819, 1041) else 0):
        base = 7 + slot * 6
        if conversion[base] is None:
            continue
        parameter, value, unit = int(conversion[base + 1]), conversion[base + 3], int(conversion[base + 5])
        key = PARAMETER_NAMES.get(parameter)
        if key is None:
            problems.append("unmapped parameter %d" % parameter)
            continue
        if unit in (9102, 9122, 9110, 9105, 9101):
            expected = to_degrees(value, unit)
        else:
            expected = float(value)
        if key == "lonc" and "lonc" not in params and "lon_0" in params:
            key = "lon_0"
        if key not in params:
            if key == "gamma" and method == 9815:
                problems.append("gamma missing in proj4")
            elif not (key in ("lat_0", "x_0", "y_0") and expected == 0):
                problems.append("%s missing in proj4 (EPSG %s)" % (key, expected))
            continue
        actual = float(params[key])
        if abs(actual - expected) > 1e-6 * max(1.0, abs(expected)):
            problems.append("%s: proj4 %s, EPSG %s" % (key, actual, expected))
    if method == 1024:
        # Popular Visualisation Pseudo Mercator projects on a sphere of the ellipsoid's semi-major axis.
        params = dict(params)
        params.pop("b", None)
        params["rf_any"] = "1"
    problems.extend(compare_ellipsoid(int(geodetic[int(crs[7])][8]), params))
    if method == 9808 and params.get("axis") != "wsu":
        problems.append("south-orientated, but proj4 has no +axis=wsu")
    return problems, name, deprecated

def compare_ellipsoid(datum_code, params):
    """Compare the ellipsoid of an EPSG datum with that of a proj4 definition."""
    problems = []
    datum = datums[datum_code]
    ellipsoid = ellipsoids[int(datum[5])]
    a = float(ellipsoid[6])
    b = float(ellipsoid[10]) if ellipsoid[10] is not None else None
    rf = float(ellipsoid[9]) if ellipsoid[9] is not None else (a / (a - b) if a != b else float("inf"))
    mine = ellipsoid_of(params)
    if "rf_any" in params and mine is None and "a" in params:
        mine = (float(params["a"]), rf)
    if mine is None or abs(mine[0] - a) > 0.01 or (abs(mine[1] - rf) > 1e-4 and not (math.isinf(mine[1]) and ellipsoid[10] is not None and float(ellipsoid[10]) == a)):
        problems.append("ellipsoid: proj4 %s, EPSG %s (%s, %s)" % (mine, ellipsoid[2], a, rf))
    return problems

def check_geographic(code, proj4):
    """Check a geographic EPSG code against a proj4 definition of longitude and latitude."""
    crs = geodetic[code]
    params = parse_proj4(proj4)
    problems = [] if params.get("proj") == "longlat" else ["geographic CRS, but proj4 is %s" % params.get("proj")]
    problems.extend(compare_ellipsoid(int(crs[8]), params))
    return problems, crs[2], crs[10] == "1"

table = {int(m.group(1)): m.group(2) for m in re.finditer(r'new OcadToProj4Projection\((-?\d+), @"([^"]*)"\)', open(table_path, encoding="utf-8").read())}
compared = json.load(open(compare_path))
verdicts = {}
for group in ("confirmed", "mismatch", "not_in_authority"):
    for item in compared[group]:
        ocad_id, code = item[0], item[1]
        problems, name, deprecated = check(code, table[ocad_id])
        verdicts[ocad_id] = {"code": code, "group": group, "problems": problems, "name": name, "deprecated": deprecated}
        if group != "confirmed" or problems:
            print(ocad_id, code, group, name, "DEPRECATED" if deprecated else "", problems)
print("checked", len(verdicts), "clean", sum(1 for v in verdicts.values() if not v["problems"]),
      "deprecated", sum(1 for v in verdicts.values() if v["deprecated"]))
json.dump(verdicts, open(out_path, "w"), indent=0)

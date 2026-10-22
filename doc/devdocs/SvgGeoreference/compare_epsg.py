"""Compare the OCAD grid table of MapProjection.cs with the OCAD-grid-to-EPSG list it was generated from and with the
EPSG definitions of the repository's AuthorityCodeToProj4.txt. Prints one line per table entry and a summary."""
import csv
import json
import re
import sys

table_path, csv_path, authority_path, out_path = sys.argv[1:5]

table = {}
for match in re.finditer(r'new OcadToProj4Projection\((-?\d+), @"([^"]*)"\)', open(table_path, encoding="utf-8").read()):
    table[int(match.group(1))] = match.group(2)

listing = {}
with open(csv_path, encoding="utf-8-sig") as file:
    reader = csv.reader(file, delimiter=";")
    next(reader)
    for row in reader:
        if row and row[0].strip():
            listing[int(row[0])] = {"code": row[1].strip(), "catalog": row[2].strip(), "name": row[3].strip(), "comment": row[4].strip() if len(row) > 4 else ""}

authority = {}
for line in open(authority_path, encoding="utf-8-sig"):
    if line.startswith("EPSG:"):
        key, _, proj4 = line.rstrip("\n").partition("\t")
        authority[int(key[5:])] = proj4.strip()

IGNORED = {"no_defs", "towgs84", "wktext", "nadgrids", "type"}
NUMERIC_TOLERANCE = 1e-7

def parse(proj4):
    """Parse a proj4 string into its parameters, the datum WGS84 standing for its ellipsoid."""
    params = {}
    for token in proj4.split():
        if not token.startswith("+"):
            continue
        key, _, value = token[1:].partition("=")
        if key in IGNORED:
            continue
        params[key] = value
    if params.get("datum") == "WGS84":
        params.setdefault("ellps", "WGS84")
    if params.get("datum") in ("NAD83",):
        params.setdefault("ellps", "GRS80")
    params.pop("datum", None)
    if "k_0" in params:
        params["k"] = params.pop("k_0")
    for key, default in (("lat_0", "0"), ("x_0", "0"), ("y_0", "0"), ("k", "1"), ("units", "m")):
        params.setdefault(key, default)
    return params

def same(a, b):
    """Whether two parsed definitions describe the same projection."""
    if set(a) != set(b):
        return False
    for key in a:
        if a[key] == b[key]:
            continue
        try:
            x, y = float(a[key]), float(b[key])
        except ValueError:
            return False
        if abs(x - y) > NUMERIC_TOLERANCE * max(1.0, abs(x), abs(y)):
            return False
    return True

results = {"confirmed": [], "mismatch": [], "not_in_authority": [], "not_epsg": [], "not_listed": []}
for ocad_id in sorted(table):
    entry = listing.get(ocad_id)
    if entry is None:
        results["not_listed"].append(ocad_id)
        continue
    if entry["catalog"] != "EPSG":
        results["not_epsg"].append((ocad_id, entry["catalog"], entry["code"]))
        continue
    code = int(entry["code"])
    if code not in authority:
        results["not_in_authority"].append((ocad_id, code, entry["name"]))
        continue
    if same(parse(table[ocad_id]), parse(authority[code])):
        results["confirmed"].append((ocad_id, code))
    else:
        results["mismatch"].append((ocad_id, code, entry["name"], table[ocad_id], authority[code]))

print("table entries", len(table), "listed", sum(1 for i in table if i in listing))
for key, value in results.items():
    print(key, len(value))
for item in results["mismatch"][:40]:
    print("MISMATCH", item[0], item[1], item[2]); print("   table    ", item[3]); print("   authority", item[4])
for item in results["not_in_authority"][:80]:
    print("NOT IN AUTHORITY", item)
print("NOT EPSG", results["not_epsg"])
print("NOT LISTED", results["not_listed"])
json.dump(results, open(out_path, "w"), indent=0)

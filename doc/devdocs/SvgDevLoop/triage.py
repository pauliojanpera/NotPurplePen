"""Sort the outcome of a run of the SVG tests by what each failure means.

Usage: python3 -I triage.py <test results .trx> [--names]

Prints how many comparison cases passed, were pending, have no recorded reference yet (within the tolerance or not),
fall outside the tolerance, put a character of a text too far from the shaping, or failed otherwise, and the other
tests' failures. With --names, every case is named under its heading; without it, only the failures are.
"""
import collections
import re
import sys
import xml.etree.ElementTree as ElementTree

NAMESPACE = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
CASE = re.compile(r"(?:MapArea|Drawing)\((.*)\)$")


def message_of(result):
    message = result.find(f"{NAMESPACE}Output/{NAMESPACE}ErrorInfo/{NAMESPACE}Message")
    return (message.text or "").strip() if message is not None else ""


def sort(path):
    kinds = collections.defaultdict(list)
    for result in ElementTree.parse(path).getroot().iter(NAMESPACE + "UnitTestResult"):
        name, outcome, text = result.get("testName") or "", result.get("outcome"), message_of(result)
        case = CASE.match(name)
        first_line = text.splitlines()[0] if text else ""
        if not case:
            kinds["other tests passed" if outcome == "Passed" else "other tests failed"].append((name, first_line[:200]))
            continue
        case = case.group(1)
        pixels = re.search(r"has ([\d,]+) pixels outside the tolerance", text)
        if outcome == "Passed":
            kinds["passed"].append(case)
        elif outcome == "NotExecuted":
            kinds["pending"].append((case, first_line[:100]))
        elif "No reference image" in text:
            kinds["new, outside the tolerance" if pixels else "new, within the tolerance"].append(
                (case, int(pixels.group(1).replace(",", "")) if pixels else 0))
        elif pixels:
            kinds["outside the tolerance"].append((case, int(pixels.group(1).replace(",", ""))))
        elif "from where Skia's shaping puts its glyph" in text:
            kinds["text misplaced"].append((case, first_line[:200]))
        else:
            kinds["failed otherwise"].append((case, first_line[:200]))
    return kinds


def main():
    kinds = sort(sys.argv[1])
    names = "--names" in sys.argv[2:]
    order = ["passed", "pending", "new, within the tolerance", "new, outside the tolerance", "outside the tolerance",
             "text misplaced", "failed otherwise", "other tests passed", "other tests failed"]
    for kind in order:
        if kind not in kinds:
            continue
        print("%s: %d" % (kind, len(kinds[kind])))
        quiet = kind in ("passed", "other tests passed", "new, within the tolerance", "pending") and not names
        if quiet:
            continue
        entries = kinds[kind]
        if entries and isinstance(entries[0], tuple) and isinstance(entries[0][1], int):
            entries = sorted(entries, key=lambda entry: -entry[1])
        for entry in entries:
            print("   ", entry if isinstance(entry, str) else " | ".join(str(part) for part in entry))
    failures = sum(len(kinds[kind]) for kind in order[3:] if kind != "other tests passed")
    sys.exit(1 if failures else 0)


if __name__ == "__main__":
    main()

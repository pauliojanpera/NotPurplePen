"""Print the titles of an agent's new steps every few minutes, each after the UTC time it was taken: the tool call's
description, or its tool and file.
Keeps how far it got in a state file, so that a dump started again begins where the last one ended.

Usage: python3 -I dump-titles.py <agent transcript.jsonl> <state file> <minutes> <most titles a dump shows>
"""
import json
import os
import sys
import time


def titles(transcript):
    found = []
    with open(transcript, encoding="utf-8") as f:
        for line in f:
            try:
                record = json.loads(line)
            except ValueError:
                continue
            if record.get("type") != "assistant":
                continue
            for block in record.get("message", {}).get("content", []) or []:
                if not isinstance(block, dict) or block.get("type") != "tool_use":
                    continue
                arguments = block.get("input", {}) or {}
                title = arguments.get("description")
                if not title:
                    path = arguments.get("file_path") or arguments.get("path") or ""
                    title = "%s %s" % (block.get("name", "?"), os.path.basename(path))
                when = (record.get("timestamp") or "")[11:16] or "--:--"
                found.append(when + " " + title.strip().replace("\n", " ")[:140])
    return found


def main():
    transcript, state, minutes, most = sys.argv[1], sys.argv[2], float(sys.argv[3]), int(sys.argv[4])
    try:
        with open(state) as f:
            seen = int(f.read().strip() or 0)
    except (OSError, ValueError):
        seen = 0
    while True:
        found = titles(transcript)
        new = found[seen:]
        if new:
            shown = new[-most:]
            head = "%d new steps, UTC" % len(new) + (" (the last %d):" % len(shown) if len(new) > len(shown) else ":")
            print("\n".join([head] + ["- " + t for t in shown]), flush=True)
            seen = len(found)
            with open(state, "w") as f:
                f.write(str(seen))
        time.sleep(minutes * 60)


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Give agents a one-hour prompt cache, and report which cache lifetime requests got.

Usage:
  agent-cache.py set [settings.json]
      Sets "subagentPromptCacheTtl": "1h" in Claude Code's user settings,
      ~/.claude/settings.json unless another file is named, keeping every other
      key. Claude Code otherwise gives the requests of agents, workflows and its
      own background work a five-minute cache (see
      https://code.claude.com/docs/en/prompt-caching); the setting needs
      Claude Code v2.1.242 or later.
  agent-cache.py report <transcript.jsonl>...
      For each transcript, a session's or an agent's, prints the tokens its
      requests wrote to the cache with a five-minute and with a one-hour
      lifetime, the tokens they read from it, and, for the last requests that
      came after a gap of 5 to 60 minutes, what each read and wrote: a request
      that read nothing and wrote its whole context found its cache expired.
"""

import json
import os
import sys
from datetime import datetime


def set_ttl(path):
    """Set subagentPromptCacheTtl to 1h in the settings file at path, keeping its other keys."""
    try:
        with open(path, encoding="utf-8") as f:
            settings = json.load(f)
    except FileNotFoundError:
        settings = {}
    settings["subagentPromptCacheTtl"] = "1h"
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(settings, f, indent=2)
        f.write("\n")
    os.replace(tmp, path)
    print("%s: subagentPromptCacheTtl = %s" % (path, settings["subagentPromptCacheTtl"]))


def report(path, last=6):
    """Print the cache writes by lifetime, the reads, and the requests after gaps in a transcript.

    last: how many of the requests after a gap of 5 to 60 minutes to print.
    """
    w5 = w1 = reads = n = 0
    gaps = []
    prev = None
    for line in open(path, encoding="utf-8"):
        try:
            e = json.loads(line)
        except ValueError:
            continue
        if e.get("type") != "assistant":
            continue
        u = e.get("message", {}).get("usage") or {}
        cc = u.get("cache_creation") or {}
        c5 = cc.get("ephemeral_5m_input_tokens", 0) or 0
        c1 = cc.get("ephemeral_1h_input_tokens", 0) or 0
        r = u.get("cache_read_input_tokens", 0) or 0
        w5, w1, reads, n = w5 + c5, w1 + c1, reads + r, n + 1
        ts = e.get("timestamp")
        if not ts:
            continue
        t = datetime.fromisoformat(ts.replace("Z", "+00:00"))
        if prev is not None and 300 < (t - prev).total_seconds() < 3600:
            gaps.append((ts[:19], int((t - prev).total_seconds()), r, c5 + c1))
        prev = t
    print("%s: %d replies; cache writes %d tokens at 5 minutes, %d at 1 hour; %d read"
          % (os.path.basename(path), n, w5, w1, reads))
    for g in gaps[-last:]:
        print("  after a gap of %d s, at %s: read %d, wrote %d" % (g[1], g[0], g[2], g[3]))


def main():
    """Run the subcommand named on the command line."""
    if len(sys.argv) >= 2 and sys.argv[1] == "set" and len(sys.argv) <= 3:
        set_ttl(sys.argv[2] if len(sys.argv) == 3 else os.path.expanduser("~/.claude/settings.json"))
    elif len(sys.argv) >= 3 and sys.argv[1] == "report":
        for path in sys.argv[2:]:
            report(path)
    else:
        sys.exit(__doc__)


if __name__ == "__main__":
    main()

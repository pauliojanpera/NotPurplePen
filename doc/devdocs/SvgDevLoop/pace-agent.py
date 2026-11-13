#!/usr/bin/env python3
"""Pace an agent by its transcript, and end when the session has something to do.

Usage: pace-agent.py <agent transcript.jsonl> <state.json> [--work MIN] [--ratio R]
                     [--max-rest MIN] [--short SEC] [--keepalive MIN] [--poll SEC] [--dry-run]

The agent's work is counted cumulatively from its transcript:
- the interval up to each of its replies (an assistant record) is work;
- the interval up to any other record is work when it is at most --short seconds
  (a short command), and rest when it is longer (a long command, a wait for a
  run, a rest).
A balance B gains the work and loses the rest divided by --ratio, never below 0.

The script ends, printing why, when:
- in the work phase, B reaches --work minutes: a rest of B * ratio is due, but
  never more than --max-rest minutes, so that the agent's prompt cache, which
  lasts an hour, outlives it ("REST DUE: N minutes"), and the state moves to the
  rest phase; work that a capped rest leaves unpaid stays in B and brings the
  next rest sooner;
- in the rest phase, the transcript shows that the agent resumed after a rest of
  at least 3/4 of the one asked for ("REST OVER"): the state moves back to work;
- in either phase, --keepalive minutes have passed since it started
  ("KEEPALIVE"), so that the session wakes within the hour its own prompt cache
  lasts.
Each time it also prints the balance, the work and rest since its last report,
and the titles of the agent's steps since then. The state file carries the count
over to the next start; the first start counts the whole transcript.
"""

import argparse
import json
import os
import sys
import time
from datetime import datetime, timezone


def parse_ts(text):
    """Return the UTC datetime of an ISO 8601 timestamp such as 2026-11-12T09:26:00.123Z."""
    return datetime.fromisoformat(text.replace("Z", "+00:00"))


def load_state(path):
    """Return the state saved at path, or a fresh one in the work phase."""
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except (OSError, ValueError):
        return {"offset": 0, "last_ts": None, "balance": 0.0, "phase": "work",
                "asked_at": None, "asked_seconds": 0.0, "work": 0.0, "rest": 0.0,
                "longest_rest": 0.0, "titles": []}


def save_state(path, state):
    """Write the state to path atomically."""
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(state, f)
    os.replace(tmp, path)


def read_new_records(path, state):
    """Yield the transcript's complete records after the state's offset, advancing the offset."""
    try:
        f = open(path, "rb")
    except OSError:
        return
    with f:
        f.seek(state["offset"])
        data = f.read()
    end = data.rfind(b"\n")
    if end < 0:
        return
    state["offset"] += end + 1
    for line in data[:end].split(b"\n"):
        try:
            yield json.loads(line)
        except ValueError:
            continue


def account(state, records, ratio, short):
    """Add the intervals up to each record to the state's work or rest, and to its balance.

    ratio: minutes of rest that pay for one minute of work.
    short: the longest interval, in seconds, up to a record other than a reply that counts as work.
    """
    for e in records:
        ts = e.get("timestamp")
        if not ts:
            continue
        t = parse_ts(ts)
        if state["last_ts"] is not None:
            gap = max(0.0, (t - parse_ts(state["last_ts"])).total_seconds())
            if e.get("type") == "assistant" or gap <= short:
                state["work"] += gap
                state["balance"] += gap
            else:
                state["rest"] += gap
                state["longest_rest"] = max(state["longest_rest"], gap)
                state["balance"] = max(0.0, state["balance"] - gap / ratio)
        state["last_ts"] = ts
        if e.get("type") == "assistant":
            for b in (e.get("message", {}).get("content") or []):
                if isinstance(b, dict) and b.get("type") == "tool_use":
                    d = (b.get("input") or {}).get("description")
                    if d:
                        state["titles"] = (state["titles"] + [ts[11:16] + " " + d])[-12:]


def report(state, why):
    """Print why the script ends, with the work, rest and step titles since the last report."""
    print(why)
    print("balance %.1f min of work; since the last report %.1f min of work, %.1f min of rest"
          % (state["balance"] / 60, state["work"] / 60, state["rest"] / 60))
    for t in state["titles"]:
        print("- " + t)
    state["work"] = state["rest"] = state["longest_rest"] = 0.0
    state["titles"] = []


def main():
    """Count the agent's work until a rest is due, a rest is over, or the keepalive is reached."""
    p = argparse.ArgumentParser()
    p.add_argument("transcript")
    p.add_argument("state")
    p.add_argument("--work", type=float, default=20.0, help="minutes of work before a rest")
    p.add_argument("--ratio", type=float, default=2.0, help="minutes of rest per minute of work")
    p.add_argument("--max-rest", type=float, default=59.0, help="the longest rest, in minutes, within the agent's hour of cache")
    p.add_argument("--short", type=float, default=180.0, help="seconds a command may take and count as work")
    p.add_argument("--keepalive", type=float, default=50.0, help="minutes after which to end anyway")
    p.add_argument("--poll", type=float, default=30.0, help="seconds between reads of the transcript")
    p.add_argument("--dry-run", action="store_true", help="count, report and end, without saving")
    a = p.parse_args()

    state = load_state(a.state)
    started = time.time()
    while True:
        account(state, read_new_records(a.transcript, state), a.ratio, a.short)
        now = datetime.now(timezone.utc)
        if a.dry_run:
            report(state, "DRY RUN, phase %s" % state["phase"])
            return
        if state["phase"] == "work" and state["balance"] >= a.work * 60:
            rest = min(state["balance"] * a.ratio, a.max_rest * 60)
            state["phase"] = "rest"
            state["asked_at"] = now.isoformat()
            state["asked_seconds"] = rest
            report(state, "REST DUE: %d minutes" % round(rest / 60))
            save_state(a.state, state)
            return
        if state["phase"] == "rest" and state["longest_rest"] >= 0.75 * state["asked_seconds"]:
            state["phase"] = "work"
            report(state, "REST OVER: the agent resumed after %d minutes"
                   % round(state["longest_rest"] / 60))
            save_state(a.state, state)
            return
        if time.time() - started >= a.keepalive * 60:
            report(state, "KEEPALIVE, phase %s" % state["phase"])
            save_state(a.state, state)
            return
        save_state(a.state, state)
        sys.stdout.flush()
        time.sleep(a.poll)


if __name__ == "__main__":
    main()

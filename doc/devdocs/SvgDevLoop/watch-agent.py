"""Print one line for each headline event of an agent's work: a commit in its worktrees, a push of its branches,
and a GitHub Actions run of them starting or finishing. Polls once a minute; keeps its state in a file, so that a
watch started again reports only what is new.

Usage: python3 -I watch-agent.py <clone> <state.json> <since UTC, e.g. 2026-10-30T20:55:00Z> <branch substring>...

The worktrees of the clone are the agent's, but for the clone itself and those named, colon-separated, in
WATCH_SESSION_WORKTREES, which are the session's own. Branches named exactly feature/svg-target and dev are always
watched.
"""
import json
import os
import subprocess
import sys
import time

REPO = sys.argv[1]
SESSION_WORKTREES = {REPO} | {p for p in os.environ.get("WATCH_SESSION_WORKTREES", "").split(":") if p}
WATCHED_EXACTLY = {"feature/svg-target", "dev"}


def run(*arguments):
    return subprocess.run(list(arguments), capture_output=True, text=True).stdout


def watched(branch, substrings):
    return branch in WATCHED_EXACTLY or any(s in branch for s in substrings)


def snapshot(since, substrings, previous):
    snap = {"commits": {}, "branches": {}, "runs": {}}
    for line in run("git", "-C", REPO, "worktree", "list", "--porcelain").splitlines():
        if line.startswith("worktree ") and line[9:] not in SESSION_WORKTREES:
            worktree = line[9:]
            snap["commits"][worktree] = run("git", "-C", worktree, "log", "-1", "--format=%h %s").strip()
    heads = run("git", "-C", REPO, "ls-remote", "--heads", "origin")
    if heads:
        for line in heads.splitlines():
            sha, ref = line.split("\t")
            name = ref[len("refs/heads/"):]
            if watched(name, substrings):
                snap["branches"][name] = sha[:8]
    else:
        snap["branches"] = previous["branches"] if previous else {}
    try:
        answer = json.loads(run("curl", "-sS", "https://api.github.com/repos/pauliojanpera/NotPurplePen/actions/runs"
                                f"?per_page=40&created=>{since}"))
        for r in answer["workflow_runs"]:
            if watched(r["head_branch"], substrings):
                snap["runs"][str(r["id"])] = [r["name"], r["head_branch"], r["head_sha"][:8], r["status"], r["conclusion"]]
    except (ValueError, KeyError):
        snap["runs"] = previous["runs"] if previous else {}
    return snap


def events(old, new):
    for worktree, head in new["commits"].items():
        if worktree not in old["commits"]:
            yield "worktree %s made at %s" % (worktree.rsplit("/", 1)[-1], head)
        elif old["commits"][worktree] != head:
            yield "commit in %s: %s" % (worktree.rsplit("/", 1)[-1], head)
    for name, sha in new["branches"].items():
        if old["branches"].get(name) != sha:
            yield "pushed %s at %s" % (name, sha)
    for run_id, (name, branch, sha, status, conclusion) in new["runs"].items():
        before = old["runs"].get(run_id)
        if before is None:
            yield "%s run %s started on %s (%s)" % (name, run_id, branch, sha)
        if status == "completed" and (before is None or before[3] != "completed"):
            yield "%s run %s on %s (%s): %s" % (name, run_id, branch, sha, conclusion)


def main():
    state, since, substrings = sys.argv[2], sys.argv[3], sys.argv[4:]
    try:
        with open(state) as f:
            old = json.load(f)
    except (OSError, ValueError):
        old = None
    while True:
        new = snapshot(since, substrings, old)
        if old is not None:
            for line in events(old, new):
                print(line, flush=True)
        with open(state, "w") as f:
            json.dump(new, f)
        old = new
        time.sleep(60)


if __name__ == "__main__":
    main()

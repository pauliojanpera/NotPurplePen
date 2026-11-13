@AGENTS.md
@AGENTS.NotPurplePen.md

## Merge messages

A merge into a shared branch (`base`, `basewithfixes`, `dev`, `master` or `feature/svg-target`) says what went in,
since the releases page shows a build's commit message as its entry (@Pauli, Nov 13). Its subject names the work it
brings, as "Merge feature/svg-target, with the SVG export's strings translated, into dev", and never git's default
"Merge branch …". Its body lists the commits the merge brings, one line each, as `git merge --log=100` writes them
(or `git log --no-merges --format='%s (%h)' <target>..<branch>`), and then says how any conflict was resolved.

## Running the work: the agents' cache and pacing

These apply to the sessions that carry the work and the agents they start (@Pauli, Nov 12). The scripts are in
`doc/devdocs/SvgDevLoop/`, and its README says how to run them.

- **The agents' prompt cache.** At the start of every session, and before starting any agent, run
  `agent-cache.py set`: it sets `"subagentPromptCacheTtl": "1h"` in `~/.claude/settings.json`. Without it Claude Code
  gives agents a five-minute cache, and every wait longer than that, for a build, a run or a rest, makes an agent
  write its whole context to the cache again. `agent-cache.py report <transcript>` shows from a transcript which
  lifetime the cache writes got.
- **Pacing.** So that the 5-hour quota lasts until it resets, after each 20 minutes of an agent's work the agent and
  the session rest 40 minutes.
  - `pace-agent.py` counts the work from the agent's transcript, cumulatively: the time up to each of its replies, and
    commands of up to 3 minutes. Longer commands, waits for runs and rests count as rest, at half their length
    against the work.
  - A rest never exceeds 59 minutes, so that the agent's prompt cache, which lasts an hour, outlives it (@Pauli,
    Nov 13). `pace-agent.py` caps the rest it asks for; the work a capped rest leaves unpaid brings the next rest
    sooner. A rest sent late is still at most 59 minutes.
  - The session runs `pace-agent.py` as a background command and starts it again each time it ends. When it prints
    `REST DUE: N minutes`, the session sends the agent "Rest N minutes" and reports the agent's progress to @Pauli.
  - Each agent's brief says what to do then: push the work in progress, start one background command that ends after
    N minutes, make no other call until it has ended, and go on where it was. An agent does not rest on its own.
  - While the work is paced, the session runs no other watch of the agent. It wakes when a rest begins or ends, and
    otherwise every 50 minutes, within the hour its own prompt cache lasts.

# The session that drives the flattening

Nov 16, 2026. These are the steps of the session that drives the flattening work. The session starts one agent for
one step, examines what the agent did, records it, and starts the next agent. A session that starts with no context
can continue the work from this file. The requirements, the gates, the ladder and the phases are in
[README.md](README.md), and the rules of the calculation in [Model.md](Model.md). A session that needs the context
of the fork reads [../Introduction.md](../Introduction.md).

The folders `<clone>`, `<work>/flat`, `<work>/flat-int` and `<work>/flat-results` are in README.md, "The branches
and the folders". This machine keeps them between sessions.

**The model.** The agents are `claude-fable-5-2` at effort `high` (@Pauli, Nov 16). The agent type `flatten-step`
(`.claude/agents/flatten-step.md` on this branch) sets both, and holds the rules that each agent follows. Do not use
`plan-step` for this work: it runs at effort `max`.

**One agent works at a time** (@Pauli, Nov 16). The session does not start a second agent, a review agent, or a
watch of the agent.

**These need @Pauli's word.** A wider tolerance. A case set apart (GATE-7). A change of [Model.md](Model.md) that
changes what a case must draw. A push to `wip/`, `base`, `basewithfixes`, `dev` or any shared branch. A force push.
A deleted branch.

**These need no word.** To write a brief and start an agent. To accept or refuse a step. To commit and push
`docs/process-coordination` and `feature/flattening`, neither of which starts a workflow. To publish the page.

**After phase 0, a next step needs a consultation** (@Pauli, Nov 17: "do not get into further steps without
consultation"). The session tells @Pauli which step it means to start, and why, and it starts no agent before he
answers. The session of Nov 17 takes this to hold until he says otherwise.

## 1. Find where the work stands

1. Read README.md, Model.md and `Status.md`, when it exists.
2. Read the last review in `reviews/`, and the last brief in `briefs/`. A brief that has no review is a step that did
   not end, or that the session did not examine.
3. Look at the code: `git -C <work>/flat log --oneline -15` and `git -C <work>/flat status --short`. Changes that
   are not committed are the work of an agent that stopped.
4. Before the first step: make the branch and the worktree, if they are not there.

   ```
   git -C <clone> fetch origin
   git -C <clone> branch feature/flattening origin/feature/multiply-blending
   git -C <clone> worktree add <work>/flat feature/flattening
   ```

   `feature/flattening` is rooted at the tip of `feature/multiply-blending` on GitHub, which carries the ink of task
   7, and never at anything else (README.md, "Why it sits on the ink of task 7").

## 2. Write the brief

One step is one case of the ladder, or one piece of a phase that one agent can do with its context in good order:
about one or two hours of work. Cut a larger step.

Write the brief to `briefs/<step>.md`, in Simplified Technical English, and commit it before the agent starts. The
agent starts with no context, so the brief holds all that it needs:

- **The step:** one sentence that says what is delivered.
- **The premise:** what the step rests on. The agent stops if it finds the premise false.
- **Read first:** the files, with full paths. Model.md, the design notes of the stage, and the file of the case.
- **Where things are:** the files and lines of the code that the step touches, so that the agent does not look for
  them again. Take them from the last reviews.
- **What to deliver,** and what the agent may change for it.
- **The gates:** the commands, and what each must show.
- **Stop when:** the conditions on which the agent stops without a result.
- **The report:** what the last message of the agent holds.

For a case, write the file of the case first (README.md, "The file of a case"), with probe values calculated from
Model.md by hand. Give it to the agent in the brief. The agent does not change it.

## 3. Start the agent

**The lifetime of the agent's prompt cache.** The practice of CLAUDE.md for the agents' cache does not bind this
work (@Pauli, Nov 16): the session chooses the lifetime by the step.

- An agent whose cache is older than its lifetime writes its whole context to the cache again. So a step in which
  one command can run longer than five minutes gets the one-hour cache:

  ```
  python3 <clone>/doc/devdocs/SvgDevLoop/agent-cache.py set
  ```

- A step whose builds and test runs each end within five minutes can use the five-minute cache. It is the default
  when `~/.claude/settings.json` holds no `subagentPromptCacheTtl`.
- Decide from a measurement. `agent-cache.py report <transcript>` shows from the transcript of an agent how often
  its cache expired, and phase 0 measures the time of a build and of a test run on this machine. The transcript of
  an agent is `~/.claude/projects/<project>/<session>/subagents/agent-<id>.jsonl`.

Start one agent of the type `flatten-step` with the brief as its prompt. If the agent type is not known, the session
started before the file was there: @Pauli loads it with `/agents`, or starts the session again.

The session goes on when the agent ends. It runs no watch. An agent in the session's background stops when the
session's turn is interrupted, and cannot be resumed. Then look at what it committed (step 1), and start a new agent
for what is left.

**The session's own cache** lasts one hour. A session that waits longer for an agent writes its whole context again
when it goes on. While an agent works, keep a background command that ends after 50 minutes, and start it again
when it ends. When the context of the session is large, let a new session drive: this file, the briefs and the
reviews hold what it needs.

## 4. Examine what the agent did

Do not take the agent's report as the result. Measure.

1. Make sure that the agent did not change what is the session's: the harness, the tolerances and the cases.

   ```
   git -C <work>/flat diff --stat <commit before the step> HEAD -- src/MapModel/Map_Flatten.Tests/Harness src/TestFiles/flattening
   ```

   Phase 1 writes the harness, so this check applies from phase 2 on. A change there that the brief did not ask for
   refuses the step.
2. Run the ladder on the agent's commit.

   ```
   python3 <clone>/doc/devdocs/Flattening/flat.py run <work>/flat <work>/flat-results
   ```

   Nothing that passed before may be pending or failed now. The case of the step passes, or is pending with the
   reason that the agent gave.
3. Read the diff of the step. Outside `src/MapModel/Map_Flatten` and its tests, a change is as small as the step
   needs, because the branch is offered to Purple Pen.
4. Merge the commit into `<work>/flat-int` and run the tests that the work must not move (README.md, "Where the work
   runs"). The tests that passed there before the work still pass.

A step that fails one of these goes back to the same agent with what failed, when the agent's context is still of
use. Otherwise a new agent gets a brief that names what is left.

## 5. Record the step

1. Write `reviews/<step>.md`. **The review marks the step done.** It holds:
   - the step, and what it adds;
   - the constructs the stage now declares, and the ones it does not;
   - the numbers that the session measured: the probes, the samples that differ, the count of pieces;
   - the commits;
   - the model id and the effort setting of the agent;
   - what the agent's report said that the next step must know, and the doubts.
2. Make the table and the page again, and publish the page.

   ```
   python3 <clone>/doc/devdocs/Flattening/flat.py status <work>/flat-results <clone>/doc/devdocs/Flattening
   python3 <clone>/doc/devdocs/Flattening/logbook.py <work>/flat-results <work>/flat-results/page
   ```

3. Commit the documents and push the two branches.

   ```
   git -C <clone> add doc/devdocs/Flattening
   git -C <clone> commit -m "Record the flattening step <step>"
   git -C <clone> push origin docs/process-coordination
   git -C <work>/flat push origin feature/flattening
   ```

4. Tell @Pauli in a few lines: the step, its numbers, and the doubts that he must see. Then go to step 2.

## 6. At the end of a phase

1. Bring the handover up to date ([../NotPurplePenProgress.md](../NotPurplePenProgress.md), task 12).
2. Ask @Pauli for the full run on `wip/linux/flattening`: `basewithfixes` with the feature merged in. It is the first
   run of the work on Windows, and the first on a machine other than this one (GATE-5).

## Before phase 5

`fix/colour-transparency` is merged into `feature/flattening`, so that the real maps give the opacity half of the
work its input (README.md, "The transparency of a colour"). A session that reaches case 11 with that branch unmerged
stops and says so.

## What phase 0 changed

Phase 0 measured the path operations (Nov 17): [Measurements.md](Measurements.md), and the review
[reviews/phase-0.md](reviews/phase-0.md). These are its results for the briefs. **The structure of the cut is open.**
The second point is not measured, and it needs @Pauli's word on GATE-6.

- **A division of the page into pieces that do not overlap cannot be built on these operations as it stands.** It
  needs the `Difference` of each run and the union of all runs above it. On a whole map that takes too long: 860 s
  for the smallest map, where 9 of 40 operations give no result. In tiles of 5 to 25 mm, 0.4 % to 13.7 % of the
  operations give no result or a wrong result, and most of these report no failure. The session of Nov 16 planned
  this division: a knockout colour was one piece that cuts all that is below it.
- **A structure that is not measured.** An opaque colour stays whole, in its drawing order, and covers what is below
  it with no cut. Only a run with blending, or with an opacity below 1, is cut: its marks are intersected with what
  lies below them, and each part is drawn above, with the mixed colour. The handover describes the flattening so:
  "Each such colour is cut against each colour below it". This needs `Intersect` only. Its pieces overlap, so GATE-6
  cannot stay as README.md gives it. A separation (SEP-1) is then the same pieces, each drawn with the coverage of
  the ink, and not one mask of the ink as geometry.
- **Each result of a path operation needs a check** that uses samples and no path operation. A right result differs
  from its operands in single samples along its outline. A wrong result differs in areas: one sample or more
  differs together with its eight neighbours. What the stage does when the check fails is not known.
- **Tiles make the time practical.** `LordHill.ocd` in tiles of 10 mm takes 16 s with the machine alone, and
  `teanaway.omap` 315 s on a loaded machine. They do not cure the failures.
- **Unite the marks of a run by pairs.** One after the other, and `SKPath.OpBuilder`, pass 10 s at 687 marks. Check
  the union too: the union of the contours of `LordHill.ocd` is wrong with no failure.
- **FL-8 needs a rule for the resolution scale of `SKPaint.GetFillPath`.** At the default, the outline of a line of
  0.35 mm is wrong by 0.158 mm. At 40 for each millimetre it is wrong by 0.0044 mm, and at 400 by 0.0006 mm.
- **A pattern reaches the stage as elements inside a clip** (case 8). One `Op(Intersect)` for each element is 351 s
  of the 357 s in which the probe took the largest map as marks, and three of four elements end outside the clip.
- **Text** is 231 to 315 calls a map (case 9). The probe did not keep it.
- **The test maps hold case 5:** the overprinted black of `LordHill.ocd` is runs 14 and 16, with a knockout run
  between them, and that of `McHugh Creek.omap` is runs 8, 10, 12 and 16. `teanaway.omap` has 34 overprinted
  colours of 35, so no colour of it knocks out.
- **The results repeat** on this machine, bit for bit, in each process. Another machine is not measured (GATE-5).
- **The loop is short:** the first build of `Map_Skia.Tests` takes 34.5 s, a build with nothing changed 6.3 s, and
  one test 3.7 s. So a step whose commands are builds and test runs can use the five-minute cache. The agent of
  phase 0 had the one-hour cache, and it waited four times for more than five minutes, each time for a run of the
  probe.

## What the session knows for the next briefs

These are not decisions. They are what the session of Nov 16 found when it examined the plan, for the briefs of
phases 1 to 3. Phase 0 changed what they said of the cut and of tiles: see "What phase 0 changed".

- **The stage has four parts.**
  1. A target that keeps the marks of a drawing as paths in the coordinates of the page, with the colour, the
     opacity and the blending of each run.
  2. The model of Model.md as one function without state: from the colours that cover a point, the lowest first,
     and the blend mode, to one CMYK value.
  3. The cut: the pieces, made with the path operations. Its structure is open.
  4. The calls that draw the pieces on the target behind the stage.

  The raster reference and the vector stage both call part 2. So GATE-2 examines only the geometry, and GATE-M and
  GATE-1 examine part 2.
- **GATE-3** needs a target of its own that fails on `PushBlending` and on a colour with an alpha below 1.
  `RecordingGraphicsTarget` keeps closures, not data.
- **GATE-6** can count, sample by sample and without antialiasing, how many pieces cover a sample. Then it does not
  use the operations that it examines. If the pieces of the stage overlap, GATE-6 changes first.
- **GATE-4** needs constructs that tell case 1 from case 2, which make the same calls: a stage that changes nothing
  must leave case 2 pending, not failed. The harness finds the constructs of a case from the calls that the case
  makes.
- **The harness writes, for each case and mode,** one JSON file with the numbers of each gate, and the images that
  the page shows: the drawing before the stage and after it, their difference, the pieces in false colours, and the
  four channels. `flat.py` and `logbook.py` read only these files.
- **`logbook.py` and `publish-logbook.sh`** share their code with `../Translations/`: the drawing of the plan as a
  course, the page, and the publishing of a folder to a branch and to a folder of `gh-pages`. `build.yml` can copy a
  list of branches into the site, not one more block for each page.
- **The new projects** `Map_Flatten` and `Map_Flatten.Tests` must join `PPen.slnx` and the test workflow before the
  first full run.
- **Case 11** needs the colour levels with overprint ink, which task 14 adds. Until then the case can set the flag
  in its test.
- **FL-9** does not say which shape grows. If the shape that cuts grows, the lower piece gets smaller and a gap
  opens. Ask @Pauli before phase 4.
- **Three points of Model.md** wait for @Pauli's word (its last section). None changes cases 1 to 5.

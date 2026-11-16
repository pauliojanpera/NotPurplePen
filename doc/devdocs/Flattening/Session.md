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

```
python3 <clone>/doc/devdocs/SvgDevLoop/agent-cache.py set
```

Start one agent of the type `flatten-step` with the brief as its prompt. If the agent type is not known, the session
started before the file was there: @Pauli loads it with `/agents`, or starts the session again.

The session goes on when the agent ends. It runs no watch. An agent in the session's background stops when the
session's turn is interrupted, and cannot be resumed. Then look at what it committed (step 1), and start a new agent
for what is left.

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

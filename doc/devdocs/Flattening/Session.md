# A session that does the next case of the flattening

Nov 16, 2026. This is the brief of a session, or of an agent that a session starts, that begins with no context and
does the next case of the flattening ladder. Do the steps in order. The requirements, the gates and the ladder are
in [README.md](README.md). A session that needs the context of the fork reads [../Introduction.md](../Introduction.md).

@Pauli, or the session that starts the agent, gives a prompt such as:

> Do the next cases of the NotPurplePen flattening. Follow `doc/devdocs/Flattening/Session.md` on the branch
> `docs/process-coordination`. You are `claude-fable-5-2`, at effort `max`.

The model id and the effort setting go into each review. If the prompt gives others, use those of the prompt.

**One agent works at a time, and one case at a time. Do not start an agent of your own**, and do not start one to
review your own work: the gates review it. @Pauli set this.

**You own the flattening code.** `src/MapModel/Map_Flatten` and the tests of this work are yours to design: name
things well, find the common form of things that repeat, and correct a stupidity where you meet one. **You do not
own the rest of the repository.** A change to the map code, to the targets or to the application is as small as the
case needs, because these branches are offered to Purple Pen, and a diff that carries a restructure is not offered.
Where you see a fault outside the case, say so when you stop; do not fix it on the way past.

**These steps need no approval, and no question to @Pauli.** Build. Run the tests. Write and change the files of
this work. Commit. Push `feature/flattening` and `docs/process-coordination`, neither of which starts a workflow.
Retry a command that failed for an ordinary reason, such as a timeout or a busy file. Work through an ordinary
obstacle with the access you have, instead of stopping.

**These need @Pauli's word.** A wider tolerance. A case set apart (GATE-7). A push to `wip/`, `base`,
`basewithfixes`, `dev` or any shared branch. A force push. A deleted branch.

**The inner loop is on this machine.** The build and the tests run here, and take seconds for one case. Do not push
a branch to start a run, and do not wait for one. The Windows suite runs on `wip/linux/flattening` at the end of a
phase, and the session that ends the phase starts it.

## 1. Get the files

The machine keeps its clones and worktrees between sessions. Use the worktree that is there; make it only if it is
not.

```
git -C <clone> worktree add <work>/flat feature/flattening
```

If `feature/flattening` does not exist, this is the first session. Make it:

```
git -C <clone> branch feature/flattening feature/multiply-blending
```

It is rooted at `feature/multiply-blending`, which carries the ink of task 7, and never at anything else
(README.md, "Why it sits on the ink of task 7").

Before phase 5, `fix/colour-transparency` is merged into it, so that the real maps give the opacity half of the work
its input (README.md, "The transparency of a colour"). A session that reaches case 11 with that branch unmerged
stops and says so.

Read [README.md](README.md). Read the review of the last case that has one, in `reviews/`.

## 2. Take the next case

```
python3 doc/devdocs/Flattening/next-case.py <work>/flat
```

It names the next case of the ladder that has no review, and prints its brief: what the case adds, which
requirements it first meets, which constructs the stage must declare for it, and where its fixture goes. If it says
"all cases are done", stop, and say so (step 6).

In phase 1 there is no script yet. Then the case is phase 1 itself: write the harness, the gates and the scripts
that README.md names, and nothing of the flattening.

## 3. Do the case

1. **Write the case first.** Add its fixture and its test before the code that makes it pass. A declarative case is
   a drawing, as `SimpleDrawingTests` holds its cases. The test runs the case twice, with the stage and without it,
   and applies the gates.
2. **Declare nothing you have not built.** The stage declares the constructs it flattens (FL-10). A construct joins
   the list only when the whole requirement behind it is met. Until then the case is pending with that construct
   named, which is not a failure (GATE-4).
3. **Write the code.** Follow the repository's style: a header comment on each class and method, explicit types
   instead of `var`, and names that say what they are.
4. Check that nothing else moved. The tests of `Map_Skia.Tests`, `Map_PDF.Tests` and `Map_Svg.Tests` must pass as
   they did, because the stage is off unless it is asked for.

## 4. The gates

```
bash doc/devdocs/Flattening/run-flatten-tests.sh --case <id> <work>/flat <results>
```

The case passes when the stage declares every construct it uses, and:

| Gate | What it says |
| --- | --- |
| GATE-1 | The flattened drawing rasterizes to the unflattened drawing, within the tolerance |
| GATE-2 | The vector stage equals the raster reference, within the tolerance |
| GATE-3 | The flattened calls hold no `PushBlending` and no alpha below 1 |
| GATE-5 | The same input flattens to the same geometry twice |
| GATE-6 | The pieces do not overlap each other, and none has zero area |

`compare-images.py` gives the number behind GATE-1 and GATE-2: the pixels outside the tolerance, the greatest level
difference, and a difference image. Read the difference image when a gate fails. It says where the error is.

**Do not widen a tolerance. Do not set a case apart.** GATE-7 is @Pauli's to grant. A case you cannot make pass is
left pending with its reason, and you stop and say so (step 6).

When the case passes, run the whole ladder once:

```
bash doc/devdocs/Flattening/run-flatten-tests.sh <work>/flat <results>
```

Nothing that passed before may be pending or failed now.

## 5. Record the case, and take the next

1. Commit the code on `feature/flattening`. The message tells what the change does, and what in the code made it
   necessary. No trailer, and no paragraph on why the feature is wanted.

   ```
   git -C <work>/flat add src
   git -C <work>/flat commit -m "<what the change does>" -m "<what in the code required it>"
   git -C <work>/flat push origin feature/flattening
   ```

2. Write the case's review in `doc/devdocs/Flattening/reviews/<id>.md`. **The review is what marks the case done.**
   It holds:
   - the case, and what it adds;
   - the constructs the stage now declares, and the ones it still does not;
   - the numbers of GATE-1 and GATE-2: the pixels outside the tolerance, and the greatest level difference;
   - the commits;
   - the model id and the effort setting;
   - your doubts, in plain words, for the session that comes next.
3. Make the table again, and commit the documents.

   ```
   python3 doc/devdocs/Flattening/status.py <work>/flat doc/devdocs/Flattening
   git -C <docs> add doc/devdocs/Flattening
   git -C <docs> commit -m "Record the flattening case <id>"
   git -C <docs> push origin docs/process-coordination
   ```

4. Take the next case (step 2). Stop after the last case of the phase, or when about four hours of work are done.

## 6. Stop

Tell @Pauli, or the session that started you, in a few lines:

- the cases done, and the cases left pending with their reason;
- the constructs the stage declares now;
- the numbers of the last run: passed, pending, failed;
- a gate you could not meet, and what you found out about it;
- the doubts he should see.

Say "all cases are done" if `next-case.py` said so.

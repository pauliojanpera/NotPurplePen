# The glossary work: handover

Nov 21, 2026. This file tells a new session how to continue the glossary work. The glossary is
[Glossary.md](Glossary.md). It gives the names of the concepts that the specification of the palette-layer architecture
needs. The work on the names comes before the new text of [Specification.md](Specification.md).

## Read first

Read these in this order:
1. [../Introduction.md](../Introduction.md): what NotPurplePen is, and the rules of all work.
2. [README.md](README.md): how the palette-layer work is organized.
3. The decisions of Nov 20 in [Progress.md](Progress.md): "The structure of the passes", "Earlier decisions are
   negotiable" and "The names".
4. The note "The structure of the passes (@Pauli, Nov 20)" in section 02 of [Specification.md](Specification.md), and
   the notes of Nov 20 in its sections 03 and 05.
5. All of [Glossary.md](Glossary.md). Its preface gives the rules of the names.

## The rules of the work

@Pauli gave these rules in the session of Nov 20 and 21:
- The glossary is joint work. Each term is @Pauli's decision. The session does not choose a term itself, also not by
  the use of a word or by a rule of the names (Nov 21).
- Everything is @Pauli's to decide (Nov 21). Where the sources answer a question, the session presents the answer to
  @Pauli as a question, and he decides.
- The session presents one question at a time (Nov 20, Nov 21).
- The session does not make proposals that @Pauli did not ask for (Nov 20).
- When @Pauli uses a vague reference, the session asks him to make it exact (Nov 20, the preface of the glossary).
- The code of the palette-layer branches is not a source of the new definition of `Map` that the glossary builds
  (Nov 21).
- A word of the specification is not an authority. @Pauli wrote some of its concepts in passing, for example *full
  coverage* (Nov 21).
- An ordinary word, for example the adjective *yhtenäinen* or the noun *peitto*, is not reserved for one concept. A
  term is the whole expression (Nov 21).

In the session of Nov 20, the session chose five terms by their use without a question (commit 06f4c824d), and it
recorded the meaning of the palette layer before @Pauli decided it (commit 740d9b778). @Pauli objected to both. On
Nov 21, the session based its recommendations for G = 1 on a name of the work's code, on a phrase of the
specification and on the other uses of ordinary words, and not on the theory of the concept. @Pauli objected to each
of these.

### How to make a question

- **The question stands alone.** @Pauli does not look back in the conversation to understand it. The question gives
  the present use of the word with quotations and their sources, an example, the options and a recommendation.
- **The session reads the sources before it asks.** The sources are the specification, the decisions in Progress.md
  and the tickets, and the code of upstream. The session does not ask @Pauli for facts that the sources give. It reads
  them, and it presents the result to him.
- **The question gives the theoretical basis.** Before a question, the session does a proper investigation with its
  theoretical knowledge, for example of mathematics and of the theory of computer graphics and compositing, so that
  @Pauli has the theoretical basis of the choice (Nov 21). The counts of use, the words of the specification and the
  code of the work are not a basis alone. For example, the theory showed that G is a coverage: the indicator function
  of a set of the plane, and in compositing the alpha has two parts, the coverage and the opacity.
- **The length is moderate.** @Pauli objected to answers that were too long, to answers that were too short, and to
  lists of fragments.

### How to record

- After a decision, the session records it at once in Glossary.md, commits, pushes, and tells @Pauli what it recorded,
  with the commit.
- Only @Pauli's decisions have his name and date. A fact has its source. A name that is not decided is marked
  "proposed name".
- A decision on a Finnish word concerns the Finnish word only, unless @Pauli says otherwise (the entry of *kerros*,
  Nov 20).
- When a decision changes a word that other entries use, the session changes those entries in the same commit.
- The glossary is in Simplified Technical English, with the Finnish and the English term of each entry. The lines are
  at most 120 characters long, and the line endings are LF.
- A commit message tells what the change does and what required it, without a co-author line.
- The glossary tells what the terms mean now. It does not tell what a word meant before, or what the tickets, the
  decisions or the specification called a concept. The commits and the transcripts keep that (@Pauli, Nov 21).

## Where the work is

### Decided

Glossary.md has all the decisions of Nov 20 and Nov 21, each with @Pauli's name and date:
- The rules of the names, in the preface.
- The words that are not names: *malli*, *karttamalli*, *karttakoordinaatit* and *maailmankoordinaatit*, *muunnos*,
  *litistys* and *litistäminen*, *toisto* and *toistuva*, *kuvio*, *täyttö*, *reuna*, *osa*, *ajo*, *kerros*, *ryhmä*,
  and *separaatio* and *erottelu*.
- The readings of "the model" in the decisions of Nov 20, in the entry of *malli*.
- The terms *symboli*, *vaihe*, *viiva* and *kynä*, *taitepiste* and *jana*, the layers of `SymLayer`, *värierottelu*
  and *palettierottelu*, and *palettiväri*.
- The names of the structure of the passes: `PaletteSeparation` · palettierottelu and `PaletteCutter` ·
  palettileikkuri, and *johdos* · *derived value* (the English term on Nov 21).
- The terms of `IGraphicsTarget`: *piirtokohde* · *graphics target* (Nov 21).
- The terms of α: *peittävyys* · *opacity* (Nov 21).
- The coverages: *jaksollinen peitto* · *periodic coverage* (G, in place of *jaksollinen geometria*), *täysi peitto* ·
  *full coverage* (G = 1) and *peittoprototyyppi* · *coverage prototype* (Nov 21).
- A point and an angle, and a place on a path: *asema* · *pose* and *kohta* · *position along the path* (Nov 21). They
  replace *paikka* · *position*.
- The case that a channel solves with an abstraction of its own: *perustapaus* · *base case* (Nov 21).
- The group that pass 2 composites for a palette color: *palettivärin läpinäkyvyysryhmä* · *transparency group of a
  palette color*, in place of *palettitaso* (Nov 21).
- The triple (`Symbol`, `SymColor`, G): `SymbolCoverage` · symbolin palettiväripeitto · *palette color coverage of a
  symbol*, in place of the proposed `SymbolSeparation` · symbolierottelu (Nov 21).
- The shape that a pen draws and the band that the pen covers: *viivamuoto* · *stroked shape* (`StrokedShape`) and
  *kynänjälki* · *stroke shape* (Nov 21).

The five terms that the session chose by their use in commit 06f4c824d are all decided on Nov 21.

### Open

Present these to @Pauli, one question at a time:
1. **The items marked "Open" in the glossary.** On Nov 21 they are:
   - the identifier of a `Symbol` to which a drawn object of a file can refer
   - whether the shared structures that are not periodic, for example the shared shapes of a glyph, are prototypes
   - whether the logical path is `LineLikeSymbol.Path`
   - whether a part in "the parts as its pieces and offsets" (Nov 20) is a segment or a line component, and the exact
     content of a piece
   - when two configurations are the same (009, question 1)
   - whether a hatching is a prototype of its own (009, question 3), and the meaning of `RotateMode.ManualOnly`
     (009, question 4)
   - which channel conversions stay in the encodings
   - the measure in which a stroke keeps the dash pattern and its phase (point 2 of 6 in the review of 011)
   - how the encodings relate to `IGraphicsTarget`, and its place when pass 2 and the encodings draw from the
     intermediate layer
   - whether the highlights stay a direct drawing (000).

### After the glossary

In the plan that the session gave on Nov 20, the specification is written again by the glossary and by the structure
of the passes after the glossary work. Then the tickets 009 to 013 follow it. The tickets, the older decisions in
Progress.md and the original text of the specification still use the earlier words.

## Where things are

- **The main clone** `~/Documents/Projects/NotPurplePen` is on `docs/process-coordination`. The glossary and the other
  documents are there. Do not change its branch. Commit and push there.
- **The code of upstream:** the worktree `~/Documents/Projects/npp-worktrees/upstream` is detached at the master of
  upstream, bcd20c43c (fetched on Nov 20). The names of upstream in the glossary come from it. The files of
  `src/MapModel/MapModel` and `src/MapModel/Graphics2D` are the same there as at d48f4456.
- **The code of the work:** the worktree `~/Documents/Projects/npp-worktrees/text-geometry` is at 2ad8c6f39. The names
  marked "work" in the glossary come from it.
- **The transcript** of the session of Nov 20 and 21:
  `~/.claude/projects/-home-pauli-Documents-Projects-NotPurplePen/24af0e14-e17c-4f92-bc24-2c6815053a55.jsonl`.

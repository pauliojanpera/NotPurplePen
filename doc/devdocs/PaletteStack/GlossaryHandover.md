# The glossary work: handover

Nov 21, 2026. This file tells a new session how to continue the glossary work and the work on the specification that
follows it. The glossary is [Glossary.md](Glossary.md). It gives the names of the concepts that the specification of
the palette-stack architecture needs. [Specification.md](Specification.md) is written in its terms (Nov 21 and 22).

## Read first

Read these in this order:
1. [../Introduction.md](../Introduction.md): what NotPurplePen is, and the rules of all work.
2. [README.md](README.md): how the palette-stack work is organized.
3. The decisions of Nov 20 in [Progress.md](Progress.md): "The structure of the passes", "Earlier decisions are
   negotiable" and "The names".
4. Section 01 of [Specification.md](Specification.md), which gives the structure of the passes.
5. All of [Glossary.md](Glossary.md). Its preface gives the rules of the names.
6. The decisions of Nov 21 to 23 in [Progress.md](Progress.md), from "The division of the specification" to "The
   approval of the specification": the review of the specification, the names of the architecture, of the work and
   of the branches, the new branch of the epic, and the approval.

## The rules of the work

@Pauli gave these rules in the sessions of Nov 20 to 23:
- The glossary is joint work. Each term is @Pauli's decision. The session does not choose a term itself, also not by
  the use of a word or by a rule of the names (Nov 21).
- The session chooses meaningful names for the concepts that have no name yet, on the basis of the glossary work
  (Nov 23).
- Everything is @Pauli's to decide (Nov 21). Where the sources answer a question, the session presents the answer to
  @Pauli as a question, and he decides. From Nov 23, the session resolves a matter itself, as well as it can, when
  the knock-on effects of its choice are not unreasonable, and records the choice as its own (Progress.md, "The
  session resolves matters").
- The session presents one question at a time (Nov 20, Nov 21).
- The session does not make proposals that @Pauli did not ask for (Nov 20).
- The session gives options only where the choice is real. It does not add options only to involve @Pauli (Nov 23).
- When @Pauli uses a vague reference, the session asks him to make it exact (Nov 20, the preface of the glossary).
- The session starts no agents and does not run `agent-cache.py`, although `CLAUDE.md` asks every session to run it.
  It does not start the script that archives the session (Nov 18, [README.md](README.md)).
- The code of the palette-stack branches is not a source of the new definition of `Map` that the glossary builds
  (Nov 21).
- A word of the specification is not an authority. @Pauli wrote some of its concepts in passing, for example *full
  coverage* (Nov 21).
- An ordinary word, for example the adjective *yhtenäinen* or the noun *peitto*, is not reserved for one concept. A
  term is the whole expression (Nov 21).
- A general word can have its general meaning in the documents. Where it means a concept that the specification
  defines by another name, the documents use that name (Nov 22, Progress.md, "General words").
- A reference to a section of the specification without a version names the present text (Nov 22).
- A mistake of the session is corrected at once. The session does not put it to @Pauli as a question (Nov 22).

In the session of Nov 20, the session chose five terms by their use without a question (commit 06f4c824d), and it
recorded the meaning of the palette layer before @Pauli decided it (commit 740d9b778). @Pauli objected to both. On
Nov 21, the session based its recommendations for G = 1 on a name of the work's code, on a phrase of the
specification and on the other uses of ordinary words, and not on the theory of the concept. @Pauli objected to each
of these.

### How to record

- After a decision, the session records it at once in Glossary.md, commits, pushes, and tells @Pauli what it recorded,
  with the commit.
- The glossary gives no dates, attributions or sources in parentheses (@Pauli, Nov 21). The list under "Decided" and
  the commit messages tell whose decision a term is and when. A name that is not decided is marked "proposed name".
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

These are @Pauli's decisions of Nov 20 and Nov 21. Glossary.md gives their terms:
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
- The identity of a `Symbol`: `Symbol.Key` · *symbolin avain* · *symbol key*. The maker of the `Symbol` gives it,
  `Map` from a counter and the course map from `EventDB` (Nov 21). Its basis is the future of `Map` and the SVG
  channel, in the decisions of [Progress.md](Progress.md).
- The identity of a `CourseObj`: `CourseObj.Key` · *rataobjektin avain* · *course object key*, a key that
  `CourseFormatter` or `TopologyFormatter` gives at the place where it makes the `CourseObj`, from the role of the
  place, the identifiers of `EventDB` and the `CourseLayer`. The terms of `CourseObj` are *rataobjekti* · *course
  object* (Nov 21).
- The path that the parts of a line are pieces and offsets of: *peruspolku* · *base path*, in place of *looginen
  polku*, *keskiviiva* and *pohjapolku* (Nov 21).
- The shared shapes of a glyph are not coverage prototypes, and the code names of the coverage prototype are
  `CoveragePrototype` and `CoveragePrototype.FullCoverage` (Nov 21).
- *viivan osa* · *part of a line*: one line component on one piece of the base path, with its offset, as a derived
  value. A segment divides a piece for the fitting, and a piece is a start and an end along the base path (Nov 21).
- The same configuration: the same coverage, so that M₁⁻¹M₂ is a symmetry of G, which a normal form decides (Nov 21).
- A hatching is a coverage prototype of its own, and `ManualOnly` is the same as `Always` in the specification
  (Nov 21).
- `Stroke.DashPattern` and `Stroke.DashPhase` are in the measure of the channels, and the dash structure in the
  measure of the line is the fitted pattern of a part of a line (Nov 21).
- The own channel conversions of an encoding are only those into the vocabulary of its channel and the adaptations
  to its deviations. `Map` offers all the geometric conversions (Nov 21).
- An encoding writes its channel directly, and `IGraphicsTarget` stays beside the palette stack for the templates,
  the highlights, the control descriptions and the old drawing code. So the highlights stay a direct drawing (Nov 21).
- The name of OCAD for a spot ink in its files: *spottivärierottelu* · *spot color separation* (Nov 22).
- The term of `CourseLayout`: *ratojen asettelu* · *course layout*, which the session chose by the rule of Nov 23.
- The terms of the areas and of the graphics objects, which the session chose by the rule of Nov 23 for ticket 012:
  `AreaSymDef`, `AreaComboSymDef` · *alueen symbolimäärittely* · *area symbol definition*, `AreaSymbol` · *aluesymboli*
  · *area symbol, area object*, `SymPathWithHoles.MainPath`, `SymPathWithHoles.Holes` · *pääpolku*, *reikä* · *main
  path*, *hole*, and `GraphicsSymDef`, `GraphicsAreaSymbol`, `GraphicsLineSymbol` · *grafiikkaobjekti* · *graphics
  object*.
- The top left corner of a text run is in the coordinates of its placement, whose Y axis points up, and its glyphs
  lie below it. The mirror of the Y axis is a part of the text run, and the innermost step of the placement of a text
  is its pose (Nov 23, with the approval of ticket 013).
- The terms of the texts and the rectangles, which the session chose by the rule of Nov 23 for ticket 013:
  `PointSymDef`, `PointSymbol` · *pistesymbolin symbolimäärittely*, *pistesymboli* · *point symbol definition*, *point
  symbol*, `TextSymDef` · *tekstin symbolimäärittely* · *text symbol definition*, `TextSymbol` · *tekstisymboli* · *text
  symbol*, *muotoiltu teksti* · *formatted text*, `LineTextSymbol` · *viivan varren teksti* · *text along a line*,
  *merkkiväli*, *sanaväli* · *character spacing*, *word spacing*, *grafeemi* · *grapheme*, *tekstin kehys* · *framing
  of a text*, *varjo* · *shadow*, *kehyssuorakulmio* · *framing rectangle*, *alleviivaus* · *underline*, *tekstin
  keskipiste* · *center point of a text*, `RectangleSymDef`, `RectangleSymbol` · *suorakulmion symbolimäärittely*,
  *suorakulmiosymboli* · *rectangle symbol definition*, *rectangle symbol*, and *ruudukko*, *ruudukon viivat*,
  *ruudukon tekstit* · *grid*, *grid lines*, *grid texts*.
- The names and the terms of ticket 009, which the session chose by the rule of Nov 23 and @Pauli approved with the
  ticket: `HatchingPrototype` · *viivoituksen peittoprototyyppi*, `AreaPatternPrototype` · *aluekuvion
  peittoprototyyppi*, `SymbolCoverage.Transformation`, `CoveragePrototype.GetNormalForm` · *normaalimuoto*,
  *perusalue*, *siirtosymmetria* and `SymbolCoverage.ExpandPrototype` · *kuviotäytön laajennus* (Nov 23).
- *valitut glyyfin osat* · *selected glyph parts*, in place of *valitut elementit*, as a fill mode selects each glyph
  part (Nov 23, with ticket 009).
- The terms of ticket 010, which the session chose by the rule of Nov 23, and which @Pauli approved with the ticket
  (Nov 24): `CourseAppearance` · *ratojen ulkoasu* · *course appearance*, `OverprintMode` ·
  *päällepainatustila* · *overprint mode*, *päällepainatusväri* · *overprint color*, *sekoitusväriavaruus* · *blending
  color space*, `RenderOptions.minResolution` · *piirron resoluutio* · *resolution of a drawing*, *leikkausobjektit
  piirron resoluutiolla* · *clipping objects at the resolution of a drawing*, *kuviosivellin* · *pattern brush*,
  `RecordingGraphicsTarget` · *tallentava piirtokohde* · *recording graphics target*, and the code names
  `TransparencyGroup`, `IPaletteStackEncoding`, `IPaletteStackEncodingProvider`, `SkiaEncoding`,
  `SymbolCoverage.GetClippingObjects(float)`, `IDeferredDrawingTarget`, `AreaPatternPrototype.Tile` and `PatternTile`.
  The code names of the ticket that the glossary got later (Nov 24): `RenderOptions.overprintMode`,
  `CourseAppearance.overprintMode`, `MapDisplay.OverprintMode`, `Controller.OverprintMode` and
  `RenderOptions.drawColorsDirectly`.
- The code names that the implementation of ticket 010 adds, which its session chose by the rule of Nov 23 and the
  glossary gives as proposed names (Nov 24): `TransparencyGroup.UsePatternBrushes`, `SymDef.AddDashResolutions`,
  `AreaSymDef.MakePatternTile`, `AreaSymDef.GetPatternBrushPlacement`, `PatternTile.GetBrushPlacement`,
  `PatternTile.IsTooPixelatedAt`, `PathShape.Rectangle`, `PathShape.FromRectangle` and
  `IDeferredDrawingTarget.DrawDeferred`.

The five terms that the session chose by their use in commit 06f4c824d are all decided on Nov 21.

### Open

A new open item gets a question by "How to make a question", and the glossary marks it "Open" in its entry.

1. **korostus:** how the highlights are expressed in the SVG DOM of the browser, and whether they stay there over the
   renderings: as a style of the drawn objects through their symbol keys, as a layer of their own for highlights that
   have no symbol, for example the control that `AddControlMode` makes, or as a direct drawing on a drawing surface in
   the DOM, for example a canvas (Nov 21). @Pauli leaves it for later (Nov 21), so a session does not take it up
   unless he asks.

### After the glossary

The specification is written again in the terms of the glossary and by the structure of the passes (Nov 21). @Pauli
went through the choices of the session on Nov 22, and the decisions of Nov 22 in Progress.md record them. He
approved the specification at 4c11a6ea on Nov 23, and the new tickets 023 to 026 of pass 1, by the structure of the
passes and in place of 005, 007 and 008, on the same day. The drafts 009 to 013 follow the specification when @Pauli
asks. The tickets 011 to 013 were
implemented while they were drafts, on branches rooted at the old branch of the epic. The done tickets take the
terms of the glossary, apart from 005, 007 and 008. The drafts, the epic and the older decisions in Progress.md
still use the earlier words. A draft is not implemented, and a session changes it only when @Pauli asks
([README.md](README.md)).

## Where things are

- **The main clone** `~/Documents/Projects/NotPurplePen` is on `docs/process-coordination`. The glossary and the other
  documents are there. Do not change its branch. Commit and push there.
- **The code of upstream:** the worktree `~/Documents/Projects/npp-worktrees/upstream` is detached at d48f4456, where
  `base` meets upstream (Nov 21). The names of upstream in the glossary come from it. `base` has the same code of `Map`,
  of the symbols, of the graphics targets and of the courses ([README.md](README.md), "Branches").
- **The code of the work:** the worktree `~/Documents/Projects/npp-worktrees/text-geometry` is at 2ad8c6f39. The names
  marked "work" in the glossary come from it. The new branch of the epic, `feature/palette-stack` at 8127117f, merges
  001, 003, 004 and the fixes 015 to 021, and has no pass 1 yet. The old branch `feature/palette-layers` keeps the old
  pass 1.
- **The transcripts** in `~/.claude/projects/-home-pauli-Documents-Projects-NotPurplePen/`: the session of Nov 20 and
  21 is `24af0e14-e17c-4f92-bc24-2c6815053a55.jsonl`. The session of Nov 21 is, in order,
  `9c11143a-a462-4aa8-afb8-c160fab19a1b.jsonl`, `64a7360d-8a53-46f1-9163-6772c93bfb4e.jsonl`,
  `57f43ccb-eaca-4dbf-892b-32e3b326895f.jsonl` and `1e9438c5-a2f3-408a-b614-8f0be26a97a8.jsonl`. The session of
  Nov 21 and 22 that wrote the specification again ran in a cloud environment of Claude, and its transcript is not
  in that folder.

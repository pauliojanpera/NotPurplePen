# Glossary of the palette-stack architecture

Nov 20, 2026. This glossary gives the names of the concepts that the specification of the palette-stack architecture
needs: the name in the code, the Finnish term and the English term. The content of many concepts is not decided or not
delimited yet. Each entry tells what the code, the specification and the decisions say now, and marks what is open.

The rules of the names:
- The names follow the concepts of upstream, unless there is a special reason to deviate.
- The name of a concept in the code is its primary name, also in Finnish text.
- The word *malli* (*model*) alone is too ambiguous to name a concept in the specification.
- When @Pauli uses a vague reference, such as *karttamalli*, the session asks him to make it exact.

The structure of the passes divides the concepts into three parts: the map side, the intermediate layer that pass 1
makes from the map, and the rendering side. Section 01 of [Specification.md](Specification.md) gives the structure. The
sections of this glossary follow the three parts.

Each entry starts with the primary name of its concept. Where the concept has a name in the code, the entry gives that
name first, and then the Finnish and the English term. Where it has none, the entry starts with the Finnish and the
English term. The tickets use the Finnish terms, and the other documents use the English terms. "Proposed name" marks
a name that is not decided.

A name in code font is a name in the code of upstream at d48f4456, where `base` meets upstream. A name marked "work" is
a name in the code of `feature/palette-layers` or of the branches of the tickets 011 to 013 (`feature/text-geometry` at
2ad8c6f39). "**Upstream:**" gives the name of upstream where it is different from the term of the entry.

The section "Words that are not names" lists the words that are not used as names.

## The map side · Kartan puoli

`Map` is the source. It keeps the structure in its ideal form.

### The map and its objects

- **`Map`** · kartta · *map* — It holds the colors (`SymColor`), the symbol definitions (`SymDef`), the symbols
  (`Symbol`), the templates (`TemplateInfo`), the scales, the print area, the georeferencing and the metric. It keeps
  the OCAD setup and the OCAD symbol header, to write them back (`OcadSetup`, `OcadSymbolHeader`). It also holds a state
  of the editing and of the drawing: an undo manager (`UndoMgr`), the changed areas, a cache of the bounds and the pens
  of the highlights. The map of the courses is a `Map` that `CourseLayout.RenderToMap` makes from the courses.
- **`SymDef`** · symbolimäärittely · *symbol definition* — The definition of a symbol, for example `PointSymDef`,
  `LineSymDef`, `AreaSymDef` or `TextSymDef`. The comments of upstream call it a *symdef*. The terms follow upstream:
  *symbolimäärittely* · *symbol definition* is `SymDef`.
- **`Symbol`** · symboli · *symbol* — One object of a map, for example `PointSymbol`, `LineSymbol`, `AreaSymbol` or
  `TextSymbol`. Its geometry is in paper coordinates. A `PointSymbol` and a `RectLikeSymbol` have a location, a
  `LineLikeSymbol` has a path (`SymPath`), and an `AreaLikeSymbol` has a path with holes (`SymPathWithHoles`). The
  comments of upstream also call it an object, for example a line object or an area object. The terms follow upstream:
  *symboli* · *symbol* is `Symbol`. Its identity is its symbol key (`Symbol.Key`). **Upstream:** a `Symbol` has no key
  or other identifier, but a `SymColor` has `Name` and `OcadId`, and a `SymDef` has `Name` and `SymbolId`.
- **`Symbol.Key`** · symbolin avain · *symbol key* — The identity of a `Symbol`. The maker of the `Symbol` gives it:
  - In a `Map` that is read from a file or edited, `Map` gives it from a counter whose values it does not use again.
    When an edit replaces a `Symbol`, the replacing `Symbol` gets the same symbol key. The undo restores it, and a
    self-supporting vector file carries it.
  - In the course map, `CourseLayout.RenderToMap` makes it from `EventDB`: from the course object key
    (`CourseObj.Key`) of the course object that made the `Symbol`, and from the index of the `Symbol` among the
    symbols of that course object. So a course map that is made again gives the same symbol keys to the same course
    object.

  Its type is `SymbolKey`: a value that does not change, with a text form. Two keys are the same when their text
  forms are the same, character by character. The text form of a key from the counter is the value of the counter in
  base 62, with the digits 0–9, A–Z and a–z in this order, for example `4C92`. A key is not the number of a symbol
  definition (`SymDef.SymbolId`).

  The symbol key is unique in its `Map`, so the reference of a drawn object also gives the `Map`. All the drawn objects
  of a `Symbol` share its symbol key, so it is not the `id` of an SVG element. It is not `SymDef.SymbolId`, which is
  the number of a symbol definition.
- **`GraphicsSymDef`, `GraphicsAreaSymbol`, `GraphicsLineSymbol`** · grafiikkaobjekti · *graphics object* — An object
  that the command "To Graphics" of OCAD made: a fill with its own color, or a line with its own color, width, caps and
  join. The OCAD import makes them (`OcadImport.CreateGraphicsObject`). `GraphicsSymDef` is one symbol definition for
  all the graphics objects of a map, and it has no values of its own. A `GraphicsAreaSymbol` fills its path with holes
  in its color (`FillColor`), and a `GraphicsLineSymbol` draws its path with its own pen in its color (`LineColor`).
  The comments of upstream call them graphics objects.
- **paperikoordinaatit** · *paper coordinates* — The coordinates of a map, in millimeters on the printed map. The points
  are `PointF` values. The OCAD import divides the OCAD units of 0.01 mm by 100 (`OcadImport.ToWorldDimensions`). The
  comments of upstream also say *map coordinates* and *world coordinates* (see "Words that are not names"). The
  real-world coordinates (`RealWorldCoords`) are a different concept: the georeferencing of the map.
- **ideaalimuoto** · *ideal form* — The form in which `Map` keeps a structure, apart from the form of a channel: the
  information that an encoding needs. `Map` keeps the curves of the paths as Bezier curves. The derived values are not
  ideal forms.

### Palette and color

- **`SymColor`** · palettiväri · *palette color* — A color of the normal layer (`SymLayer.Normal`). A palette color has
  a color or ink components, an opacity, a blend mode, a knockout and a priority. Upstream calls it a color, and the
  comments of upstream also call the colors layers.
- **palettipino, palettijärjestys, prioriteetti** · *palette stack, palette order, priority* — The transparency groups
  of the palette colors in palette order, and the order in which pass 2 composites them. The colors of a map are
  `Map.AllColors`, and upstream draws them from the lowest to the highest. One palette stack is for all the maps, the
  courses included.
- **väri C** · *color C* — A spatially constant color, independent of color space. A `SymColor` of upstream keeps a CMYK
  color (`CmykColor`).
- **`SymColor.Opacity`** (work) · peittävyys α · *opacity α* — A spatially constant opacity. The Finnish translations of
  Inkscape and of GIMP 3.0 call the opacity *peittävyys* and *peittokyky*. The coverage (*peitto*) is a different
  concept, which section 01 of the specification keeps apart from α.
- **`SymColor.BlendMode`** (work) · sekoitustila B · *blend mode B* — The compositing operation (`ColorBlendMode`). For
  an OCAD map, B = Darken is the larger share of each ink, and not the RGB minimum.
- **`Ink`** (work) · muste, prosessimuste, spottimuste · *ink, process ink, spot ink* — A colorant identity, apart from
  the palette color. It has a name (`Name`), a CMYK appearance (`Appearance`), and a screen frequency and angle where
  they are defined (`Screen`, `InkScreen`). The process inks C, M, Y and K always exist (`Ink.ProcessInks`). A spot ink
  can be a palette color by itself, at full tint (`SymColor.SpotInk`).
- **`InkComponent`** (work) · mustekomponentti, kerroin · *ink component, factor* — A palette color holds ink components
  (`SymColor.InkComponents`). Each has an ink and a factor from 0 to 1. A missing component and the factor 0 are
  different.
- **värierottelu** · *ink separation* — The coverage of one ink. A missing share leaves the separation as it is, 0 %
  erases it, and a positive share sets the coverage. *Värierottelu* is the established term of printing.
- **spottivärierottelu** · *spot color separation* — The name of OCAD for a spot ink in its files: a spot color with
  a name and a halftone screen, to which the colors refer by percentages. OCAD 6 to 8 keep them in the symbol header
  (`aColorSep`), and OCAD 9 and later in the strings of type 10 (`si_SpotColor`). The import makes each of them a
  spot ink (`Ink`), and the export writes the spot inks as them. The import and the export translate between the
  files of OCAD and `Map`, so where they describe the files of OCAD, they use the name of OCAD.
- **`SymColor.OverPrint`** · päällepainatus, päällepainatusväri · *overprint, overprint color* — The overprint flag of
  an OCAD process color keeps the inks below. A palette color that has the flag is an overprint color. The option
  `RenderOptions.blendOverprintedColors` draws the overprint colors with Darken, and the overprint mode
  (`OverprintMode`) tells how the drawing blends them. The inks simulate the overprinting in a rendering mode.
- **knockout** · *knockout* — The flag of Mapper. Its intent is the inverse of the overprint flag: it removes the inks
  below. The Mapper import sets `SymColor.OverPrint` only for a color that has a spot definition and whose knockout is
  off.

### Coverage prototypes

- **`CoveragePrototype`** · peittoprototyyppi · *coverage prototype* (work: `GeometryPrototype`) — The coverage G in
  its origin-bound form, before M places it. On the map side, G is a part of a `SymDef`: the full coverage, one hatching
  (an index in `AreaSymDef.hatchings`) or one area pattern in one palette color (an index in `AreaSymDef.patterns` and
  a `SymColor`). In an `AreaComboSymDef`, the component definition is part of G. An area pattern whose element has
  glyph parts in several palette colors has a coverage prototype in each of them. Upstream has no class for G. The
  shared shapes of a glyph are not coverage prototypes (*jaetut muodot*).
- **jaksollinen peitto G** · *periodic coverage G* — A binary, spatially repetitive, origin-bound selector: for each
  point of the plane, G tells if the point is in the coverage. In mathematics, G is the indicator function of a set of
  the plane that the translations of a lattice do not change. The hatchings and the regular area patterns are periodic
  coverages. An irregular area pattern is not: of its translations, only the identity is certain to be a symmetry. G
  selects a set of points, which is a coverage. In compositing, the alpha has two parts, the coverage and the opacity.
  The specification keeps them apart: the binary coverage (G and the clipping objects) and α (*peittävyys* ·
  *opacity*).
- **`CoveragePrototype.FullCoverage`** · täysi peitto, G = 1 · *full coverage* (work:
  `GeometryPrototype.FullCoverage`) — The whole plane: G(x, y) = 1 at each point. A constant function does not change
  in a translation, so it is periodic with each lattice. The full coverage is the trivial case of periodic coverage, and
  not a kind of its own. No M changes it. The G of points, lines, texts and area fills is the full coverage. The dash
  patterns and the glyphs along a line are strokes and placements, not lattices.
- **`HatchingPrototype`** · viivoituksen peittoprototyyppi · *coverage prototype of a hatching* — The G
  of one hatching (`HatchInfo`) of an `AreaSymDef`: stripes of the width `hatchWidth` about the lines y = k ·
  `hatchSpacing`. It refers to its `AreaSymDef` and to the index of the hatching, and copies nothing.
- **`AreaPatternPrototype`** · aluekuvion peittoprototyyppi · *coverage prototype of an area pattern* —
  The G of one area pattern (`PatternInfo`) of an `AreaSymDef` in one palette color: the parts of its element in the
  palette color at the points of its lattice. An element with parts in several palette colors gives an area pattern a
  coverage prototype in each of them. It refers to its `AreaSymDef`, to the index of the area pattern and to the palette
  color, and copies nothing.

### Glyphs

- **`PointSymDef`, `PointSymbol`** · pistesymbolin symbolimäärittely, pistesymboli · *point symbol definition, point
  symbol* — A point symbol definition draws its glyph at the location of each of its symbols (`Location`), turned by the
  angle of the symbol (`Rotation`), with the gaps of the symbol in the circles of the glyph (`Gaps`).
- **`Glyph`** · glyyfi · *glyph* — It defines a point symbol definition, or a part of a line or an area symbol
  definition. Its parts are lines, areas, circles and filled circles. The term of a part is *glyyfin osa* · *glyph part*
  (`Glyph.GlyphPart`).
- **`extraTransform`** · glyyfin lisämuunnos · *extra transformation of a glyph* — It transforms the points of a glyph
  but not its line widths, for example for the scale at corners and for the glyphs that decrease. So it is not an affine
  placement of a stroked shape.

### Lines

- **`LineSymDef`, `LineComboSymDef`** · viivan symbolimäärittely · *line symbol definition* — A `LineSymDef` has the
  dashes (`DashInfo`), the double line (`DoubleLineInfo`), the shortening and the pointy ends (`ShortenInfo`) and the
  glyphs (`GlyphInfo`). A `LineComboSymDef` gathers other line symbol definitions, its component definitions. The term
  is *yhdistelmän osamäärittely* · *component definition*. The comments of upstream call a `LineSymDef` a *linesymdef*.
- **`LineSymbol`** · viivasymboli · *line symbol, line object* — One line object of a map. Its path is
  `LineLikeSymbol.Path`, a `SymPath`. *Viiva* · *line* means a `LineSymbol` or its `LineSymDef`.
- **viivan mitta** · *measure of the line* — The metric that the source means to be used, which `Map` keeps in the ideal
  form (`Map.UseEuclideanMetric`). `Map` offers the means to convert it to suit a channel, and to keep the derived
  values. The measure of fix 021 (work: `OcadCurveMeasure`, `SymPath.GetOcadCurveMeasure`) gives a straight segment its
  length, and a quarter of a curve its chord.
- **viivan rakenne** · *structure of a line* — The base path, the parts as its pieces and offsets, the measure of the
  line, the fitted pattern of a part, the end dashes and the exceptions of fix 020. `Map` keeps the information that is
  necessary, that is the ideal forms: the base path with the flags of its points, the `LineSymDef` with its `DashInfo`,
  `DoubleLineInfo`, `ShortenInfo` and `GlyphInfo`, and the metric. The parts of the line, the segments, the fitted dash
  patterns, the merged end dashes and the exceptions of fix 020 are derived values, which `Map` can keep for the
  channels where the implementation finds it sensible.
- **peruspolku** · *base path* — The path that the parts of a line are pieces and offsets of: the `SymPath` along which
  a `LineSymDef` is drawn. For a line object, it is `LineLikeSymbol.Path`. For the border of an area, it is the main
  path or a hole of the `AreaLikeSymbol`. `Map` keeps it in the ideal form. The borders of a double line are offsets of
  it, and their dash patterns are measured along it. In the theory of offset curves, it is the base curve.
- **`DoubleLineInfo`** · kaksoisviiva, kaksoisviivan reuna, kaksoisviivan täyttö · *double line, double line border,
  double line fill* — A double line has a fill and two borders, the left and the right border. Upstream calls the
  borders the left and the right double line, with their colors, widths and dashes (`doubleLeftColor`,
  `doubleRightColor`), and calls the band between them the fill (`doubleFillColor`). That name would make *kaksoisviiva*
  mean both the whole line and each of its sides. `doubleThick` is the distance between the double lines and the
  thickness of the fill.
- **`DashInfo`** · katko, väli, toissijainen väli · *dash, gap, secondary gap* — The dashes of a `LineSymDef`: the
  lengths of the dashes and of the gaps (`dashLength`, `gapLength`), and the secondary gaps (`secondaryMiddleGaps`,
  `secondaryEndGaps`). Upstream calls the dashes between the first and the last dash the main dashes, and has the
  lengths of the first and the last dash (`firstDashLength`, `lastDashLength`).
- **`ShortenInfo`** · lyhennys · *shortening* — The shortening of the beginning and of the end of a line
  (`shortenBeginning`, `shortenEnd`). It is measured with the metric of the map. `ShortenInfo` also has the pointy ends
  (`pointyEnds`).
- **`SpacingMethod.OCAD`** · OCADin välistys · *OCAD spacing* — The method that fits the dashes and the glyphs of a
  segment as OCAD does. The other spacing methods of upstream are `OpenMapperDashes` and `OpenMapperMidSymbols`.
- **`SymPath.DistanceMetric`** · metriikka · *metric* — The OCAD metric is the larger coordinate difference plus half
  the smaller one (`SymPath.BizzarroDistance`): a norm whose unit circle is an octagon. A map uses the OCAD metric when
  its view parameter `p` is 1. Mapper maps use the euclidean metric. The metric of a map is `Map.MapDistanceMetric`, and
  `Map.UseEuclideanMetric` selects it. The OCAD metric is best effort.
- **`PointKind.Corner`, `PointKind.Dash`** · kulmapiste, katkopiste · *corner point, dash point* — The points of a path
  that have the OCAD flags y & 1 and y & 8. They divide a line into segments.
- **`SymPath.GetSubpaths`** · osapolku · *subpath* — A part of a path that the start and stop flags of its points
  delimit, for example for a border of a double line (`SymPath.DOUBLE_LEFT_STARTSTOPFLAG`).
- **Bezier-käyrä** · *Bezier curve* (`PointKind.BezierControl`) — A curve of a path, with its control points.

### Areas

- **`AreaSymDef`, `AreaComboSymDef`** · alueen symbolimäärittely · *area symbol definition* — An `AreaSymDef` has the
  area fill (`FillColor`), the area border (`BorderSymdef`), the hatchings (`HatchInfo`) and the area patterns
  (`PatternInfo`). An `AreaComboSymDef` gathers other symbol definitions, its component definitions (*yhdistelmän
  osamäärittely*). The Mapper import makes a `LineComboSymDef` when all the parts of a combined symbol are
  `LineSymDef`s, and an `AreaComboSymDef` otherwise. So a component definition of an `AreaComboSymDef` can be a
  `LineSymDef`, a `LineComboSymDef`, an `AreaSymDef` or another `AreaComboSymDef`.
- **`AreaSymbol`** · aluesymboli · *area symbol, area object* — One area object of a map. Its path with holes is
  `AreaLikeSymbol.Path`, a `SymPathWithHoles`.
- **`SymPathWithHoles.MainPath`, `SymPathWithHoles.Holes`** · pääpolku, reikä · *main path, hole* — The path with holes
  of an `AreaLikeSymbol` is a main path and its holes, each a `SymPath`. The map keeps each hole as a symbol of its own
  (`HoleSymbol`), whose definition (`HoleSymDef`) has no color, and `AreaLikeSymbol.Path` gathers the main path and
  the paths of the holes. `SymPathWithHoles.Fill` fills them together with the alternate fill rule
  (`AreaFillMode.Alternate`), so the area fill leaves out the holes. `AreaSymDef.Draw` draws the area border along the
  main path and along each hole, so each of them is a base path of the area border.

### Patterns

- **`AreaSymDef.FillColor`** · alueen täyttö · *area fill* — The solid fill of an area, in one color.
- **`AreaSymDef.BorderSymdef`** · alueen reuna · *area border* — The line symbol definition with which an area is
  bordered. `AreaSymDef.Draw` draws it along the main path and along each hole.
- **kuviotäyttö** · *pattern fill, structured fill* (`HatchInfo`, `PatternInfo`) — The hatchings and the area patterns.
- **`HatchInfo`** · viivoitus · *hatching* — It has a color, a width, a spacing, an offset, an angle and a rotate mode
  (`hatchColor`, `hatchWidth`, `hatchSpacing`, `hatchOffset`, `hatchAngle`, `hatchRotateMode`). Upstream calls its lines
  hatch lines. A hatching is a coverage prototype of its own: stripes, whose symmetries include all the translations
  along the lines. A pattern of line elements would have only the steps of its element along the lines, so it would
  separate configurations that give the same coverage. Upstream draws the hatch lines as continuous lines
  (`DrawHatchLines`). A cross hatching is two hatchings, so it has two prototypes.
- **`PatternInfo`** · aluekuvio · *area pattern* — It has a pattern element (`patternGlyph`), the size of the pattern
  element (`patternWidth`, `patternHeight`), an offset of the alternate rows by 1/2 (`offsetRows`), an angle, an offset,
  a rotate mode, a fill mode and the irregularity. Upstream calls it a pattern.
- **hila, hilavektori** · *lattice, lattice vector* — The arrangement in which a pattern repeats its elements.
  **Upstream:** no name. The size of the pattern element and the offset of the rows give the lattice.
- **`PatternInfo.patternGlyph`** · elementti · *element* — The geometry that the lattice repeats. The center of an
  element is its origin, the point that the lattice places at a lattice vector. Upstream calls it the pattern element,
  and the structure element in the comments of the fill modes.
- **hilan affiininen muunnos L, elementin affiininen muunnos E** · *affine lattice transformation L, affine element
  transformation E* — The point e of the element at the lattice vector v is at L(v) + E(e).
- **`PatternInfo.patternAngle`, `HatchInfo.hatchAngle`, `AreaSymbol.Angle`** · rakennekulma, alueobjektin kulma ·
  *structure angle, angle of the area object* — In OCAD, the structure angle of the symbol turns the lattice alone, and
  the angle of an area object turns the lattice and its elements together. The structure angle is `patternAngle` or
  `hatchAngle`, and the angle of an area object is `AreaSymbol.Angle`.
- **`AreaSymbol.RotationCenter`** · kiertokeskus · *rotation center* — The point about which the angle of an area object
  turns its hatchings and patterns. It is also their origin: the transformation of a hatching is T(rotation center) ·
  R(angle of the area object) · R(structure angle). **Upstream:** the OCAD import sets it to the origin of the map, and
  the Mapper import takes it from the pattern origin of the object (`rotationCoord`).
- **`AreaSymDef.RotateMode`** · kiertotila · *rotate mode* (`Always`, `ManualOnly`, `Never`) — The comments of upstream:
  `Always` turns the pattern when the map or the object turns. `ManualOnly` allows only a manual setting of the
  direction of the pattern, and does not turn the pattern when the object or the map turns. `Never` lets no direction
  change the drawing of the pattern. In the specification, `ManualOnly` is the same as `Always`: M takes the angle and
  the rotation center of the `AreaSymbol`, and only `Never` leaves them out, as the drawing of upstream does. The
  difference is in the editing: when the object or the map turns, `Always` turns the angle with it, and `ManualOnly`
  keeps it. The OCAD import gives `ManualOnly` to a pattern that does not turn, and the Mapper import gives `Never`.
- **`AreaSymDef.PatternFillMode`** · täyttötila · *fill mode* (`Clip`, `CompletelyInside`, `CenterInside`,
  `PartiallyInside`) — It clips the elements, or selects each glyph part of the elements by its own points and center:
  the glyph parts that are entirely inside, centered inside or partially inside the area. The area does not clip a
  selected glyph part. Upstream (`GlyphPartFilter`) and Mapper select each glyph part separately, and not the complete
  element. **Upstream:** `Clip` was the only mode until OCAD 12.
- **`PatternInfo.irregular`** · epäsäännöllinen kuvio · *irregular pattern* (`irregularVarX`, `irregularVarY`,
  `irregularMinDist`) — Its elements move a random but deterministic distance. The seed comes from the rounded position
  of the element. The randomness is kept as a definition.

### Texts

- **`TextSymDef`** · tekstin symbolimäärittely · *text symbol definition* — The definition of the texts and of the
  texts along a line: the font (`FontName`, `FontEmHeight`, `TextEffects`) and its color (`FontColor`), the spacings of
  the lines, the paragraphs, the characters and the words, the indents, the tabs, the alignments, the framing
  (`FramingInfo`), the underline (`Underline`) and the center point (`CenterPointSymdef`). A `TextSymbol` and a
  `LineTextSymbol` both have a `TextSymDef`.
- **`TextSymbol`** · tekstisymboli · *text symbol, text object* — One text object of a map: its lines of text, its
  location (`Location`), its angle (`Rotation`), its width (`Width`) and its alignments. The location is the origin of
  the text, and the text turns about it by the angle. The drawing breaks the text into lines (`WrappedText`), and lays
  out each line from the location, with the Y axis down.
- **muotoiltu teksti** · *formatted text* (`TextSymbol.Width`) — A text whose width is more than 0. The drawing breaks
  its lines at the width, and justifies them when the alignment is `Justified`. A text of the width 0 is unformatted:
  its lines break only where the text has a new line.
- **`LineTextSymbol`** · viivan varren teksti · *text along a line* — One text of a map that the drawing draws along
  its path (`LineLikeSymbol.Path`), with a `TextSymDef`. Each grapheme is at its own place along the path, and turns
  with the direction of the path there. The drawing measures the path with the OCAD metric in all the maps, as current
  OCAD does. A text along a line has no underline, no center point and no framing rectangle, as the drawing does not
  draw them along a line.
- **`TextSymDef.CharSpacing`, `TextSymDef.WordSpacing`** · merkkiväli, sanaväli · *character spacing, word spacing* —
  The space that the drawing adds after each grapheme, and the width of a space between the words, each as a factor of
  the width of a space of the font. Where the character spacing is more than 0, each grapheme is a text run of its own.
- **grafeemi** · *grapheme* — A character as a reader sees it: one or more Unicode code points that are one text
  element (`StringInfo.GetNextTextElement`).
- **`TextSymDef.Framing`, `TextSymDef.FramingStyle`** · tekstin kehys · *framing of a text* — An effect in a color of
  its own (`framingColor`) that sets a text apart from the map. Its styles are the framing line (`FramingStyle.Line`),
  the shadow (`FramingStyle.Shadow`) and the framing rectangle (`FramingStyle.Rectangle`).
- **`FramingStyle.Line`** · kehysviiva · *framing line* — The framing of a text symbol definition in the style
  `FramingStyle.Line`. The other styles are a shadow and a rectangle. The framing of a text run is one line: a stroke of
  the full width with the joins of the symbol.
- **`FramingStyle.Shadow`** · varjo · *shadow* — The framing in which each text run has a copy in the color of the
  framing, moved by `shadowX` and `shadowY`.
- **`FramingStyle.Rectangle`** · kehyssuorakulmio · *framing rectangle* — The framing that fills a rectangle around
  the lines of a text in the color of the framing, with the margins `rectBorderLeft`, `rectBorderTop`, `rectBorderRight`
  and `rectBorderBottom`.
- **`TextSymDef.Underlining`** · alleviivaus · *underline* — A line under the last line of each paragraph of a text, in
  its own color, width and distance (`underlineColor`, `underlineWidth`, `underlineDistance`). The underline effect of
  the font (`TextEffects.Underline`) is a line under each line of the text, in the color of the font. The drawing draws
  both as lines, and the font of the text has no underline effect.
- **`TextSymDef.CenterPointSymdef`** · tekstin keskipiste · *center point of a text* — A point symbol definition that
  the drawing draws at the center of an unformatted text, as OCAD does: at the middle of the box of the lines of the
  text, as OCAD measures the box.

### Rectangles

- **`RectangleSymDef`, `RectangleSymbol`** · suorakulmion symbolimäärittely, suorakulmiosymboli · *rectangle symbol
  definition, rectangle symbol* — A rectangle of a map: its location, its size and its rotation (`Location`, `Size`,
  `Rotation`). Its definition gives the color and the width of its line (`LineColor`, `LineThickness`), the radius of
  its corners (`CornerRadius`) and its grid. It is the rectangle symbol of OCAD. The maps of the tests have, for
  example, the boxes of the punches as rectangles.
- **`RectangleSymDef.GridFlags`** · ruudukko, ruudukon viivat, ruudukon tekstit · *grid, grid lines, grid texts* — The
  grid divides a rectangle into cells of about `CellWidth` by `CellHeight`. Its lines are 0.15 mm wide, in the color of
  the rectangle. Its texts number the cells from the top or from the bottom, in Arial bold of the size `TextSize`, and
  the last cells can have a text of their own (`UnnumberedCells`).

### Beside the palette stack

- **`TemplateInfo`** · pohjakartta · *template* (`LoadedTemplate`) — It is not a palette color. `Map.Draw` draws the
  templates under and over the palette colors now. An OCAD template is a `Map` of its own.
- **`SymLayer.Image`** · kuvataso · *image layer* — The layer below all the normal layers. The layers of `SymLayer` are
  layers in the sense of image editing, and their names are *kuvataso*, *tavallinen taso* and *asettelutaso*. Its
  objects are image objects, with a special symbol definition (`ImageSymDef`). It is not a palette color. Upstream draws
  the image layer and the layout layer with the same loop as the colors (`DrawColor`), through two pseudo-colors without
  a color value (`Map.ImageColor`, `Map.LayoutColor`). They are not palette colors.
- **`SymLayer.Layout`** · asettelutaso · *layout layer* — The layer above all the normal layers. Its objects are layout
  objects, with a special symbol definition (`ImageSymDef`). It is not a palette color.
- **`SymLayer.Normal`** · tavallinen taso · *normal layer* — The layer between the image layer and the layout layer. It
  holds the transparency groups of the palette colors in palette order, as a group of layers holds layers in image
  editing.
- **ratakartta** · *course map* (`CourseLayout.RenderToMap`) — The map of the courses is a `Map` of its own, so the same
  pass 1 applies to it. One palette stack is for all the maps. Upstream makes it again when its `CourseLayout` changes
  (`MapDisplay.SetCourse`). The symbol keys of its symbols come from `EventDB` (`Symbol.Key`).
- **`CourseLayout`** · ratojen asettelu · *course layout* — The course objects of one drawing of courses, each in its
  `CourseLayer`, and the colors of the layers. `CourseFormatter` and `TopologyFormatter` add the course objects to it,
  and `CourseLayout.RenderToMap` makes the course map from it. One course layout can hold several courses in
  different layers, for example the main course and the view of all the controls.
- **`CourseAppearance`** · ratojen ulkoasu · *course appearance* — The settings of an event that tell how the courses
  are drawn: for example the sizes of the course objects, the purple, the descriptions and the overprint mode
  (`OverprintMode`). The course file keeps them, and `CourseAppearanceDialog` edits them.
- **`CourseObj`** · rataobjekti · *course object* — One object of the courses on the course map, for example a control
  circle, a leg or a description. `CourseFormatter` and `TopologyFormatter` make the course objects of a
  `CourseLayout` from the data of `EventDB`, and `CourseObj.AddToMap` makes their symbols in the course map. One course
  object can make many symbols. Its fields `controlId`, `courseControlId`, `courseControlId2` and `specialId` are
  associations that the selection uses, and not its identity.
- **`CourseObj.Key`** · rataobjektin avain · *course object key* — The identity of a course object. `CourseFormatter`
  or `TopologyFormatter` gives it at the place where it makes the course object. The key has three parts: the role of
  the place in the data of `EventDB`, for example the whole leg, the beginning part of a leg or the map issue point of
  a leg; the identifiers of `EventDB` that the place uses; and the `CourseLayer`. Each place has a role of its own, so
  two places do not give the same key. The role tells what the place makes of the data of `EventDB`, so two places of
  the code that make the same thing give the same role: a control label that the user placed and one that the
  formatter placed are both `Label` (@Pauli, Nov 23). The class of the `CourseObj` is not a part of the key: when a
  whole leg becomes flagged, its key stays the same. Its type is `CourseObjKey`, and the roles are the members of
  `CourseObjRole`. Its text form is `<role>.<identifier>….<layer>`, with the numbers in base 62 and the name of the
  role with a lowercase first letter, for example `leg.4.5.0`.
- **`Map.PrefixSymbolKeys`** · avainjakso · *period of a key prefix* — A period in which `Map.AddSymbol` gives each
  symbol without a symbol key, and each hole that the map gets with it, the key `<prefix>/<index>`, in place of a key
  from the counter. The index is in base 62, from 0, in the order in which the map gets the symbols, and it skips an
  index whose key a symbol of the map has. `CourseLayout.RenderToMap` opens a period with the text form of the course
  object key for each `CourseObj.AddToMap`, so the symbols of a course map get the keys `<course object key>/<index>`,
  for example `leg.4.5.0/0`.

## The intermediate layer · Välikerros

`Map` makes the intermediate layer from the map alone, in pass 1. It is not source data: `Map` stays the source.

### The separations and the cutters

- **vaihe 1, ilmentäminen** · *pass 1, layer instantiation* (work: `Map.CreateLayerInstances`) — `Map` makes the palette
  separations and their palette cutters from the map alone, and no rectangle or resolution of a drawing. The palette
  color coverages of the symbols get their derived values from the geometry methods of the symbol definitions. *Vaihe*
  means a pass.
- **`SymbolCoverage`** · symbolin palettiväripeitto · *palette color coverage of a symbol* — The coverage of one
  `Symbol` in one palette color with one coverage prototype G: the triple (`Symbol`, `SymColor`, G). Its coverage is G
  under M inside the union of its clipping objects. A symbol has one for each G in a palette color: usually one, and two
  for a cross hatching, which upstream imports as two hatchings of one color (`OcadImport`). It refers to these objects
  and copies nothing. It keeps its own derived values. The triple is a coverage, and not a separation: a separation is
  named by what it separates by, as *värierottelu* and *palettierottelu* are.
- **`PaletteSeparation`** · palettierottelu · *palette separation* — The palette color coverages of the symbols in one
  palette color. It keeps its palette cutters as a derived value.
- **`PaletteCutter`** · palettileikkuri · *palette cutter* — The palette color coverages of the symbols in one palette
  separation that have the same configuration. It cuts the uniform color of the palette color to the coverage that G
  under M gives inside the union of its clipping objects. Each palette color coverage of a symbol is in one palette
  cutter, and each palette cutter is in one palette separation.
- **`AreaCluster`** (proposed name) · alueryväs · *area cluster* — The palette color coverages of one palette cutter of
  a hatching or a clipped area pattern whose areas are connected: the bounding boxes of their areas overlap or touch,
  directly or through other areas of the cluster. An area whose contours meet (`SymPathWithHoles.ContoursMeet`), or
  that meets no other area of the palette cutter, is a cluster of its own. A palette cutter keeps its area clusters as
  a derived value (`PaletteCutter.AreaClusters`, proposed name). The pattern of an area cluster is drawn once, in the
  union of its areas, and the expansion of the coverage prototype goes over the bounding box of the cluster
  (`SymbolCoverage.ExpandPrototype(RectangleF)`, proposed name). The term avoids "group", which is the transparency
  group of a palette color. Ticket 028.
- **`AreaCluster.Union`** (proposed name) · ryppään unioni · *union of an area cluster* — A clipping object: one
  filled path with the nonzero rule (`AreaFillMode.Winding`) that holds all the contours of all the areas of the
  cluster. A contour winds positively when an even number of the other contours of its area enclose it, and the other
  way when an odd number do (`SymPathWithHoles.AddToUnionPathPartList`), so the nonzero rule fills of each area what
  the even-odd rule fills of it alone, and no area cancels another. It is a derived value of the intermediate layer,
  the same for all channels: each channel clips to it and fills it as any filled path. `fix/overlapping-patterns`
  makes the same path in the old drawing (`OverlappingAreas.Group.GetUnionPathKey`).
- **kokoonpano, samanlainen kokoonpano, normaalimuoto, perusalue, siirtosymmetria** · *configuration, equivalent
  configuration, normal form, fundamental domain, translation symmetry* — The palette color, G and the effective affine
  transformation M of a palette color coverage of a symbol. M comes from the `Symbol` and from G. The palette color
  coverages of the symbols with the same configuration are one palette cutter. Two configurations are the same when
  they give the same coverage: the palette color and the coverage prototype are the same, and M₁⁻¹M₂ is a symmetry of
  G, an affine map S for which G∘S = G. The translation symmetries of G are its symmetries that are translations. The
  symmetries of a regular area pattern include the translations of its lattice. Of the translations, only the identity
  is certain to be a symmetry of an irregular area pattern. The symmetries of a hatching include all the
  translations along its lines, and the translations across them by multiples of the spacing. All the maps are
  symmetries of the full coverage. A normal form decides the sameness
  (`CoveragePrototype.GetNormalForm`): the translation of M is reduced into a fundamental domain of the translation
  symmetries of G, and it is rounded to a fixed precision. A fundamental domain holds exactly one translation of each
  class of translations that differ by a translation symmetry of G. The configurations with the same normal form are
  the same, and the relation is an equivalence.
- **`SymbolCoverage.Transformation`**, **`PaletteCutter.Transformation`** · affiininen muunnos M ·
  *affine transformation M* (work: `LayerInstance.Transformation`) — An invertible 2D affine transformation of G. A
  palette color coverage of a symbol has the M of its configuration, and a palette cutter has the normal form of it.
- **peitto, unioni** · *coverage, union* — A binary coverage is a set of points of the plane. In mathematics, it is the
  indicator function of the set, which is 1 at the points of the set. G is a coverage, and so is the area that a
  clipping object covers. The coverage of a palette cutter is G under M inside the union of its clipping objects. The
  coverage of a palette separation is the union of the coverages of its palette cutters. The union can stay implicit in
  the rendering, but the palette color coverages of the symbols cannot be left out. The union of the clipping objects
  of a palette cutter is a true union, also where each area fills with the even-odd rule.

### The derived values

- **johdos** · *derived value* — A result that `Map` gives from the ideal form for the needs of the channels. `Map`
  offers the means to make derived values and to keep them, where the shared use of the channels or a cache of the
  results makes it sensible. The means are the known channel conversions of `Map`, and an encoding chooses the channel
  conversions that its channel can use. A palette color coverage of a symbol keeps its own derived values. A derived
  value that depends on a definition alone belongs to the definition, for example the shared shapes of a glyph. A
  palette separation keeps its palette cutters. A derived value is calculated when it is necessary the first time, and
  it becomes invalid when the map changes. Upstream keeps derived values in the same way, for example
  `SymPath.flatpoints`, `TextSymbol.wrappedText` and `Symbol.boundingBox`. The term comes from reactive programming.
  There, `Map` is the state, and a derived value is calculated from it: for example *derived* of Svelte and Solid, and
  *derived state* of React. A computed value is a derived value that is kept and calculated again when a dependency
  changes, for example `Signal.Computed` of the signal proposal of TC39 and `computed` of Vue and Angular. A derived
  value that `Map` keeps acts as a computed value. A derived value is not an ideal form, but a practical adaptation of
  the ideal form to the needs of the channels. Its form depends on what the implementations that the channels choose
  need: for example, the dashes on a curve can be Bezier pieces, polylines or both. In the work, the shapes keep
  ellipses and arcs as ellipses and arcs, as the graphics targets make polylines of them each in its own way.
- **viivan osa** · *part of a line* — One line component on one piece of the base path, with its offset: the triple
  (line component, piece, offset). Its fitted pattern is the fitted patterns of its segments, which the fitting
  calculates together. It is a derived value of the palette color coverage of the symbol, and the same for all the
  channels.
- **segmentti** · *segment* — The section of a piece of the base path between corner and dash points. The fitting gives
  a dash pattern to each segment, but it calculates the segments of a part of a line together: the minimum gaps apply
  to the longest segment only, and the decreasing glyphs to the first and the last segment. A segment is not a part of
  a line. Upstream calls it a segment.
- **viivan osatekijä** · *line component* — One of the components that a `LineSymDef` draws from the base path: the
  main line, the secondary line, the double line fill, a double line border, the pointy ends or the glyphs along the
  line.
- **pala** · *piece* — An interval of the base path: its start and its end, as positions along the path (*kohta*) in the
  measure of the line. The start and stop flags of the points and the shortening give the pieces of a line component.
- **siirtymä** · *offset* — The distance of a line component from the base path, to the side.
- **siirretty polku** · *offset path* — The shape of a border of a double line, in its ideal form: the base path and the
  offset. Its dash pattern is measured along the base path. The rule of the corners of the offset belongs to the shape,
  and the join belongs to the stroke. The offset curve is a derived value.
- **neljännes, jänne** · *quarter, chord* — Each quarter of a Bezier curve has its chord as its length in the measure.
  Within the quarter, the dashes are in proportion to the true arc.
- **litistetty polku, polun litistäminen** · *flattened path, path flattening* (`SymPath.GetFlattenedPoints`) — The
  polyline that approximates the curves of a path. Upstream calls its points the flat points and keeps them in the
  `SymPath` (`flatpoints`). The ideal form keeps the curves. A derived value can keep a polyline, and an encoding can
  make one.
- **taitepiste, jana** · *vertex, edge* — A point of the flattened path, and the straight piece between two adjacent
  points of the flattened path. The English term of *jana* is *edge*, as *segment* is the segmentti. The points of the
  path itself have the kinds `Normal`, `BezierControl`, `Corner` and `Dash` (`PointKind`). The flat points also have
  interpolated points (`PointKind.Interpolated`).
- **sovitettu katkokuvio** · *fitted dash pattern* (`ComputeDashDistances`) — The dash and the gap that the fitting
  gives to each segment of a part of a line. The fitting calculates the segments of a part together. It belongs to the
  structure of a line.
- **päätykatko, yhdistetty katko** · *end dash, merged dash* — The end dashes of adjacent segments merge into one dash
  at a corner point or a dash point. Upstream has the lengths of the first and the last dash (`DashInfo`).
- **korjauksen 020 poikkeus** · *exception of fix 020* — A dash from which the rule of fix 020 leaves out the vertex.
  Part of the structure of a line.
- **katkojuoste** · *run of dashes* — The middle dashes of a segment as one periodic dashed line. The runs in the
  euclidean measure are the same for Skia, PDF and SVG, as their dash patterns measure the path euclidean. They are
  derived values: `Map` offers the means to make them and to keep them.
- **koostettu katkoviiva** · *composed dashed line* — The form of a line for a channel whose dash pattern measures the
  euclidean length: the runs and the single dashes.
- **kanavamuunnos** · *channel conversion* — A conversion of the ideal form into a form that a channel can use. `Map`
  offers the known channel conversions, and keeps their results as derived values where the shared use of the channels
  or a cache makes it sensible. `Map` offers all the geometric conversions, and their results are forms of `Map`:
  shapes, strokes, placements and polylines. For example, the conversion of the arcs into Bezier pieces is a channel
  conversion of `Map`. An encoding chooses the channel conversions of `Map` that its channel can use. Its own
  conversions are only the conversions into the vocabulary of its channel, for example SVG elements and attributes,
  PDF operators and resources, or Skia objects, and the adaptations to the deviations of its channel that no general
  conversion covers. It does not keep their results. In the legalization of a compiler, the channel conversions of
  `Map` are the general expansions (LLVM: *Expand*), the choice of an encoding is the table of what is legal, and the
  own conversions of an encoding are the custom lowering (*Custom*) and the emission.
- **katkotaulukko** · *dash table* — A known channel conversion of the dashes of a line. `Map` offers it.
- **laajennus** · *expansion* (work: `StrokedShape.ExpandDashes`, `RepeatedShapes.Expand`) — A known channel conversion
  into single parts: a run into dashes, and a repeated placement into single placements. `Map` offers it.
- **leikkausobjektit piirron resoluutiolla** · *clipping objects at the resolution of a drawing*
  (`SymbolCoverage.GetClippingObjects(float)`) — A channel conversion: the clipping objects of a palette color coverage
  of a symbol for a drawing of a resolution (*piirron resoluutio*). A dashed line component whose gaps are all shorter
  than the resolution is solid, as the old drawing draws it (`LineSymDef.DrawDashedWithOffset`). At the resolution 0,
  they are the clipping objects of the coverage (`SymbolCoverage.ClippingObjects`). Pass 1 takes no resolution, so
  `Map` offers this conversion, and an encoding chooses it. The symbol definitions give the resolutions above which
  their dashed line components are solid (`SymDef.AddDashResolutions`, proposed name), and the coverage keeps the
  result for each number of these resolutions that are below the resolution of the drawing.
- **kuviotäytön laajennus** · *expansion of a pattern fill* (`SymbolCoverage.ExpandPrototype`) — The
  lines of a hatching or the elements of an area pattern over the bounds of an area, as single clipping objects at the
  places and with the steps of the drawing. The expansion of an irregular area pattern is the placements of its
  elements. It is a known channel conversion, which `Map` offers.

### The forms of the derived values

- **`ClippingObject`** (work) · leikkausobjekti · *clipping object* — It limits the extent over which G of its palette
  cutter applies. It gives no color or alpha. It has a shape that is drawn (`Shape`), a fill or a stroke, and a
  placement (`Placement`). The clipping objects of a palette color coverage of a symbol are its derived values.
- **`ClippingObject.Shape`** (work) · piirtyvä muoto · *shape that is drawn* — The shape of a clipping object. For a
  filled shape, the shape is the outline of the area. For a stroked shape, the shape is the path at the middle of its
  stroke shape (*kynänjälki*).
- **`Shape`** (work) · muoto · *shape* — A path (`PathShape`), an ellipse (`EllipseShape`), an arc (`ArcShape`) or a
  text run (`TextShape`). The border of a double line is an offset path. A path that the old drawing fills as a
  rectangle, the framing rectangle of a text, keeps the rectangle (`PathShape.Rectangle`, `PathShape.FromRectangle`,
  proposed names), so a channel can write it as a rectangle.
- **`FilledShape`** (work) · täytetty muoto, täyttösääntö · *filled shape, fill rule* — Its fill rule is
  `FilledShape.FillRule` (`AreaFillMode`).
- **viivamuoto** · *stroked shape* (work: `StrokedShape`) — A shape that a pen (*kynä*) draws along its path. In the
  painting models of PostScript, PDF and SVG, a path is painted with a fill, which paints its inside by a fill rule (the
  operator `f` of PDF), or with a stroke, which paints the band that a pen draws along the path (the operator `S`). A
  filled shape (*täytetty muoto*) is the one, and a stroked shape is the other.
- **kynänjälki** · *stroke shape* — The band that the pen of a stroked shape covers along its path: its coverage. SVG 2
  calls it the stroke shape. PostScript makes its outline with `strokepath`, and Skia with `FillPathWithPaint`. With
  round caps and joins, it is the Minkowski sum of the path and a disk as wide as the pen, that is the area that the
  disk sweeps. METAFONT and MetaPost call its boundary the envelope.
- **`Stroke`** (work) · kynä · *stroke* — The width (`Width`), the caps (`Caps`), the join (`Join`) and the miter limit
  (`MiterLimit`) of a stroked shape, and the dash pattern and its phase (`DashPattern`, `DashPhase`). The Finnish term
  is *kynä*, as upstream calls the same values a pen (`IGraphicsTarget.CreatePen`). A pen of upstream also has a color
  or a brush. A `Stroke` has no color, as the palette color gives it, and it has the dash pattern and its phase. The
  band that a pen covers along a path is its stroke shape (*kynänjälki*). The symbol definitions give the caps and the
  joins together as a line style (`LineStyle`: `Rounded`, `Mitered`, `Beveled`, `FlatRounded`). The dash structure in
  the measure of the line is the fitted pattern of a part of a line, and not a part of `Stroke`.
- **`LineCapMode`, `LineJoinMode`, `Stroke.MiterLimit`** · pää, liitos, jiirausraja · *cap, join, miter limit* — The
  form of the ends of a stroke, the form of its corners, and the limit beyond which a mitered corner is beveled.
- **`Stroke.DashPattern`, `Stroke.DashPhase`** (work) · katkokuvio, katkokuvion alkukohta · *dash pattern, dash phase* —
  The lengths of the dashes and the gaps in turn, and the distance that the pattern has gone at the start of the shape,
  as `stroke-dasharray` and `stroke-dashoffset` of SVG. They are properties of `Stroke`. They are in the measure of the
  channels: euclidean, along the own path of the stroked shape, as the dash pattern of PDF and the dash effect of Skia
  measure. They belong to the runs of dashes, which are derived values.
- **`TextShape`** (work) · tekstijuoste · *text run* — A word, or a grapheme where the character spacing is more than 0,
  at its own position. It has the text, the font family, the size, the effects and the top left corner. A text run is a
  shape. Its top left corner is in the coordinates of its placement, whose Y axis points up, and its glyphs lie below
  the corner, along the X axis. The mirror of the Y axis with which the drawing lays out a text is a part of the text
  run, so the innermost step of the placement of a text is the pose of the text.
- **jaetut muodot** · *shared shapes* — Each part of a glyph is one shape, which each placement of the glyph refers to.
  They depend on the definition alone, so they belong to the definition. Stretched and diminishing glyphs get shapes of
  their own. They are not coverage prototypes: they are the shapes of clipping objects, and placements place them. A
  glyph belongs to G when it is the element of an area pattern (`PatternInfo.patternGlyph`). **Upstream:** a glyph at a
  corner is scaled, unless `noScaleAtCorners` is set, and the glyphs of `GlyphLocation.SpacedDecrease` decrease in size
  and spacing.
- **`Placement`** (work) · sijoitus · *placement* — It gives one or more poses. Each reader asks a placement for its
  poses. It composes its steps into one matrix (`Matrix`).
- **yksittäinen sijoitus** · *single placement* — It keeps one pose: one point and an angle.
- **toistosijoitus** · *repeated placement* — A placement that keeps its path, its first position along the path, its
  step and its count. It calculates its poses from the path: the point of the path at each position along the path, and
  the direction of the path there, with the method of the drawing. Only an encoding that gives a repeated placement to
  an abstraction of its channel reads the path and the step. On a Bezier curve, the glyphs of the OCAD spacing repeat by
  quarters. **Upstream:** the glyphs that `GlyphLocation.Spaced` places along a line at the distances given.
- **asema** · *pose* — A point and an angle (x, y, θ). In robotics and in computer vision, a pose is the location and
  the orientation of a body. In the plane, it is a rotation and a translation, that is a rigid motion (the group SE(2)),
  so the pose of a placement is its innermost step. In technical Finnish, *asema* also covers an angle (*kulma-asema*),
  and in astronomy the position angle (*asemakulma*) gives the orientation of an object on the sky. Robotics separates
  *asema* (location) and *asento* (orientation). In the theory, a position is the point alone.
- **kohta** · *position along the path* — A distance from the start of a path, in the measure of the line: the
  arc-length parameter s of the path. The pose at a position along the path is the point of the path there and the
  direction of its tangent. The first position of a repeated placement is a position along the path, and its step is the
  distance between two adjacent ones. The channels take a distance in the same way: `SkPathMeasure.getPosTan` of Skia
  gives the point and the tangent at a distance, and `getPointAtLength` of SVG gives the point.
- **`Placement.Steps`** (work) · sijoituksen sisäkkäiset affiiniset muunnokset · *steps of a placement* — Their Finnish
  term is not *vaiheet*. The affine transformations in the order in which the symbols set them. A placement keeps its
  sequence of steps, and the pose is the innermost step. Skia composes nested transformations with its own rounding.
  With the same steps in the same order, the coverage is the same to the pixel.
- **askel, määrä** · *step, count* (work: `RepeatedShapes.Step`, `RepeatedShapes.Count`) — The distance along the path
  between two adjacent positions along the path of a repeated placement, and the number of its poses.
- **kuvioruutu** · *pattern tile* (`AreaPatternPrototype.Tile`, `PatternTile`) — One cell of an area
  pattern, which is composed from single elements. It depends on the definition alone: the glyph parts of the element
  in one palette color at their places in the cell, as the pattern brush of the old drawing has them
  (`AreaSymDef.CreatePatternBrushes`). The area symbol definition makes it once (`AreaSymDef.MakePatternTile`,
  proposed name). The Skia encoding rasterizes it as a pattern brush (*kuviosivellin*).
- **valitut glyyfin osat, epäsäännöllisen kuvion sijoitukset** · *selected glyph parts, placements of an irregular
  pattern* — The glyph parts that a fill mode selects are clipping objects of the palette color coverage of the symbol
  with the full coverage: shared shapes of the glyph of the area pattern, each at a single placement. The placements of
  the elements of an irregular pattern are derived values of the palette color coverage of the symbol with the
  irregular area pattern.

## The rendering side · Renderöinnin puoli

The rendering side reads the intermediate layer. It keeps none of the results of the channel conversions that the
encodings make themselves.

- **vaihe 2, koostaminen** · *pass 2, layer composition* — *Vaihe* means a pass. It takes the palette separations in
  palette order. For each palette separation, an encoding writes the transparency group of its palette color, to which
  C, α and B apply once. In the group, the encoding writes each palette cutter with one fill and one clip, or each
  palette color coverage of a symbol separately. Pass 2 does not call the drawing code of the symbols (`Draw`). The
  internal render option `RenderOptions.drawColorsDirectly` makes `Map.Draw` draw the palette colors with the old
  drawing code (`Map.DrawColor`) also on a graphics target that gives an encoding. Only the tests set it.
- **palettivärin läpinäkyvyysryhmä** · *transparency group of a palette color* — The group that pass 2 composites for
  one palette color. Its content is the color C on the coverage of the palette separation. The group is composited once
  onto the backdrop, with the opacity α and the blend mode B of the palette color. So the overlaps of one palette color
  do not get α twice. One transparency group is for each palette color. The palette color is its definition, and the
  palette separation is its content. In a drawing, pass 2 makes it as a `TransparencyGroup`: the palette
  color, its opacity α, the blend mode B that the overprint mode gives (`OverprintMode`), the resolution of the drawing,
  whether the encoding fills a regular clipped area pattern with a pattern brush (`TransparencyGroup.UsePatternBrushes`,
  proposed name), and the palette color coverages of the symbols that the rectangle of the drawing can show.

  The theory: the transparency model of PDF (ISO 32000) calls this unit a transparency group. The objects of the group
  are composited into one result, and the result is composited onto the backdrop with the opacity and the blend mode of
  the group. The model of W3C (Compositing and Blending) calls it a group, and image editing and `saveLayer` of Skia
  call it a layer. A group is composited in a blending color space, which can be RGB, or CMYK with spot inks. The
  separable blend modes, for example Darken and Multiply, apply to each color component. So in this model the group is
  the same in all the rendering modes, and only the blending space changes. In a CMYK group, Darken takes the larger
  share of each ink, as the PDF export of OCAD does. The transparency flattening flattens these groups.
- **sekoitusväriavaruus** · *blending color space* — The color space in which a transparency group composites with its
  blend mode, as the transparency model of PDF calls it. A separable blend mode applies to each component of the
  blending color space. Skia composites in RGB: there, Darken takes the minimum of each component after the conversion
  of the colors to RGB, and Multiply their product. In CMYK, PDF applies a blend mode to the complements of the inks,
  so Darken takes the larger share of each ink. The two give the same result only when each ink changes one RGB
  component alone, which is not true for black or for the SWOP conversion of the screen (`SwopColorConverter`).
- **`OverprintMode`** · päällepainatustila · *overprint mode* — The choice of the course appearance
  (`CourseAppearance`) that tells which blend mode the drawing applies, in all the channels: `None` (*ei käytössä*),
  `ByPaletteColor` (*palettivärin mukaan*: the blend mode B of each palette color, `SymColor.BlendMode`), `Darken`
  (*tummenna*) and `Multiply` (*kerro*). `Darken` and `Multiply` apply to the overprint colors
  (`SymColor.OverPrint`). The opacity α applies in each mode. The render options keep it
  (`RenderOptions.overprintMode`), and so do the course appearance (`CourseAppearance.overprintMode`),
  `MapDisplay.OverprintMode` and `Controller.OverprintMode`. The course file keeps the attribute `overprint-colors`,
  which is true in the modes `Darken` and `Multiply`, and the attribute `overprint-mode` (`by-palette-color`,
  `multiply`) in the modes `ByPaletteColor` and `Multiply` only. **Upstream:** the overprint effect
  (`CourseAppearance.useOcadOverprint`, `RenderOptions.blendOverprintedColors`), which blends the colors that have the
  overprint flag with Darken, and the course map always blends its colors in this way.
- **piirtotila, väriasetus** · *rendering mode, color configuration* — A rendering mode is a color configuration of pass
  2: the normal view, the overprinting that the inks simulate, or the separations. All the rendering modes use the same
  intermediate layer. **Upstream:** the render options (`RenderOptions`). For example, `blendOverprintedColors` blends
  the colors that have the overprint flag with Darken, and `colorBeginDrawExclusive` and `colorEndDrawInclusive` draw
  only some of the colors.
- **koodaus, koodaaja** · *encoding, encoder* — It writes the drawing for one channel. It chooses the channel
  conversions of `Map` that its channel can use, and writes their results in the vocabulary of its channel. Its own
  conversions are only those into that vocabulary and the adaptations to the deviations of its channel
  (*kanavamuunnos*). When an encoding needs information, the map side keeps it, and the encoding recognizes no
  structure. Where the implementation of a channel does not agree with the ideal form, the encoding chooses a channel
  conversion of `Map`, for example a polyline. An encoding writes its channel directly, and not through
  `IGraphicsTarget`: the SVG encoding writes SVG, the PDF encoding the objects of PdfSharp, and the Skia encoding an
  `SKCanvas`. An encoding writes one transparency group of a palette color at a time (`IPaletteStackEncoding`). A
  graphics target that shares its surface with an encoding gives the encoding to the drawing
  (`IPaletteStackEncodingProvider`). The Skia encoding is `SkiaEncoding`.
- **piirron resoluutio** · *resolution of a drawing* (`RenderOptions.minResolution`) — The size of a pixel of a
  drawing, in paper coordinates. The screen and the Skia rendering tests give it the size of a pixel. Pass 1 takes no
  resolution. Pass 2 adapts to it: an encoding chooses the clipping objects at the resolution of the drawing, and the
  pattern brush of an area pattern.
- **kanava** · *channel* — An output for which an encoding writes. The epic names the rendering channels Skia, PDF and
  SVG. A channel offers abstractions: for example the dash pattern of PDF and SVG, the 1D path effect of Skia and the
  markers of SVG.
- **perustapaus** · *base case* — A case that a channel solves with an abstraction of its own. In a base case, the
  encoding converts the structure directly into a structure of the channel, and otherwise it splits the structure. In
  computing, a base case is solved directly, and the other cases are split until only base cases remain. The splitting
  of the encodings ends in single dashes and placements, which each channel draws directly. Here the base case depends
  on the channel, as in the legalization of a compiler: an operation that the target supports directly is legal, and the
  others are expanded (LLVM: *Legal*, *Expand*). The expansion (*laajennus*) is that splitting. A case is a class of the
  structures that one branch handles.
- **`IGraphicsTarget`** · piirtokohde · *graphics target* — The interface on which the symbols draw now. Its
  implementations are classes such as `Skia_GraphicsTarget` and `Pdf_GraphicsTarget`. The code of upstream says
  *graphics target* 44 times, in 16 files. It says *drawing target* only in "the graphics drawing target", in two
  comments of `IGraphicsTarget` that its five implementations repeat. The application that ships uses the graphics
  targets of `Map_SkiaStd` and `Map_PDF`. When pass 2 and the encodings draw from the intermediate layer, it stays
  beside the palette stack. It draws what does not belong to the palette stack: the templates, the highlights and the
  control descriptions (`DescriptionRenderer`), and the old drawing code until that code goes away. The encoding and the
  graphics target of one channel share the surface of the channel, for example the same `SKCanvas` or the same PDF
  page. Its vocabulary is a subset of the imaging model of PostScript, with flags for the differences of the channels
  (`SupportsPatternBrushes`).
- **kuviosivellin** · *pattern brush* (`IGraphicsTarget.CreatePatternBrush`, `RenderOptions.usePatternBitmaps`) — A
  brush that tiles a bitmap of one cell of an area pattern. The old drawing fills a regular clipped area pattern with
  it when `usePatternBitmaps` is set and the bitmap is not too coarse (`AreaSymDef.PatternBrushTooPixelated`), as it
  is faster than the drawing glyph by glyph. The display uses it, and the printing does not. It leaves out the
  rotation center of an area symbol whose angle is 0. The Skia encoding makes it from a pattern tile (*kuvioruutu*),
  where the brush is not too coarse at the resolution (`PatternTile.IsTooPixelatedAt`, proposed name), and places it by
  M, within the placement of the brush for the area symbol (`PatternTile.GetBrushPlacement`,
  `AreaSymDef.GetPatternBrushPlacement`, proposed names). It writes the brush with the Skia calls of the old drawing.
- **`RecordingGraphicsTarget`** · tallentava piirtokohde · *recording graphics target* — A graphics target that records
  the calls of a drawing and plays them back on another graphics target, for example for the print preview
  (`RecordingPrintTarget`). Pass 2 records its drawing as a deferred drawing (`IDeferredDrawingTarget.DrawDeferred`),
  which reads the map at the playback and writes with the encoding of the target that plays it back.
- **piirrosobjekti** · *drawn object* — An object that an encoding writes. Each drawn object refers to its palette color
  coverage of a symbol, and through it to its `Symbol`, `SymColor` and G. The reference gives the `Map` and the symbol
  key (`Symbol.Key`). In a file, the drawn objects keep a link back to the ideal forms that is as direct as possible,
  also through the hierarchy of the file, so that the SVG graphics can be a part of a web user interface.
- **viittaus** · *reference* — What the structure of `Map` lets a drawn object refer to is not duplicated in the
  drawn objects. They use it through references. A drawn object links directly to the parts of the content of `Map`.
  It can take the geometry that it draws by reference from the shared parts of the intermediate layer. The link and
  the reuse are different references, and they do not limit each other. In SVG, `<use href>` and `fill="url(#…)"`
  reuse, and the hierarchy of the groups or an identifier of the drawn object links. In PDF, a form XObject reuses, and
  marked content links.
- **itsensä kannatteleva vektoritiedosto** · *self-supporting vector file* — An SVG or a PDF file that carries the
  content of `Map`, so that Purple Pen can one day read it from the file. The opening is not in the scope of the epic.
- **korostus** · *highlight* (`Symbol.DrawHighlight`, `CourseObj.DrawHighlight`) — It is drawn directly on the graphics
  target, and it stays a direct drawing beside the palette stack in the channels that have a graphics target. A direct
  drawing keeps nothing over the renderings: the selection keeps the state, and each rendering draws the highlights
  again. **Open:** how the highlights are expressed in the SVG DOM of the browser, and whether they stay there over the
  renderings: as a style of the drawn objects through their symbol keys, as a layer of their own, or as a direct
  drawing on a drawing surface in the DOM, for example a canvas, as on a graphics target.
- **läpinäkyvyyden litistys** · *transparency flattening* — The conversion of the transparencies into opaque vector
  objects. It is not in the scope of the epic.

## Words that are not names · Sanat, jotka eivät ole nimiä

- **malli** · *model* — Too ambiguous to name a concept in the specification.

  The decisions of Nov 20 say "the model" (in Finnish *malli* and *karttamalli*). Their readings by the structure of the
  passes:
  - "the map model keeps the ideal forms": `Map` keeps the ideal forms, that is the information that an encoding needs.
    The derived values are not ideal forms, but practical adaptations of them to the needs of the channels. They keep
    the dashes, the runs and the paths of the repeated placements on a curve as Bezier pieces or as polylines, as the
    implementations that the channels choose need, and possibly in both forms.
  - "the implementation of the channel does not agree with the model": with the ideal form of `Map`. When the decision
    was made, `Map` did not offer derived values yet. Now `Map` also offers the means to make and to keep derived
    values, where the shared use of the channels or a cache of the results makes it sensible.
  - "the model keeps it, and the encoding reads the structure from the model": `Map` keeps the information that an
    encoding needs, and the intermediate layer gives access to it in forms that are more usable for the needs of the
    channels.
  - "the model keeps the structure of a line": `Map` keeps the information that is necessary, that is the ideal forms:
    the base path with the flags of its points, the `LineSymDef` with its `DashInfo`, `DoubleLineInfo`, `ShortenInfo`
    and `GlyphInfo`, and the metric. The parts of the line, the segments, the fitted dash patterns, the merged end
    dashes and the exceptions of fix 020 are derived values, which `Map` can keep for the channels where the
    implementation finds it sensible.
  - "the measure of the model": the metric that the source means to be used, which `Map` keeps in the ideal form
    (`Map.UseEuclideanMetric`). `Map` offers the means to convert the metric to suit a channel, for example into runs in
    the euclidean measure, and to keep the derived values.
  - "an SVG or a PDF file carries the model" and "Purple Pen reads the model from the file": the content of `Map`. The
    drawn objects of the file keep a link back to the ideal forms that is as direct as possible, also through the
    hierarchy of the file, so that the SVG graphics can be a part of a web user interface.
  - "the parts of the model": the parts of the content of `Map`. A drawn object links directly to them. It can take the
    geometry that it draws by reference from the shared parts of the intermediate layer. The link and the reuse are
    different references, and they do not limit each other.
  - "what the structure of the model lets an object refer to": the structure of `Map`. The drawn objects do not
    duplicate what it lets them refer to. They use it through references.
  - "the epic includes a model that makes it possible": `Map` with the parts to which an encoding can refer. They make
    it possible to open a file as a map. The intermediate layer is also a part of the epic, but this decision does not
    concern it.
  - "the model offers the known conversions": `Map`. It offers the known channel conversions as means to make derived
    values from its ideal forms, and keeps their results where the shared use of the channels or a cache makes it
    sensible. An encoding chooses a channel conversion that its channel can use. With channel conversions of its own, it
    makes only what no channel conversion of `Map` covers.
- **karttamalli** · *map model* — A vague reference. It can mean three names of the code:
  - the folder `src/MapModel`: the map model and the graphics targets, in many projects, for example `Graphics2D`,
    `Map_SkiaStd` and `Map_PDF`;
  - the project `src/MapModel/MapModel/MapModel.csproj`: the classes that read, hold, write and draw the maps, for
    example `Map`, `SymColor`, `SymDef`, `Symbol`, `Glyph` and `SymPath`. The palette-stack work adds the classes of
    the intermediate layer to it;
  - the namespace `PurplePen.MapModel`: the classes of that project, and also classes of `Map_SkiaStd`, `Map_PDF`,
    `Map_GDIPlus`, `Map_WPF` and `Map-iOS`.
- **karttakoordinaatit, maailmankoordinaatit** · *map coordinates, world coordinates* — Ambiguous names that the
  comments of upstream give to the paper coordinates. They are not used.
- **separaatio, erottelu** · *separation* — Not a term alone. *Separaatio* is *erottelu* in Finnish. The terms are
  *värierottelu* · *ink separation*, *palettierottelu* · *palette separation* and *spottivärierottelu* · *spot color
  separation*.
- **kerros** — Not a term. The decision concerns the Finnish word.
- **ryhmä** — Not a term alone. The decision concerns the Finnish word. *Palettivärin läpinäkyvyysryhmä* is a term.
- **ajo** · *run* — Not a term in the sense of a sequence, where it translates *run* word for word. The terms are
  *katkojuoste* · *run of dashes* and *tekstijuoste* · *text run* (`TextShape`). A run of the tests (*täysi ajo*) is a
  word of the process.
- **osa** · *part* — Not a term without a qualifier. The terms are *viivan osa* · *part of a line*, *segmentti* ·
  *segment*, *viivan osatekijä* · *line component*, *glyyfin osa* · *glyph part* (`Glyph.GlyphPart`), *yhdistelmän
  osamäärittely* · *component definition* and *osapolku* · *subpath* (`SymPath.GetSubpaths`).
- **reuna** · *border* — Not a term without a qualifier. The terms are *kaksoisviivan reuna* · *double line border* and
  *alueen reuna* · *area border*.
- **täyttö** · *fill* — Not a term without a qualifier. The terms are *täytetty muoto* · *filled shape* (`FilledShape`),
  *täyttösääntö* · *fill rule*, *alueen täyttö* · *area fill*, *kaksoisviivan täyttö* · *double line fill*,
  *kuviotäyttö* · *pattern fill* and *täyttötila* · *fill mode*.
- **kuvio** · *pattern* — Not a term without a qualifier. The terms are *aluekuvio* · *area pattern* (`PatternInfo`),
  *katkokuvio* · *dash pattern* (`Stroke.DashPattern`), *sovitettu katkokuvio* · *fitted dash pattern*, *kuviotäyttö* ·
  *pattern fill* and *kuvioruutu* · *pattern tile*.
- **toisto, toistuva** · *repetition, repetitive* — Not a term without a qualifier. The terms are *toistosijoitus* ·
  *repeated placement* and *jaksollinen peitto* · *periodic coverage*.
- **litistys, litistäminen** · *flattening* — Not a term without a qualifier. The terms are *läpinäkyvyyden litistys* ·
  *transparency flattening* and *polun litistäminen* · *path flattening*.
- **muunnos** · *transformation, conversion* — Too generic to be a term without a qualifier, as *malli* is. The terms
  are *affiininen muunnos* · *affine transformation*, for example M, L, E and the steps of a placement, and
  *kanavamuunnos* · *channel conversion*.

## Not in this glossary

The words of the process and of the tests: the collector (*kerääjä*, work: `Map.CollectLayerInstances`), the oracle
(*oraakkeli*), the geometry method (*geometriametodi*), the baseline, the full run (*täysi ajo*) and the coverage
comparison (*peiton vertailu*).

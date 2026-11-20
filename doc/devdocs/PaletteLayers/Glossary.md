# Glossary of the palette-layer architecture

Nov 20, 2026. This glossary gives the names of the concepts that the specification of the palette-layer architecture
needs: the name in the code, the Finnish term and the English term. The content of many concepts is not decided or not
delimited yet. Each entry tells what the code, the specification and the decisions say now, and marks what is open.

The rules of the names (@Pauli, Nov 20):
- The names follow the concepts of upstream, unless there is a special reason to deviate.
- The name of a concept in the code is its primary name, also in Finnish text.
- The word *malli* (*model*) alone is too ambiguous to name a concept in the specification.
- When @Pauli uses a vague reference, such as *karttamalli*, the session asks him to make it exact.

The structure of the passes (@Pauli, Nov 20) divides the concepts into three parts: the map side, the intermediate
layer that pass 1 makes from the map, and the rendering side. Section 02 of [Specification.md](Specification.md) gives
the structure. The sections of this glossary follow the three parts.

Each entry starts with the primary name of its concept. Where the concept has a name in the code, the entry gives that
name first, and then the Finnish and the English term. Where it has none, the entry starts with the Finnish and the
English term. The tickets use the Finnish terms, and the other documents use the English terms. "Proposed name" marks
a name that is not decided.

The sources are given in parentheses. "spec 01" is section 01 of [Specification.md](Specification.md). A number such as
"011" is a ticket in [tickets/](tickets/). A date is a decision of @Pauli, as [Progress.md](Progress.md) and the
tickets record it. A name in code font is a name in the code of upstream at d48f4456, where `base` meets upstream. A
name marked "work" is a name in the code of `feature/palette-layers` or of the branches of the tickets 011 to 013
(`feature/text-geometry` at 2ad8c6f39). "**Upstream:**" gives the name of upstream where it is different from the term
of the entry.

The section "Words that are not names" lists the words that are not used as names. The section "Words with more than
one meaning" lists the words that name two or more concepts now.

## The map side · Kartan puoli

`Map` is the source. It keeps the structure in its ideal form (spec 02, Nov 20).

### The map and its objects

- **`Map`** · kartta · *map* — It holds the colors (`SymColor`), the symbol definitions (`SymDef`), the symbols
  (`Symbol`), the templates (`TemplateInfo`), the scales, the print area, the georeferencing and the metric. It keeps
  the OCAD setup and the OCAD symbol header, to write them back (`OcadSetup`, `OcadSymbolHeader`). It also holds a
  state of the editing and of the drawing: an undo manager (`UndoMgr`), the changed areas, a cache of the bounds and
  the pens of the highlights. The map of the courses is a `Map` that `CourseLayout.RenderToMap` makes from the courses
  (005).
- **`SymDef`** · symbolimäärittely · *symbol definition* — The definition of a symbol, for example `PointSymDef`,
  `LineSymDef`, `AreaSymDef` or `TextSymDef`. The comments of upstream call it a *symdef*. The specification calls it
  a symbol (spec 02).
- **`Symbol`** · symboli · *symbol* — One object of a map, for example `PointSymbol`, `LineSymbol`, `AreaSymbol` or
  `TextSymbol`. Its geometry is in paper coordinates. A `PointSymbol` and a `RectLikeSymbol` have a location, a
  `LineLikeSymbol` has a path (`SymPath`), and an `AreaLikeSymbol` has a path with holes (`SymPathWithHoles`). The
  comments of upstream also call it an object, for example a line object or an area object. The Finnish tickets call
  it a symbol (005). The specification calls it a map object (spec 02).
- **paperikoordinaatit** · *paper coordinates* — The coordinates of a map, in millimeters on the printed map. The
  points are `PointF` values. The OCAD import divides the OCAD units of 0.01 mm by 100
  (`OcadImport.ToWorldDimensions`). @Pauli holds *paper coordinates* to be the established name. He finds *physical
  coordinates* a good name, but a fourth name is probably not worth it (Nov 20). The comments of upstream also say
  *map coordinates* and *world coordinates* (see "Words that are not names"). The real-world coordinates
  (`RealWorldCoords`) are a different concept: the georeferencing of the map.
- **ideaalimuoto** · *ideal form* — The form in which a structure is kept, apart from the form of a channel. `Map`
  keeps the curves of the paths as Bezier curves. On a curve, the dashes, the runs and the paths of the repetitions are
  Bezier pieces of the line (Nov 20). Ellipses and arcs stay ellipses and arcs (005).

### Palette and color

- **`SymColor`** · palettimerkintä · *palette entry* — The palette entries are the colors of the normal layer
  (`SymLayer.Normal`). A `SymColor` also marks the image layer and the layout layer, and these are not palette entries
  (005). A palette entry has a color or ink components, an opacity, a blend mode, a knockout and a priority (spec 02,
  06; 001, 004). The comments of upstream also call the colors layers ("colors (layers)", `RenderOptions`).
- **palettikerros** · *palette layer* — The layer of a palette entry: the uniform color of the entry over the drawing.
  In spec 01, a palette layer has C, α, B, G and M. Spec 02 divides these between the palette entry and its layer
  instances, the palette cutters. **Open:** how does a palette layer differ from a palette entry and from its group?
- **palettipino, palettijärjestys, prioriteetti** · *palette stack, palette order, priority* — The ordered stack of
  the palette layers, and the order in which pass 2 composites the groups (spec 01, 02, 05). The colors of a map are
  `Map.AllColors`, and upstream draws them from the lowest to the highest (005). One palette stack is for all the maps,
  the courses included (000).
- **väri C** · *color C* — A spatially constant color, independent of color space (spec 01). A `SymColor` of
  upstream keeps a CMYK color (`CmykColor`).
- **`SymColor.Opacity`** (work) · peittävyys α · *opacity α* — A spatially constant opacity (spec 01; 004).
- **`SymColor.BlendMode`** (work) · sekoitustila B · *blend mode B* — The compositing operation (`ColorBlendMode`)
  (spec 01; 004). For an OCAD map, B = Darken is the larger share of each ink, and not the RGB minimum
  ([oracles/ocad-darken.md](oracles/ocad-darken.md)).
- **`Ink`** (work) · muste, prosessimuste, spottimuste · *ink, process ink, spot ink* — A colorant identity, apart
  from the palette entry. It has a name (`Name`), a CMYK appearance (`Appearance`), and a screen frequency and angle
  where they are defined (`Screen`, `InkScreen`) (spec 06; 001). The process inks C, M, Y and K always exist
  (`Ink.ProcessInks`). A spot ink can be a palette entry by itself, at full tint (`SymColor.SpotInk`) (001).
- **`InkComponent`** (work) · mustekomponentti, kerroin · *ink component, factor* — A palette entry holds ink
  components (`SymColor.InkComponents`). Each has an ink and a factor from 0 to 1. A missing component and the factor 0
  are different (001). The specification says *ink contribution*.
- **separaatio** · *separation* — The coverage of one ink. A missing share leaves the separation as it is, 0 % erases
  it, and a positive share sets the coverage (spec 06). Spec 02 and spec 08 also say *color separation* and *palette
  separation* for the coverage of one palette entry (see `PaletteSeparation`).
- **`SymColor.OverPrint`** · päällepainatus · *overprint* — The overprint flag of an OCAD process color keeps the inks
  below (spec 06). The option `RenderOptions.blendOverprintedColors` draws the colors that have it with Darken. The
  inks simulate the overprinting in a rendering mode (000).
- **knockout** · *knockout* — The flag of Mapper. Its intent is the inverse of the overprint flag: it removes the inks
  below (spec 06). The Mapper import sets `SymColor.OverPrint` only for a color that has a spot definition and whose
  knockout is off (001).

### Geometry prototypes

- **`GeometryPrototype`** (work) · geometriaprototyyppi · *geometry prototype* — Reusable, origin-bound, periodic
  geometry G (spec 02). On the map side, G is a part of a `SymDef`: the full coverage, one hatching (an index in
  `AreaSymDef.hatchings`) or one pattern (an index in `AreaSymDef.patterns`) (Nov 20). In an `AreaComboSymDef`, the
  component definition is part of G. Upstream has no class for G. **Open:** are the shared structures that are not
  periodic, for example the shared shapes of a glyph, prototypes or a concept of their own?
- **toistuva geometria G** · *repetitive geometry G* — A binary, spatially repetitive, origin-bound geometry selector
  (spec 01). The hatchings and the area patterns are repetitive geometry (009, a draft).
- **`GeometryPrototype.FullCoverage`** (work) · täysi peitto, G = 1 · *full coverage* — The constant case of
  repetitive geometry (spec 01). The geometry of points, lines and texts has G = 1. Their dash patterns and their
  glyphs along a line are strokes and placements, not lattices (Nov 19, Nov 20).

### Glyphs

- **`Glyph`** · glyyfi · *glyph* — "A glyph is used to define a point symdef or part of a linesymdef or area symdef"
  (the comment of `Glyph`). Its parts are lines, areas, circles and filled circles (005).
- **`extraTransform`** · glyyfin lisämuunnos · *extra transformation of a glyph* — It transforms the points of a glyph
  but not its line widths, for example for the scale at corners and for the glyphs that decrease. So it is not an
  affine placement of a stroked shape (011).

### Lines

- **`LineSymDef`, `LineComboSymDef`** · viivan symbolimäärittely · *line symbol definition* — A `LineSymDef` has the
  dashes (`DashInfo`), the double line (`DoubleLineInfo`), the shortening and the pointy ends (`ShortenInfo`) and the
  glyphs (`GlyphInfo`). A `LineComboSymDef` gathers the parts of other line symbol definitions (011). The comments of
  upstream call a `LineSymDef` a *linesymdef*. Ticket 011 calls these line symbols.
- **`LineSymbol`** · viivasymboli · *line symbol, line object* — One line object of a map. Its path is
  `LineLikeSymbol.Path`, a `SymPath`.
- **looginen polku** · *logical path* — The path of a line in its ideal form. The parts of the line are its pieces and
  offsets (Nov 20). **Upstream:** a line object has one path (`LineLikeSymbol.Path`), and its definition draws all the
  parts of the line from it (011). **Open:** is the logical path `LineLikeSymbol.Path`?
- **keskiviiva** · *middle of the line* — The base path of the borders of a double line (Nov 20). The English
  documents have no single term for it.
- **`DoubleLineInfo`** · kaksoisviiva, reuna, täyttö · *double line, border, fill* — A double line has a fill and two
  borders (005, 011). Upstream calls the borders the left and the right double line, with their colors, widths and
  dashes (`doubleLeftColor`, `doubleRightColor`), and calls the band between them the fill (`doubleFillColor`).
  `doubleThick` is the distance between the double lines and the thickness of the fill.
- **`DashInfo`** · katko, väli, toissijainen väli · *dash, gap, secondary gap* — The dashes of a `LineSymDef`: the
  lengths of the dashes and of the gaps (`dashLength`, `gapLength`), and the secondary gaps (`secondaryMiddleGaps`,
  `secondaryEndGaps`) (011). Upstream calls the dashes between the first and the last dash the main dashes, and has
  the lengths of the first and the last dash (`firstDashLength`, `lastDashLength`).
- **`ShortenInfo`** · lyhennys · *shortening* — The shortening of the beginning and of the end of a line
  (`shortenBeginning`, `shortenEnd`). It is measured with the metric of the map (011). `ShortenInfo` also has the
  pointy ends (`pointyEnds`).
- **`SpacingMethod.OCAD`** · OCADin välistys · *OCAD spacing* — The method that fits the dashes and the glyphs of a
  part as OCAD does (017, 021). The other spacing methods of upstream are `OpenMapperDashes` and
  `OpenMapperMidSymbols`.
- **`SymPath.DistanceMetric`** · metriikka · *metric* — The OCAD metric is the larger coordinate difference plus half
  the smaller one (`SymPath.BizzarroDistance`): a norm whose unit circle is an octagon (011). A map uses the OCAD
  metric when its view parameter `p` is 1. Mapper maps use the euclidean metric (011, 016). The metric of a map is
  `Map.MapDistanceMetric`, and `Map.UseEuclideanMetric` selects it. The OCAD metric is best effort (Nov 19).
- **`PointKind.Corner`, `PointKind.Dash`** · kulmapiste, katkopiste · *corner point, dash point* — The points of a path
  that have the OCAD flags y & 1 and y & 8. They divide a line into parts (011).
- **taitepiste, jana** · *vertex, segment* — A point of a path, and the straight segment between two points.
  **Upstream:** the points of a path have the kinds `Normal`, `BezierControl`, `Corner` and `Dash` (`PointKind`). The
  flat points of a path also have interpolated points (`PointKind.Interpolated`). **Open:** in the decisions on the
  questions 6 and 7 of 011, they were the points and the segments of the flattened path. @Pauli took a vertex to be a
  corner point that the user puts on the curve (Progress.md, Nov 19).
- **Bezier-käyrä** · *Bezier curve* (`PointKind.BezierControl`) — A curve of a path, with its control points.

### Patterns

- **kuviotäyttö** · *pattern fill, structured fill* (`HatchInfo`, `PatternInfo`) — The hatchings and the area patterns
  (spec 03; 009, a draft).
- **`HatchInfo`** · viivoitus · *hatching* — It has a color, a width, a spacing, an offset, an angle and a rotate mode
  (`hatchColor`, `hatchWidth`, `hatchSpacing`, `hatchOffset`, `hatchAngle`, `hatchRotateMode`). Upstream calls its
  lines hatch lines. **Open:** is a hatching a prototype of its own, or a pattern of line elements (009, question 3)?
- **`PatternInfo`** · aluekuvio · *area pattern* — It has a pattern element (`patternGlyph`), the size of the pattern
  element (`patternWidth`, `patternHeight`), an offset of the alternate rows by 1/2 (`offsetRows`), an angle, an
  offset, a rotate mode, a fill mode and the irregularity (009). Upstream calls it a pattern.
- **hila, hilavektori** · *lattice, lattice vector* — The arrangement in which a pattern repeats its elements
  (spec 03). **Upstream:** no name. The size of the pattern element and the offset of the rows give the lattice.
- **`PatternInfo.patternGlyph`** · elementti · *element* — The geometry that the lattice repeats. The center of an
  element is its origin, the point that the lattice places at a lattice vector (spec 03). Upstream calls it the pattern
  element, and the structure element in the comments of the fill modes.
- **hilan muunnos L, elementin muunnos E** · *lattice transformation L, element transformation E* — The point e of the
  element at the lattice vector v is at L(v) + E(e) (spec 03).
- **`PatternInfo.patternAngle`, `HatchInfo.hatchAngle`, `AreaSymbol.Angle`** · rakennekulma, alueobjektin kulma ·
  *structure angle, angle of the area object* — In OCAD, the structure angle of the symbol turns the lattice alone, and
  the angle of an area object turns the lattice and its elements together (spec 03). The structure angle is
  `patternAngle` or `hatchAngle`, and the angle of an area object is `AreaSymbol.Angle`.
- **`AreaSymbol.RotationCenter`** · kiertokeskus · *rotation center* — The point about which the angle of an area
  object turns its hatchings and patterns (009).
- **`AreaSymDef.RotateMode`** · kiertotila · *rotate mode* (`Always`, `ManualOnly`, `Never`) — The comments of
  upstream: `Always` turns the pattern when the map or the object turns. `ManualOnly` allows only a manual setting of
  the direction of the pattern, and does not turn the pattern when the object or the map turns. `Never` lets no
  direction change the drawing of the pattern (009). **Open:** what does `ManualOnly` mean in the specification (009,
  question 4)?
- **`AreaSymDef.PatternFillMode`** · täyttötila · *fill mode* (`Clip`, `CompletelyInside`, `CenterInside`,
  `PartiallyInside`) — It clips the elements, or selects the complete elements that are entirely inside, centered
  inside or partially inside the area (spec 03). Figure 3 of spec 03 calls them *boundary modes*. Upstream: `Clip` was
  the only mode until OCAD 12. Upstream selects each part of a glyph by its own points and center (`GlyphPartFilter`),
  and not the complete element.
- **`PatternInfo.irregular`** · epäsäännöllinen kuvio · *irregular pattern* (`irregularVarX`, `irregularVarY`,
  `irregularMinDist`) — Its elements move a random but deterministic distance. The seed comes from the rounded
  position of the element (009). The randomness is kept as a definition (Nov 20).

### Texts

- **`FramingStyle.Line`** · kehysviiva · *framing line* — The framing of a text symbol definition in the style
  `FramingStyle.Line`. The other styles are a shadow and a rectangle. The framing of a text run is one line: a stroke
  of the full width with the joins of the symbol (Nov 19).

### Beside the palette stack

- **`TemplateInfo`** · pohjakartta · *template* (`LoadedTemplate`) — It is not a palette entry (005). `Map.Draw` draws
  the templates under and over the palette entries now. An OCAD template is a `Map` of its own (000).
- **`SymLayer.Image`** · kuvakerros · *image layer* — The layer below all the normal layers. Its objects are image
  objects, with a special symbol definition (`ImageSymDef`). It is not a palette entry (005).
- **`SymLayer.Layout`** · asettelukerros · *layout layer* — The layer above all the normal layers. Its objects are
  layout objects, with a special symbol definition (`ImageSymDef`). It is not a palette entry (005).
- **ratakartta** · *course map* (`CourseLayout.RenderToMap`) — The map of the courses is a `Map` of its own, so the
  same pass 1 applies to it (005). One palette stack is for all the maps (000).

## The intermediate layer · Välikerros

`Map` makes the intermediate layer from the map alone, in pass 1. It is not source data: `Map` stays the source
(spec 02, Nov 20).

### The separations and the cutters

- **vaihe 1, ilmentäminen** · *pass 1, layer instantiation* (work: `Map.CreateLayerInstances`) — `Map` makes the
  palette separations and their palette cutters from the map alone, and no rectangle or resolution of a drawing
  (spec 02; Nov 18, Nov 20). The symbol separations get their derivations from the geometry methods of the symbol
  definitions (008; 011 to 013 in progress).
- **`SymbolSeparation`** (proposed name) · symbolin separaatio · *symbol separation* — The part of one `Symbol` that
  belongs to one palette entry and one geometry prototype G: the triple (`Symbol`, `SymColor`, G). It refers to these
  objects and copies nothing. It keeps its own derivations (spec 02, Nov 20).
- **`PaletteSeparation`** · palettiseparaatio · *palette separation* — The symbol separations of one palette entry. It
  keeps its palette cutters as a derivation (spec 02, Nov 20). The name is @Pauli's (Nov 20).
- **`PaletteCutter`** · palettileikkuri · *palette cutter* — The symbol separations of one palette separation that have
  the same configuration. It cuts the uniform color of the palette entry to the coverage that G under M gives inside
  the union of its clipping objects. Each symbol separation is in one palette cutter, and each palette cutter is in one
  palette separation. The specification calls it a layer instance (spec 02, Nov 20). The name is @Pauli's (Nov 20).
- **kokoonpano, samanlainen kokoonpano** · *configuration, equivalent configuration* — The palette entry, G and the
  effective transformation M of a symbol separation. M comes from the `Symbol` and from G. The symbol separations with
  the same configuration are one palette cutter (spec 02, 03; Nov 20). **Open:** when are two configurations the same
  (009, question 1)?
- **kerrosilmentymä** · *layer instance* (work: `LayerInstance`) — The name of the specification for the palette
  cutter (spec 02). The class `LayerInstance` of the work has a palette entry (`PaletteEntry`), a geometry prototype
  (`Prototype`), a transformation (`Transformation`) and kept clipping objects (`ClippingObjects`). For G = 1, there
  is one for each palette entry (005).
- **`LayerInstance.Transformation`** (work) · muunnos M · *transformation M* — An invertible 2D affine transformation
  of G (spec 01).
- **peitto, unioni** · *coverage, union* — The binary coverage that the geometry and the clipping objects define
  (spec 01, 05). The coverage of a palette cutter is the union of the coverages of its symbol separations, limited by G
  under M. The coverage of a palette separation is the union of the coverages of its palette cutters. The union can stay
  implicit in the rendering, but the instances cannot be left out (spec 05, Nov 18).

### The derivations

- **johdannainen** (proposed name) · *derivation* — A kept result that is the same for all channels. A symbol
  separation keeps its own derivations. A derivation that depends on a definition alone belongs to the definition,
  for example the shared shapes of a glyph. A palette separation keeps its palette cutters. A derivation is calculated
  when it is necessary the first time, and it becomes invalid when the map changes (spec 02, Nov 20). Upstream keeps
  derivations in the same way, for example `SymPath.flatpoints`, `TextSymbol.wrappedText` and `Symbol.boundingBox`.
- **viivan rakenne** · *structure of a line* — The logical path, the parts as its pieces and offsets, the measure of
  the line, the fitted pattern of a part, the end dashes and the exceptions of fix 020 (Nov 20). `Map` keeps the path
  of the `LineSymbol`, the `DashInfo`, the `DoubleLineInfo` and the metric. The parts, the fitted patterns, the end
  dashes, the exceptions and the quarters are derivations of the symbol separation.
- **osa** · *part* — A part of a line between corner and dash points. The dashes are fitted to each part separately
  (011). **Open:** in "the parts as its pieces and offsets" (Nov 20), is a part the same thing, or each component of
  the line, for example a border or a run?
- **pala** · *piece* — A piece of the logical path, from one place on the path to another (Nov 20). **Open:** its
  exact content.
- **siirtymä** · *offset* — The distance of a part from the logical path, to the side (Nov 20).
- **siirretty polku, pohjapolku** · *offset path, base path* — The shape of a border of a double line, in its ideal
  form: the path of the middle of the line and the offset. Its dash pattern is measured along its base path. The rule
  of the corners of the offset belongs to the shape, and the join belongs to the stroke (Nov 20). The offset curve is a
  derivation.
- **viivan mitta** · *measure of the line* (work: `OcadCurveMeasure`) — Part of the structure of a line (Nov 20). The
  measure of fix 021 (`SymPath.GetOcadCurveMeasure`) gives a straight segment its length, and a quarter of a curve its
  chord (021). **Open:** its exact content.
- **neljännes, jänne** · *quarter, chord* — Each quarter of a Bezier curve has its chord as its length in the measure.
  Within the quarter, the dashes are in proportion to the true arc (021, Nov 19).
- **litistetty polku, litistäminen** · *flattened path, flattening* (`SymPath.GetFlattenedPoints`) — The polyline
  that approximates the curves of a path. Upstream calls its points the flat points and keeps them in the `SymPath`
  (`flatpoints`). The ideal form keeps the curves, and an encoding can make a polyline (Nov 20).
- **sovitettu kuvio** · *fitted pattern* (`ComputeDashDistances`) — The dash and the gap that the fitting gives to a
  part (011). Part of the structure of a line (Nov 20).
- **päätykatko, yhdistetty katko** · *end dash, merged dash* — The end dashes of adjacent parts merge into one dash at
  a corner point or a dash point (011). Upstream has the lengths of the first and the last dash (`DashInfo`).
- **korjauksen 020 poikkeus** · *exception of fix 020* — A dash from which the rule of fix 020 leaves out the vertex
  (Nov 19). Part of the structure of a line (Nov 20).
- **ajo, katkoajo** · *run, run of dashes* — The middle dashes of a part as one periodic dashed line (011). The runs in
  the euclidean measure are the same for Skia, PDF and SVG, as their dash patterns measure the path euclidean (011,
  Nov 19). So they are a derivation of the symbol separation (spec 02, Nov 20). The decision of Nov 20 on the composed
  dashed lines says that an encoding makes the runs (011).
- **koostettu katkoviiva** · *composed dashed line* — The form of a line for a channel whose dash pattern measures the
  euclidean length: the runs and the single dashes (Nov 20).

### The forms of the derivations

- **`ClippingObject`** (work) · leikkausobjekti · *clipping object* — It limits the extent over which the geometry of
  its palette cutter applies. It gives no color or alpha (spec 01). It has a shape that is drawn (`Shape`), a fill or a
  stroke, and a placement (`Placement`) (Nov 18, Nov 20). The clipping objects of a symbol separation are its
  derivations (spec 02, Nov 20).
- **`ClippingObject.Shape`** (work) · piirtyvä muoto · *shape that is drawn* — The shape of a clipping object
  (Nov 20). For a fill, the shape is the outline of the area. For a stroke, the shape is the path at the middle of the
  band that the stroke covers.
- **`Shape`** (work) · muoto · *shape* — A path (`PathShape`), an ellipse (`EllipseShape`), an arc (`ArcShape`) or a
  text run (`TextShape`) (005). The border of a double line is an offset path (Nov 20).
- **`FilledShape`** (work) · täytetty muoto, täyttösääntö · *filled shape, fill rule* — Its fill rule is
  `FilledShape.FillRule` (`AreaFillMode`) (005).
- **`StrokedShape`** (work) · viivattu muoto · *stroked shape* — 005.
- **`Stroke`** (work) · viiva · *stroke* — The width (`Width`), the caps (`Caps`), the join (`Join`) and the miter
  limit (`MiterLimit`) of a stroked shape (005), and the dash pattern and its phase (`DashPattern`, `DashPhase`)
  (Nov 19). **Upstream:** a pen of the graphics target (`IGraphicsTarget.CreatePen`) with the same values. The symbol
  definitions give the caps and the joins together as a line style (`LineStyle`: `Rounded`, `Mitered`, `Beveled`,
  `FlatRounded`). **Open:** the dash structure of a part in the measure of the line (point 2 of 6 in the review of
  011).
- **`LineCapMode`, `LineJoinMode`, `Stroke.MiterLimit`** · pää, liitos, jiirausraja · *cap, join, miter limit* — 005.
- **`Stroke.DashPattern`, `Stroke.DashPhase`** (work) · katkokuvio, vaihe · *dash pattern, phase* — The lengths of the
  dashes and the gaps in turn, and the distance that the pattern has gone at the start of the shape, as
  `stroke-dasharray` and `stroke-dashoffset` of SVG (011). They are properties of `Stroke` (Nov 19). **Open:** after
  the decisions of Nov 20, the measure in which they are kept (point 2 of 6 in the review of 011).
- **`TextShape`** (work) · tekstiajo · *text run* — A word, or a grapheme where the character spacing is more than 0,
  at its own position (Nov 19). It has the text, the font family, the size, the effects and the top left corner
  (005). A text run is a shape.
- **jaetut muodot** · *shared shapes* — Each part of a glyph is one shape, which each placement of the glyph refers to
  (Nov 18; 008). They depend on the definition alone, so they belong to the definition (spec 02, Nov 20). Stretched
  and diminishing glyphs get shapes of their own (Nov 19). **Upstream:** a glyph at a corner is scaled, unless
  `noScaleAtCorners` is set, and the glyphs of `GlyphLocation.SpacedDecrease` decrease in size and spacing.
- **`Placement`** (work) · sijoitus · *placement* — It gives one or more positions. Each reader asks a placement for its
  positions (Nov 20). It composes its steps into one matrix (`Matrix`).
- **yksittäinen sijoitus** · *single placement* — It keeps one point and an angle (Nov 20).
- **toisto** · *repetition* — A placement that keeps its path, its first position, its step and its count. It
  calculates the angles of the positions from the direction of the path, with the method of the drawing (Nov 20).
  Only an encoding that gives a repetition to an abstraction of its channel reads the path and the step. On a Bezier
  curve, the glyphs of the OCAD spacing repeat by quarters (Nov 20). Its earlier form is a kind of clipping object
  (work: `RepeatedShapes`). **Upstream:** the glyphs that `GlyphLocation.Spaced` places along a line at the distances
  given.
- **paikka** · *position* — A point and an angle (Nov 20).
- **`Placement.Steps`** (work) · sijoituksen vaiheet, vaiheketju · *steps of a placement* — The transformations in the
  order in which the symbols set them (005). A placement keeps its sequence of steps, and the position is the innermost
  step (Nov 20). Skia composes nested transformations with its own rounding. With the same steps in the same order,
  the coverage is the same to the pixel (005).
- **askel, määrä** · *step, count* (work: `RepeatedShapes.Step`, `RepeatedShapes.Count`) — The distance between the
  positions of a repetition, and the number of its positions (Nov 19, Nov 20).
- **kuvioruutu** · *tile* — One cell of a pattern, which is composed from single elements (Nov 20). It depends on the
  definition alone.
- **valitut elementit, epäsäännöllisen kuvion sijoitukset** · *selected elements, placements of an irregular pattern* —
  The elements that a fill mode selects, and the placements of the elements of an irregular pattern, are derivations of
  a symbol separation (spec 03, Nov 20).

## The rendering side · Renderöinnin puoli

The rendering side reads the intermediate layer. It keeps none of the results of its channel conversions (spec 02,
Nov 20).

- **vaihe 2, koostaminen** · *pass 2, layer composition* — It takes the palette separations in palette order. For
  each palette separation, an encoding writes one group, to which C, α and B apply once. In the group, the encoding
  writes each palette cutter with one fill and one clip, or each symbol separation separately. Pass 2 does not call
  the drawing code of the symbols (`Draw`) (spec 02, 05; Nov 20). The specification calls the operation compositing.
- **ryhmä** · *group* — The group of a palette entry is its palette separation. Pass 2 composites each group once
  (spec 05; Nov 18, Nov 20).
- **piirtotila, väriasetus** · *rendering mode, color configuration* — A rendering mode is a color configuration of
  pass 2: the normal view, the overprinting that the inks simulate, or the separations (000; spec 06, 07). All the
  rendering modes use the same intermediate layer (Progress.md, "Next"). **Upstream:** the render options
  (`RenderOptions`). For example, `blendOverprintedColors` blends the colors that have the overprint flag with Darken,
  and `colorBeginDrawExclusive` and `colorEndDrawInclusive` draw only some of the colors.
- **koodaus, koodaaja** · *encoding, encoder* — It writes the drawing for one channel. It converts the derivations into
  the forms of its channel with channel conversions. When an encoding needs information, the map side keeps it, and
  the encoding recognizes no structure (000, Nov 20). Where the implementation of a channel does not agree with the
  ideal form, the encoding adapts, for example with a polyline (Nov 20). **Open:** how do the encodings relate to
  `IGraphicsTarget`?
- **kanava** · *channel* — An output for which an encoding writes. The epic names the rendering channels Skia, PDF and
  SVG (000). A channel offers abstractions: for example the dash pattern of PDF and SVG, the 1D path effect of Skia
  and the markers of SVG (011, Nov 19).
- **kanavamuunnos** (proposed name) · *channel conversion* — A calculation of an encoding that converts a derivation
  into a form of its channel. Its results are not kept (spec 02, Nov 20). Examples: a dash table, the expansion, a
  polyline of a curve, and an explicit union for a channel that needs one.
- **laajennus** · *expansion* (work: `StrokedShape.ExpandDashes`, `RepeatedShapes.Expand`) — A conversion into single
  parts: a run into dashes, and a repetition into placements (011).
- **katkotaulukko** · *dash table* — A conversion of the dashes of a line (000, Nov 20).
- **perustapaus** · *basic case* — A case that a channel solves with an abstraction of its own (Nov 19). In a basic
  case, the encoding converts the structure directly into a structure of the channel (Nov 20).
- **`IGraphicsTarget`** · piirtokohde · *graphics target, drawing target* — The interface on which the symbols draw
  now. Its implementations are classes such as `Skia_GraphicsTarget` and `Pdf_GraphicsTarget`. The comments of
  upstream say *graphics target*, and less often *drawing target*. The documents of the fork say *drawing target*. The
  application that ships uses the graphics targets of `Map_SkiaStd` and `Map_PDF` (Nov 18). **Open:** its place when
  pass 2 and the encodings draw from the intermediate layer.
- **piirrosobjekti** · *drawn object* — An object that an encoding writes. Each drawn object refers to its symbol
  separation, and through it to its `Symbol`, `SymColor` and G (spec 02, Nov 20).
- **viittaus** · *reference* — What the map side lets a drawn object refer to is not duplicated in the drawn objects.
  They use it through references (Nov 20).
- **itsensä kannatteleva vektoritiedosto** · *self-supporting vector file* — An SVG or a PDF file that carries the
  content of `Map`, so that Purple Pen can one day read it from the file (spec 02, Nov 20). The opening is not in the
  scope of the epic.
- **korostus** · *highlight* (`Symbol.DrawHighlight`, `CourseObj.DrawHighlight`) — It is drawn directly on the graphics
  target. **Open:** does it stay a direct drawing (000)?
- **litistys** · *transparency flattening* — The conversion of the transparencies into opaque vector objects
  (spec 08). It is not in the scope of the epic (000).

## Words that are not names · Sanat, jotka eivät ole nimiä

- **malli** · *model* — Too ambiguous to name a concept in the specification (@Pauli, Nov 20). Until Nov 20, @Pauli
  used it for all that the application reads from an opened OCAD or Mapper file, held and structured. That use was not
  exact, and it meant most nearly `Map` (@Pauli, Nov 20). In the proposals of the session of Nov 20 that became the
  decisions of Nov 20, it meant the instances of pass 1.

  The decisions of Nov 20 say "the model" (in Finnish *malli* and *karttamalli*). Proposed readings by the structure of
  the passes (not decided):
  - "the map model keeps the ideal forms" (The ideal forms): `Map` keeps the curves of the paths, and the derivations
    of the symbol separations keep the dashes, the runs and the paths of the repetitions as Bezier pieces.
  - "the implementation of the channel does not agree with the model" (The ideal forms): with the ideal form of a
    derivation.
  - "the model keeps it, and the encoding reads the structure from the model" (The model keeps the information of the
    encodings; the rule of 000): the map side, that is `Map` and the intermediate layer.
  - "the model keeps the structure of a line" (The structure of a line): `Map` keeps the path of the `LineSymbol`, the
    `DashInfo`, the `DoubleLineInfo` and the metric. The symbol separation keeps the parts, the fitted patterns, the
    end dashes, the exceptions of fix 020 and the quarters.
  - "the measure of the model" (The composed dashed lines, 011): the measure of the line, which the symbol separation
    keeps from the metric of the `Map` and the quarters of the path.
  - "an SVG or a PDF file carries the model" and "Purple Pen reads the model from the file" (Self-supporting vector
    files): the content of `Map`.
  - "the parts of the model" (Self-supporting vector files): the symbol separations, through them `Symbol`, `SymColor`
    and G, and the shared definitions.
  - "the epic includes a model that makes it possible" (Self-supporting vector files): the structure of the passes
    (spec 02).
  - "the model offers the known conversions" (The conversions of the model): the map side keeps the derivations, and
    the encodings make the channel conversions. The structure of the passes changes this decision (Progress.md,
    Nov 20).
- **karttamalli** · *map model* — A vague reference (@Pauli, Nov 20). It can mean three names of the code:
  - the folder `src/MapModel`: the map model and the graphics targets, in many projects, for example `Graphics2D`,
    `Map_SkiaStd` and `Map_PDF`;
  - the project `src/MapModel/MapModel/MapModel.csproj`: the classes that read, hold, write and draw the maps, for
    example `Map`, `SymColor`, `SymDef`, `Symbol`, `Glyph` and `SymPath`. The palette-layer work adds the instances to
    it (005);
  - the namespace `PurplePen.MapModel`: the classes of that project, and also classes of `Map_SkiaStd`, `Map_PDF`,
    `Map_GDIPlus`, `Map_WPF` and `Map-iOS`.
- **karttakoordinaatit, maailmankoordinaatit** · *map coordinates, world coordinates* — Ambiguous names that the
  comments of upstream give to the paper coordinates. They are not used (@Pauli, Nov 20).

## Words with more than one meaning · Monimerkityksiset sanat

- *symboli* · *symbol*: `Symbol`, an object of a map, in the code of upstream and in the Finnish tickets, and `SymDef`
  in the specification. In the same way, *viivasymboli* · *line symbol* is `LineSymbol` in the code of upstream, and
  `LineSymDef` in 011.
- *vaihe*: a pass (*vaihe 1*, *vaihe 2*), the phase of a dash pattern (`Stroke.DashPhase`), and a step of a placement
  (`Placement.Steps`).
- *muunnos*: a transformation (M, L, E and the steps of a placement), and a conversion for a channel. The proposed
  name *kanavamuunnos* separates the second.
- *viiva*: a line (`LineSymDef` or `LineSymbol`), and a stroke (`Stroke`).
- *litistys*, *litistäminen* · *flattening*: the transparency flattening (spec 08), and the flattening of a path. The
  English word is the same for both.
- *toisto*, *toistuva*: the repetition of a placement along a path, and the repetitive geometry G of a lattice.
- *kuvio* · *pattern*: an area pattern (`PatternInfo`), a dash pattern (`Stroke.DashPattern`) and a fitted pattern.
- *täyttö* · *fill*: a filled shape (`FilledShape`), the fill of an area, the fill of a double line, and a pattern
  fill.
- *reuna* · *border*: the border of a double line, and the border of an area.
- *osa* · *part*: a part of a line between corner and dash points, a component of the structure of a line, a part of a
  glyph, and a part of a `LineComboSymDef`.
- *ajo* · *run*: a run of dashes, a text run (`TextShape`), and a run of the tests in GitHub Actions.
- *taitepiste*, *jana* · *vertex*, *segment*: of the path that the user draws, or of the flattened path.
- *kerros* · *layer*: a palette layer, a color of upstream ("colors (layers)"), a layer of upstream (`SymLayer`:
  `Normal`, `Image` and `Layout`), a layer of a channel, and the intermediate layer.
- *separaatio* · *separation*: the separation of an ink (spec 06), the palette separation (`PaletteSeparation`), and
  the symbol separation (`SymbolSeparation`).
- *palettikerros*, *palettimerkintä*, *ryhmä*: three words for close concepts.
- *step*: the step of a repetition (*askel*), and the steps of a placement (*vaiheet*).
- Two English words for one concept: *graphics target* (upstream) and *drawing target* (the documents of the fork).
- Two Finnish words for one concept: *peittävyys* and *opasiteetti* (α, 000 and 001), *täysi peitto* and *yhtenäinen
  geometria* (G = 1, 005), *paikka* and *kohta* (a position, 011), *perustapaus* and *perustilanne* (a basic case,
  011).

## Not in this glossary

The words of the process and of the tests: the collector (*kerääjä*, work: `Map.CollectLayerInstances`), the oracle
(*oraakkeli*), the geometry method (*geometriametodi*), the baseline, the full run (*täysi ajo*) and the coverage
comparison (*peiton vertailu*).

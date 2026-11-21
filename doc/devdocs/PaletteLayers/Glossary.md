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
  a symbol (spec 02), as the user interfaces of OCAD and Mapper do. The terms follow upstream: *symbolimäärittely* ·
  *symbol definition* is `SymDef` (@Pauli, Nov 20).
- **`Symbol`** · symboli · *symbol* — One object of a map, for example `PointSymbol`, `LineSymbol`, `AreaSymbol` or
  `TextSymbol`. Its geometry is in paper coordinates. A `PointSymbol` and a `RectLikeSymbol` have a location, a
  `LineLikeSymbol` has a path (`SymPath`), and an `AreaLikeSymbol` has a path with holes (`SymPathWithHoles`). The
  comments of upstream also call it an object, for example a line object or an area object. The Finnish tickets call
  it a symbol (005). The specification calls it a map object (spec 02). The terms follow upstream: *symboli* ·
  *symbol* is `Symbol` (@Pauli, Nov 20). @Pauli is used to the meaning of OCAD and Mapper, in which a symbol is a
  definition, so his *symboli* can mean a `SymDef` (Nov 20). A `Symbol` has no identifier, but a
  `SymColor` has `Name` and `OcadId`, and a `SymDef` has `Name` and `SymbolId`. **Open:** the identifier of a `Symbol`
  to which a drawn object of a file can refer (Nov 20, the parts of `Map` to which an encoding can refer).
- **paperikoordinaatit** · *paper coordinates* — The coordinates of a map, in millimeters on the printed map. The
  points are `PointF` values. The OCAD import divides the OCAD units of 0.01 mm by 100
  (`OcadImport.ToWorldDimensions`). @Pauli holds *paper coordinates* to be the established name. He finds *physical
  coordinates* a good name, but a fourth name is probably not worth it (Nov 20). The comments of upstream also say
  *map coordinates* and *world coordinates* (see "Words that are not names"). The real-world coordinates
  (`RealWorldCoords`) are a different concept: the georeferencing of the map.
- **ideaalimuoto** · *ideal form* — The form in which `Map` keeps a structure, apart from the form of a channel: the
  information that an encoding needs. `Map` keeps the curves of the paths as Bezier curves. The derivations are not
  ideal forms (@Pauli, Nov 20).

### Palette and color

- **`SymColor`** · palettiväri · *palette color* — A color of the normal layer (`SymLayer.Normal`). A palette color
  has a color or ink components, an opacity, a blend mode, a knockout and a priority (spec 02, 06; 001, 004). The
  term is *palettiväri* · *palette color* (@Pauli, Nov 20). Upstream calls it a color, the specification a palette
  entry, and the tickets *palettimerkintä*. The comments of upstream also call the colors layers ("colors (layers)",
  `RenderOptions`).
- **palettipino, palettijärjestys, prioriteetti** · *palette stack, palette order, priority* — The transparency
  groups of the palette colors in palette order, and the order in which pass 2 composites them (spec 01, 02, 05;
  @Pauli, Nov 21). The colors of a map are
  `Map.AllColors`, and upstream draws them from the lowest to the highest (005). One palette stack is for all the maps,
  the courses included (000).
- **väri C** · *color C* — A spatially constant color, independent of color space (spec 01). A `SymColor` of
  upstream keeps a CMYK color (`CmykColor`).
- **`SymColor.Opacity`** (work) · peittävyys α · *opacity α* — A spatially constant opacity (spec 01; 004). The
  terms are *peittävyys* α · *opacity* α (@Pauli, Nov 21). The tickets say *peittävyys* 17 times, among them the title
  of 004, and *opasiteetti* 2 times (000, 001). The Finnish translations of Inkscape and of GIMP 3.0 call the opacity
  *peittävyys* and *peittokyky*. The coverage (*peitto*) is a different concept, which spec 01 keeps apart from α.
- **`SymColor.BlendMode`** (work) · sekoitustila B · *blend mode B* — The compositing operation (`ColorBlendMode`)
  (spec 01; 004). For an OCAD map, B = Darken is the larger share of each ink, and not the RGB minimum
  ([oracles/ocad-darken.md](oracles/ocad-darken.md)).
- **`Ink`** (work) · muste, prosessimuste, spottimuste · *ink, process ink, spot ink* — A colorant identity, apart
  from the palette color. It has a name (`Name`), a CMYK appearance (`Appearance`), and a screen frequency and angle
  where they are defined (`Screen`, `InkScreen`) (spec 06; 001). The process inks C, M, Y and K always exist
  (`Ink.ProcessInks`). A spot ink can be a palette color by itself, at full tint (`SymColor.SpotInk`) (001).
- **`InkComponent`** (work) · mustekomponentti, kerroin · *ink component, factor* — A palette color holds ink
  components (`SymColor.InkComponents`). Each has an ink and a factor from 0 to 1. A missing component and the factor 0
  are different (001). The specification says *ink contribution*.
- **värierottelu** · *ink separation* — The coverage of one ink. A missing share leaves the separation as it is, 0 %
  erases it, and a positive share sets the coverage (spec 06). The term is *värierottelu*, the established term of
  printing (@Pauli, Nov 20). The epic says *separaatiot* for the separations of the rendering modes. Spec 02 and spec 08
  also say *color separation* and *palette separation* for the coverage of one palette color (see
  `PaletteSeparation`).
- **`SymColor.OverPrint`** · päällepainatus · *overprint* — The overprint flag of an OCAD process color keeps the inks
  below (spec 06). The option `RenderOptions.blendOverprintedColors` draws the colors that have it with Darken. The
  inks simulate the overprinting in a rendering mode (000).
- **knockout** · *knockout* — The flag of Mapper. Its intent is the inverse of the overprint flag: it removes the inks
  below (spec 06). The Mapper import sets `SymColor.OverPrint` only for a color that has a spot definition and whose
  knockout is off (001).

### Coverage prototypes

- **peittoprototyyppi** · *coverage prototype* (work: `GeometryPrototype`) — The coverage G in its origin-bound form,
  before M places it (spec 02). On the map side, G is a part of a `SymDef`: the full coverage, one hatching (an index
  in `AreaSymDef.hatchings`) or one pattern (an index in `AreaSymDef.patterns`) (Nov 20). In an `AreaComboSymDef`, the
  component definition is part of G. Upstream has no class for G. The specification calls it a geometry prototype. The
  term is *peittoprototyyppi* · *coverage prototype* (@Pauli, Nov 21). **Open:** are the shared structures that are
  not periodic, for example the shared shapes of a glyph, prototypes or a concept of their own?
- **jaksollinen peitto G** · *periodic coverage G* — A binary, spatially repetitive, origin-bound selector (spec 01):
  for each point of the plane, G tells if the point is in the coverage. In mathematics, G is the indicator function of
  a set of the plane that the translations of a lattice do not change. The hatchings and the area patterns are
  periodic coverages (009, a draft). Spec 01 calls it repetitive geometry, and spec 02 periodic geometry. The term is
  *jaksollinen peitto* · *periodic coverage* (@Pauli, Nov 21). It replaces *jaksollinen geometria* · *periodic
  geometry* (@Pauli, Nov 20), as @Pauli found *geometria* too generic (Nov 21). G selects a set of points, which is a
  coverage. In compositing, the alpha has two parts, the coverage and the opacity. The specification keeps them apart:
  the binary coverage (G and the clipping objects) and α (*peittävyys* · *opacity*).
- **täysi peitto, G = 1** · *full coverage* (work: `GeometryPrototype.FullCoverage`) — The whole plane: G(x, y) = 1 at
  each point (spec 01). A constant function does not change in a translation, so it is periodic with each lattice. The
  full coverage is the trivial case of periodic coverage, and not a kind of its own. No M changes it. The G of points,
  lines, texts and area fills is the full coverage. The dash patterns and the glyphs along a line are strokes and
  placements, not lattices (Nov 19, Nov 20). The term is *täysi peitto* · *full coverage* (@Pauli, Nov 21). The
  tickets also say *yhtenäinen geometria*.

### Glyphs

- **`Glyph`** · glyyfi · *glyph* — "A glyph is used to define a point symdef or part of a linesymdef or area symdef"
  (the comment of `Glyph`). Its parts are lines, areas, circles and filled circles (005). The term of a part is
  *glyyfin osa* · *glyph part* (`Glyph.GlyphPart`) (@Pauli, Nov 20).
- **`extraTransform`** · glyyfin lisämuunnos · *extra transformation of a glyph* — It transforms the points of a glyph
  but not its line widths, for example for the scale at corners and for the glyphs that decrease. So it is not an
  affine placement of a stroked shape (011).

### Lines

- **`LineSymDef`, `LineComboSymDef`** · viivan symbolimäärittely · *line symbol definition* — A `LineSymDef` has the
  dashes (`DashInfo`), the double line (`DoubleLineInfo`), the shortening and the pointy ends (`ShortenInfo`) and the
  glyphs (`GlyphInfo`). A `LineComboSymDef` gathers other line symbol definitions, its component definitions (011).
  The term is *yhdistelmän osamäärittely* · *component definition* (@Pauli, Nov 20). The comments of upstream call a
  `LineSymDef` a *linesymdef*. Ticket 011 calls these line symbols, but *viivasymboli* is
  `LineSymbol` (@Pauli, Nov 20).
- **`LineSymbol`** · viivasymboli · *line symbol, line object* — One line object of a map. Its path is
  `LineLikeSymbol.Path`, a `SymPath`.
- **viivan mitta** · *measure of the line* — The metric that the source means to be used, which `Map` keeps in the ideal
  form (`Map.UseEuclideanMetric`). `Map` offers the means to convert it to suit a channel, and to keep the derivations
  (@Pauli, Nov 20). The measure of fix 021 (work: `OcadCurveMeasure`, `SymPath.GetOcadCurveMeasure`) gives a straight
  segment its length, and a quarter of a curve its chord (021).
- **viivan rakenne** · *structure of a line* — The logical path, the parts as its pieces and offsets, the measure of
  the line, the fitted pattern of a part, the end dashes and the exceptions of fix 020 (Nov 20). `Map` keeps the
  information that is necessary, that is the ideal forms: the path of the `LineSymbol`, the `DashInfo`, the
  `DoubleLineInfo` and the metric. The segments, the fitted dash patterns, the merged end dashes and the exceptions
  of fix 020 are derivations, which `Map` can keep for the channels where the implementation finds it sensible (@Pauli,
  Nov 20).
- **looginen polku** · *logical path* — The path of a line in its ideal form. The parts of the line are its pieces and
  offsets (Nov 20). **Upstream:** a line object has one path (`LineLikeSymbol.Path`), and its definition draws all the
  parts of the line from it (011). **Open:** is the logical path `LineLikeSymbol.Path`?
- **keskiviiva** · *middle of the line* — The base path of the borders of a double line (Nov 20). The English
  documents have no single term for it.
- **`DoubleLineInfo`** · kaksoisviiva, kaksoisviivan reuna, kaksoisviivan täyttö · *double line, double line border,
  double line fill* — A double line has a fill and two borders, the left and the right border (005, 011). The terms
  are *kaksoisviivan täyttö* · *double line fill* and *kaksoisviivan reuna* · *double line border* (@Pauli, Nov 20).
  Upstream calls the borders the left and the right double line, with their colors, widths and dashes
  (`doubleLeftColor`, `doubleRightColor`), and calls the band between them the fill (`doubleFillColor`). That name
  would make *kaksoisviiva* mean both the whole line and each of its sides. `doubleThick` is the distance between the
  double lines and the thickness of the fill.
- **`DashInfo`** · katko, väli, toissijainen väli · *dash, gap, secondary gap* — The dashes of a `LineSymDef`: the
  lengths of the dashes and of the gaps (`dashLength`, `gapLength`), and the secondary gaps (`secondaryMiddleGaps`,
  `secondaryEndGaps`) (011). Upstream calls the dashes between the first and the last dash the main dashes, and has
  the lengths of the first and the last dash (`firstDashLength`, `lastDashLength`).
- **`ShortenInfo`** · lyhennys · *shortening* — The shortening of the beginning and of the end of a line
  (`shortenBeginning`, `shortenEnd`). It is measured with the metric of the map (011). `ShortenInfo` also has the
  pointy ends (`pointyEnds`).
- **`SpacingMethod.OCAD`** · OCADin välistys · *OCAD spacing* — The method that fits the dashes and the glyphs of a
  segment as OCAD does (017, 021). The other spacing methods of upstream are `OpenMapperDashes` and
  `OpenMapperMidSymbols`.
- **`SymPath.DistanceMetric`** · metriikka · *metric* — The OCAD metric is the larger coordinate difference plus half
  the smaller one (`SymPath.BizzarroDistance`): a norm whose unit circle is an octagon (011). A map uses the OCAD
  metric when its view parameter `p` is 1. Mapper maps use the euclidean metric (011, 016). The metric of a map is
  `Map.MapDistanceMetric`, and `Map.UseEuclideanMetric` selects it. The OCAD metric is best effort (Nov 19).
- **`PointKind.Corner`, `PointKind.Dash`** · kulmapiste, katkopiste · *corner point, dash point* — The points of a path
  that have the OCAD flags y & 1 and y & 8. They divide a line into segments (011).
- **`SymPath.GetSubpaths`** · osapolku · *subpath* — A part of a path that the start and stop flags of its points
  delimit, for example for a border of a double line (`SymPath.DOUBLE_LEFT_STARTSTOPFLAG`). The term is *osapolku* ·
  *subpath* (@Pauli, Nov 20).
- **Bezier-käyrä** · *Bezier curve* (`PointKind.BezierControl`) — A curve of a path, with its control points.

### Patterns

- **`AreaSymDef.FillColor`** · alueen täyttö · *area fill* — The solid fill of an area, in one color ("Solid fill
  information", the comment of upstream). The term is *alueen täyttö* · *area fill* (@Pauli, Nov 20).
- **`AreaSymDef.BorderSymdef`** · alueen reuna · *area border* — The line symbol definition with which an area is
  bordered ("the border symbol to use", the comment of upstream). `AreaSymDef.Draw` draws it along the main path and
  along each hole. The term is *alueen reuna* · *area border* (@Pauli, Nov 20).
- **kuviotäyttö** · *pattern fill, structured fill* (`HatchInfo`, `PatternInfo`) — The hatchings and the area patterns
  (spec 03; 009, a draft).
- **`HatchInfo`** · viivoitus · *hatching* — It has a color, a width, a spacing, an offset, an angle and a rotate mode
  (`hatchColor`, `hatchWidth`, `hatchSpacing`, `hatchOffset`, `hatchAngle`, `hatchRotateMode`). Upstream calls its
  lines hatch lines. **Open:** is a hatching a prototype of its own, or a pattern of line elements (009, question 3)?
- **`PatternInfo`** · aluekuvio · *area pattern* — It has a pattern element (`patternGlyph`), the size of the pattern
  element (`patternWidth`, `patternHeight`), an offset of the alternate rows by 1/2 (`offsetRows`), an angle, an
  offset, a rotate mode, a fill mode and the irregularity (009). Upstream calls it a pattern. The term is *aluekuvio* ·
  *area pattern* (@Pauli, Nov 20).
- **hila, hilavektori** · *lattice, lattice vector* — The arrangement in which a pattern repeats its elements
  (spec 03). **Upstream:** no name. The size of the pattern element and the offset of the rows give the lattice.
- **`PatternInfo.patternGlyph`** · elementti · *element* — The geometry that the lattice repeats. The center of an
  element is its origin, the point that the lattice places at a lattice vector (spec 03). Upstream calls it the pattern
  element, and the structure element in the comments of the fill modes.
- **hilan affiininen muunnos L, elementin affiininen muunnos E** · *affine lattice transformation L, affine element
  transformation E* — The point e of the element at the lattice vector v is at L(v) + E(e) (spec 03).
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

- **`TemplateInfo`** · pohjakartta · *template* (`LoadedTemplate`) — It is not a palette color (005). `Map.Draw` draws
  the templates under and over the palette colors now. An OCAD template is a `Map` of its own (000).
- **`SymLayer.Image`** · kuvataso · *image layer* — The layer below all the normal layers. The layers of `SymLayer`
  are layers in the sense of image editing, and their names are *kuvataso*, *tavallinen taso* and *asettelutaso*
  (@Pauli, Nov 20). Its objects are image
  objects, with a special symbol definition (`ImageSymDef`). It is not a palette color (005). Upstream draws the
  image layer and the layout layer with the same loop as the colors (`DrawColor`), through two pseudo-colors without
  a color value (`Map.ImageColor`, `Map.LayoutColor`). They are not palette colors.
- **`SymLayer.Layout`** · asettelutaso · *layout layer* — The layer above all the normal layers. Its objects are
  layout objects, with a special symbol definition (`ImageSymDef`). It is not a palette color (005).
- **`SymLayer.Normal`** · tavallinen taso · *normal layer* — The layer between the image layer and the layout layer.
  It holds the transparency groups of the palette colors in palette order, as a group of layers holds layers in image
  editing (Nov 20, Nov 21).
- **ratakartta** · *course map* (`CourseLayout.RenderToMap`) — The map of the courses is a `Map` of its own, so the
  same pass 1 applies to it (005). One palette stack is for all the maps (000).

## The intermediate layer · Välikerros

`Map` makes the intermediate layer from the map alone, in pass 1. It is not source data: `Map` stays the source
(spec 02, Nov 20).

### The separations and the cutters

- **vaihe 1, ilmentäminen** · *pass 1, layer instantiation* (work: `Map.CreateLayerInstances`) — `Map` makes the
  palette separations and their palette cutters from the map alone, and no rectangle or resolution of a drawing
  (spec 02; Nov 18, Nov 20). The symbol separations get their derivations from the geometry methods of the symbol
  definitions (008; 011 to 013 in progress). *Vaihe* means a pass (@Pauli, Nov 20).
- **`SymbolSeparation`** (proposed name) · symbolierottelu · *symbol separation* — The part of one `Symbol` that
  belongs to one palette color and one coverage prototype G: the triple (`Symbol`, `SymColor`, G). It refers to these
  objects and copies nothing. It keeps its own derivations (spec 02, Nov 20).
- **`PaletteSeparation`** · palettierottelu · *palette separation* — The symbol separations of one palette color. It
  keeps its palette cutters as a derivation (spec 02, Nov 20). The name is @Pauli's, and the Finnish term is
  *palettierottelu* (@Pauli, Nov 20).
- **`PaletteCutter`** · palettileikkuri · *palette cutter* — The symbol separations of one palette separation that have
  the same configuration. It cuts the uniform color of the palette color to the coverage that G under M gives inside
  the union of its clipping objects. Each symbol separation is in one palette cutter, and each palette cutter is in one
  palette separation. The specification calls it a layer instance (spec 02, Nov 20). The name is @Pauli's (Nov 20).
- **kokoonpano, samanlainen kokoonpano** · *configuration, equivalent configuration* — The palette color, G and the
  effective affine transformation M of a symbol separation. M comes from the `Symbol` and from G. The symbol
  separations with the same configuration are one palette cutter (spec 02, 03; Nov 20). **Open:** when are two
  configurations the same (009, question 1)?
- **kerrosilmentymä** · *layer instance* (work: `LayerInstance`) — The name of the specification for the palette
  cutter (spec 02). The class `LayerInstance` of the work has a palette color (`PaletteEntry`), a coverage prototype
  (`Prototype`), an affine transformation (`Transformation`) and kept clipping objects (`ClippingObjects`). For G = 1,
  there is one for each palette color (005).
- **`LayerInstance.Transformation`** (work) · affiininen muunnos M · *affine transformation M* — An invertible 2D
  affine transformation of G (spec 01).
- **peitto, unioni** · *coverage, union* — A binary coverage is a set of points of the plane. In mathematics, it is
  the indicator function of the set, which is 1 at the points of the set (spec 01, 05). G is a coverage, and so is the
  area that a clipping object covers. The coverage of a palette cutter is G under M inside the union of its clipping
  objects. The coverage of a palette separation is the union of the coverages of its palette cutters (@Pauli,
  Nov 21). The union can stay implicit in the rendering, but the instances cannot be left out (spec 05, Nov 18).

### The derivations

- **johdos** · *derivation* — A result that `Map` gives from the ideal form for the needs of the channels. `Map`
  offers the means to make derivations and to keep them, where the shared use of the channels or a cache of the
  results makes it sensible (@Pauli, Nov 20). The means are the known channel conversions of `Map`, and an encoding
  chooses the channel conversions that its channel can use (@Pauli, Nov 20). The Finnish term is @Pauli's (Nov 20),
  and the English term is a proposed name. A symbol separation keeps its own derivations. A derivation that depends on
  a definition alone belongs to the definition, for example the shared shapes of a glyph. A palette separation keeps
  its palette cutters. A derivation is calculated when it is necessary the first time, and it becomes invalid when the
  map changes (spec 02, Nov 20). Upstream keeps
  derivations in the same way, for example `SymPath.flatpoints`, `TextSymbol.wrappedText` and `Symbol.boundingBox`.
  A derivation is not an ideal form, but a practical adaptation of the ideal form to the needs of the channels. Its
  form depends on what the implementations that the channels choose need: for example, the dashes on a curve can be
  Bezier pieces, polylines or both (@Pauli, Nov 20). In the work, the shapes keep ellipses and arcs as ellipses and
  arcs, as the graphics targets make polylines of them each in its own way (005).
- **segmentti** · *segment* — The part of a line between corner and dash points. The dashes are fitted to each
  segment separately (011). Upstream calls it a segment ("Get length of each segment, deliniated by corner points",
  `ComputeDashDistances`). The term is *segmentti* · *segment* (@Pauli, Nov 20). **Open:** in "the parts as its
  pieces and offsets" (Nov 20), is a part a segment or a line component?
- **viivan osatekijä** · *line component* — One of the components that a `LineSymDef` draws from the path of a line:
  the main line, the secondary line, the double line fill, a double line border, the pointy ends or the glyphs along
  the line (011). The term is *viivan osatekijä* · *line component* (@Pauli, Nov 20).
- **pala** · *piece* — A piece of the logical path, from one place on the path to another (Nov 20). **Open:** its
  exact content.
- **siirtymä** · *offset* — The distance of a line component from the logical path, to the side (Nov 20).
- **siirretty polku, pohjapolku** · *offset path, base path* — The shape of a border of a double line, in its ideal
  form: the path of the middle of the line and the offset. Its dash pattern is measured along its base path. The rule
  of the corners of the offset belongs to the shape, and the join belongs to the stroke (Nov 20). The offset curve is a
  derivation.
- **neljännes, jänne** · *quarter, chord* — Each quarter of a Bezier curve has its chord as its length in the measure.
  Within the quarter, the dashes are in proportion to the true arc (021, Nov 19).
- **litistetty polku, polun litistäminen** · *flattened path, path flattening* (`SymPath.GetFlattenedPoints`) — The
  term is *polun litistäminen* · *path flattening* (@Pauli, Nov 20). The polyline
  that approximates the curves of a path. Upstream calls its points the flat points and keeps them in the `SymPath`
  (`flatpoints`). The ideal form keeps the curves. A derivation can keep a polyline, and an encoding can make one
  (Nov 20).
- **taitepiste, jana** · *vertex, edge* — A point of the flattened path, and the straight piece between two adjacent
  points of the flattened path (@Pauli, Nov 20). The English term of *jana* is *edge*, as *segment* is the segmentti
  (@Pauli, Nov 20). The English documents of Nov 19 and of fix 021 call an edge a segment. The points of the path
  itself have the kinds `Normal`, `BezierControl`, `Corner` and `Dash` (`PointKind`). The flat points also have
  interpolated points (`PointKind.Interpolated`). The decisions on the questions 6 and 7 of
  011 use the words in this meaning. @Pauli took a vertex to be a corner point that the user puts on the curve
  (Progress.md, Nov 19).
- **sovitettu katkokuvio** · *fitted dash pattern* (`ComputeDashDistances`) — The dash and the gap that the fitting
  gives to a segment (011). Part of the structure of a line (Nov 20). The term is *sovitettu katkokuvio* · *fitted dash
  pattern* (@Pauli, Nov 20).
- **päätykatko, yhdistetty katko** · *end dash, merged dash* — The end dashes of adjacent segments merge into one dash
  at a corner point or a dash point (011). Upstream has the lengths of the first and the last dash (`DashInfo`).
- **korjauksen 020 poikkeus** · *exception of fix 020* — A dash from which the rule of fix 020 leaves out the vertex
  (Nov 19). Part of the structure of a line (Nov 20).
- **katkojuoste** · *run of dashes* — The middle dashes of a segment as one periodic dashed line (011). The term is
  *katkojuoste* (@Pauli, Nov 20); the decisions and the tickets say *ajo* and *katkoajo*. The runs in the euclidean
  measure are the same for Skia, PDF and SVG, as their dash patterns measure the path euclidean (011,
  Nov 19). They are derivations: `Map` offers the means to make them and to keep them (@Pauli, Nov 20). This changes
  the decision of Nov 20 on the composed dashed lines, which says that an encoding makes the runs (011).
- **koostettu katkoviiva** · *composed dashed line* — The form of a line for a channel whose dash pattern measures the
  euclidean length: the runs and the single dashes (Nov 20).
- **kanavamuunnos** · *channel conversion* — A conversion of the ideal form into a form that a channel can use. `Map`
  offers the known channel conversions, and keeps their results as derivations where the shared use of the channels
  or a cache makes it sensible. An encoding chooses the channel conversions of `Map` that its channel can use, and
  makes the others itself, without keeping their results (@Pauli, Nov 20). **Open:** which channel conversions stay
  in the encodings.
- **katkotaulukko** · *dash table* — A known channel conversion of the dashes of a line (000, Nov 20). `Map` offers it
  (@Pauli, Nov 20).
- **laajennus** · *expansion* (work: `StrokedShape.ExpandDashes`, `RepeatedShapes.Expand`) — A known channel
  conversion into single parts: a run into dashes, and a repeated placement into single placements (011). `Map`
  offers it (@Pauli, Nov 20).

### The forms of the derivations

- **`ClippingObject`** (work) · leikkausobjekti · *clipping object* — It limits the extent over which G of its palette
  cutter applies. It gives no color or alpha (spec 01). It has a shape that is drawn (`Shape`), a fill or a
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
- **`Stroke`** (work) · kynä · *stroke* — The width (`Width`), the caps (`Caps`), the join (`Join`) and the miter
  limit (`MiterLimit`) of a stroked shape (005), and the dash pattern and its phase (`DashPattern`, `DashPhase`)
  (Nov 19). The Finnish term is *kynä*, as upstream calls the same values a pen (`IGraphicsTarget.CreatePen`) (@Pauli,
  Nov 20). A pen of upstream also has a color or a brush. A `Stroke` has no color, as the palette color gives it, and
  it has the dash pattern and its phase. The band that a pen covers along a path is *kynänjälki* (proposed name). The
  symbol
  definitions give the caps and the joins together as a line style (`LineStyle`: `Rounded`, `Mitered`, `Beveled`,
  `FlatRounded`). **Open:** the dash structure of a segment in the measure of the line (point 2 of 6 in the review of
  011).
- **`LineCapMode`, `LineJoinMode`, `Stroke.MiterLimit`** · pää, liitos, jiirausraja · *cap, join, miter limit* — 005.
- **`Stroke.DashPattern`, `Stroke.DashPhase`** (work) · katkokuvio, katkokuvion alkukohta · *dash pattern, dash
  phase* — The Finnish term of the phase is *katkokuvion alkukohta* (@Pauli, Nov 20). The lengths of the
  dashes and the gaps in turn, and the distance that the pattern has gone at the start of the shape, as
  `stroke-dasharray` and `stroke-dashoffset` of SVG (011). They are properties of `Stroke` (Nov 19). **Open:** after
  the decisions of Nov 20, the measure in which they are kept (point 2 of 6 in the review of 011).
- **`TextShape`** (work) · tekstijuoste · *text run* — A word, or a grapheme where the character spacing is more than
  0, at its own position (Nov 19). The term is *tekstijuoste* (@Pauli, Nov 20); the tickets say *tekstiajo*. It has
  the text, the font family, the size, the effects and the top left corner (005). A text run is a shape.
- **jaetut muodot** · *shared shapes* — Each part of a glyph is one shape, which each placement of the glyph refers to
  (Nov 18; 008). They depend on the definition alone, so they belong to the definition (spec 02, Nov 20). Stretched
  and diminishing glyphs get shapes of their own (Nov 19). **Upstream:** a glyph at a corner is scaled, unless
  `noScaleAtCorners` is set, and the glyphs of `GlyphLocation.SpacedDecrease` decrease in size and spacing.
- **`Placement`** (work) · sijoitus · *placement* — It gives one or more poses. Each reader asks a placement for its
  poses (Nov 20). It composes its steps into one matrix (`Matrix`).
- **yksittäinen sijoitus** · *single placement* — It keeps one pose: one point and an angle (Nov 20).
- **toistosijoitus** · *repeated placement* — A placement that keeps its path, its first position along the path, its
  step and its count. It calculates its poses from the path: the point of the path at each position along the path,
  and the direction of the path there, with the method of the drawing (Nov 20, Nov 21). The term is *toistosijoitus* ·
  *repeated placement*, and the decisions say *toisto* · *repetition* (@Pauli, Nov 20). Only an encoding that gives a
  repeated placement to an abstraction of its channel reads the path and the step. On a Bezier curve, the glyphs of the
  OCAD spacing repeat by quarters (Nov 20). Its earlier form is a kind of clipping object (work: `RepeatedShapes`).
  **Upstream:** the glyphs that `GlyphLocation.Spaced` places along a line at the distances given.
- **asema** · *pose* — A point and an angle (x, y, θ) (Nov 20). In robotics and in computer vision, a pose is the
  location and the orientation of a body. In the plane, it is a rotation and a translation, that is a rigid motion (the
  group SE(2)), so the pose of a placement is its innermost step. In technical Finnish, *asema* also covers an angle
  (*kulma-asema*), and in astronomy the position angle (*asemakulma*) gives the orientation of an object on the sky.
  Robotics separates *asema* (location) and *asento* (orientation). The term is *asema* · *pose* (@Pauli, Nov 21). It
  replaces *paikka* · *position* of the decision of Nov 20. In the theory, a position is the point alone.
- **kohta** · *position along the path* — A distance from the start of a path, in the measure of the line: the
  arc-length parameter s of the path. The pose at a position along the path is the point of the path there and the
  direction of its tangent. The first position of a repeated placement is a position along the path, and its step is
  the distance between two adjacent ones (@Pauli, Nov 21). The channels take a distance in the same way:
  `SkPathMeasure.getPosTan` of Skia gives the point and the tangent at a distance, and `getPointAtLength` of SVG gives
  the point. The decision of Nov 19 on the glyphs along the lines says *kohta*, and the decision of Nov 20 on the
  placements says *paikka* for it.
- **`Placement.Steps`** (work) · sijoituksen sisäkkäiset affiiniset muunnokset · *steps of a placement* — The Finnish
  term is *sijoituksen sisäkkäiset affiiniset muunnokset*, and not *vaiheet* (@Pauli, Nov 20). The affine
  transformations in the
  order in which the symbols set them (005). A placement keeps its sequence of steps, and the pose is the innermost
  step (Nov 20). Skia composes nested transformations with its own rounding. With the same steps in the same order,
  the coverage is the same to the pixel (005).
- **askel, määrä** · *step, count* (work: `RepeatedShapes.Step`, `RepeatedShapes.Count`) — The distance along the path
  between two adjacent positions along the path of a repeated placement, and the number of its poses (Nov 19, Nov 20).
- **kuvioruutu** · *pattern tile* — One cell of an area pattern, which is composed from single elements (Nov 20). It
  depends on the definition alone. The term is *kuvioruutu* · *pattern tile* (@Pauli, Nov 20).
- **valitut elementit, epäsäännöllisen kuvion sijoitukset** · *selected elements, placements of an irregular pattern* —
  The elements that a fill mode selects, and the placements of the elements of an irregular pattern, are derivations of
  a symbol separation (spec 03, Nov 20).

## The rendering side · Renderöinnin puoli

The rendering side reads the intermediate layer. It keeps none of the results of the channel conversions that the
encodings make themselves (spec 02, Nov 20).

- **vaihe 2, koostaminen** · *pass 2, layer composition* — *Vaihe* means a pass (@Pauli, Nov 20). It takes the
  palette separations in palette order. For each palette separation, an encoding writes the transparency group of its
  palette color, to which C, α and B apply once. In the group, the encoding writes each palette cutter with one fill
  and one clip, or each symbol separation separately. Pass 2 does not call the drawing code of the symbols (`Draw`)
  (spec 02, 05; Nov 20, Nov 21). The specification calls the operation compositing.
- **palettivärin läpinäkyvyysryhmä** · *transparency group of a palette color* — The group that pass 2 composites
  for one palette color. Its content is the color C on the coverage of the palette separation. The group is
  composited once onto the backdrop, with the opacity α and the blend mode B of the palette color ("One compositing
  layer per palette entry", spec 05). So the overlaps of one palette color do not get α twice. One transparency group
  is for each palette color. The palette color is its definition, and the palette separation is its content. In the
  single-instance model of spec 01, a palette layer also has G and M. Spec 02 gives them to the layer instances, which
  are the palette cutters. The term is *palettivärin läpinäkyvyysryhmä* · *transparency group of a palette color*
  (@Pauli, Nov 21). It replaces *palettitaso* · *palette layer* (@Pauli, Nov 20).

  The theory: the transparency model of PDF (ISO 32000) calls this unit a transparency group. The objects of the group
  are composited into one result, and the result is composited onto the backdrop with the opacity and the blend mode
  of the group. The model of W3C (Compositing and Blending) calls it a group, and image editing and `saveLayer` of
  Skia call it a layer. A group is composited in a blending color space, which can be RGB, or CMYK with spot inks. The
  separable blend modes, for example Darken and Multiply, apply to each color component. So in this model the group
  is the same in all the rendering modes, and only the blending space changes. In a CMYK group, Darken takes the
  larger share of each ink, as the PDF export of OCAD does ([oracles/ocad-darken.md](oracles/ocad-darken.md)). The
  transparency flattening (spec 08) flattens these groups. The specification calls the group a palette layer, and the
  tickets call it *palettikerros*.
- **piirtotila, väriasetus** · *rendering mode, color configuration* — A rendering mode is a color configuration of
  pass 2: the normal view, the overprinting that the inks simulate, or the separations (000; spec 06, 07). All the
  rendering modes use the same intermediate layer (Progress.md, "Next"). **Upstream:** the render options
  (`RenderOptions`). For example, `blendOverprintedColors` blends the colors that have the overprint flag with Darken,
  and `colorBeginDrawExclusive` and `colorEndDrawInclusive` draw only some of the colors.
- **koodaus, koodaaja** · *encoding, encoder* — It writes the drawing for one channel. It chooses the channel
  conversions of `Map` that its channel can use, and makes the others itself. When an encoding needs information, the
  map side keeps it, and
  the encoding recognizes no structure (000, Nov 20). Where the implementation of a channel does not agree with the
  ideal form, the encoding adapts, for example with a polyline (Nov 20). **Open:** how do the encodings relate to
  `IGraphicsTarget`?
- **kanava** · *channel* — An output for which an encoding writes. The epic names the rendering channels Skia, PDF and
  SVG (000). A channel offers abstractions: for example the dash pattern of PDF and SVG, the 1D path effect of Skia
  and the markers of SVG (011, Nov 19).
- **perustapaus** · *base case* — A case that a channel solves with an abstraction of its own (Nov 19). In a base
  case, the encoding converts the structure directly into a structure of the channel, and otherwise it splits the
  structure (Nov 20). The term is *perustapaus* · *base case* (@Pauli, Nov 21). In computing, a base case is solved
  directly, and the other cases are split until only base cases remain. The splitting of the encodings ends in single
  dashes and placements, which each channel draws directly. Here the base case depends on the channel, as in the
  legalization of a compiler: an operation that the target supports directly is legal, and the others are expanded
  (LLVM: *Legal*, *Expand*). The expansion (*laajennus*) is that splitting. A case is a class of the structures that
  one branch handles. @Pauli said *perustilanne* on Nov 19, and the English decisions say *basic case*.
- **`IGraphicsTarget`** · piirtokohde · *graphics target* — The interface on which the symbols draw now. Its
  implementations are classes such as `Skia_GraphicsTarget` and `Pdf_GraphicsTarget`. The terms are *piirtokohde* ·
  *graphics target* (@Pauli, Nov 21). The code of upstream says *graphics target* 44 times, in 16 files. It says
  *drawing target* only in "the graphics drawing target", in two comments of `IGraphicsTarget` that its five
  implementations repeat. Upstream chose *graphics target* as the name first, and then added *drawing* in the comments
  to make it clear (@Pauli, Nov 21). The documents of the palette-layer work and the introduction say *drawing
  target*, the documents of the SVG work say *graphics target*, and the Finnish tickets say *piirtokohde*. The
  application that ships uses the graphics targets of `Map_SkiaStd` and `Map_PDF` (Nov 18). **Open:** its place when
  pass 2 and the encodings draw from the intermediate layer.
- **piirrosobjekti** · *drawn object* — An object that an encoding writes. Each drawn object refers to its symbol
  separation, and through it to its `Symbol`, `SymColor` and G (spec 02, Nov 20). In a file, the drawn objects keep a
  link back to the ideal forms that is as direct as possible, also through the hierarchy of the file, so that the SVG
  graphics can be a part of a web user interface (@Pauli, Nov 20).
- **viittaus** · *reference* — What the map side lets a drawn object refer to is not duplicated in the drawn objects.
  They use it through references (Nov 20). A drawn object links directly to the parts of the content of `Map`. It can
  take the geometry that it draws by reference from the shared parts of the intermediate layer. The link and the reuse
  are different references, and they do not limit each other (@Pauli, Nov 20). In SVG, `<use href>` and
  `fill="url(#…)"` reuse, and the hierarchy of the groups or an identifier of the drawn object links. In PDF, a form
  XObject reuses, and marked content links.
- **itsensä kannatteleva vektoritiedosto** · *self-supporting vector file* — An SVG or a PDF file that carries the
  content of `Map`, so that Purple Pen can one day read it from the file (spec 02; @Pauli, Nov 20). The opening is not
  in the scope of the epic.
- **korostus** · *highlight* (`Symbol.DrawHighlight`, `CourseObj.DrawHighlight`) — It is drawn directly on the graphics
  target. **Open:** does it stay a direct drawing (000)?
- **läpinäkyvyyden litistys** · *transparency flattening* — The conversion of the transparencies into opaque vector
  objects (spec 08). It is not in the scope of the epic (000). The term is *läpinäkyvyyden litistys* (@Pauli, Nov 20).

## Words that are not names · Sanat, jotka eivät ole nimiä

- **malli** · *model* — Too ambiguous to name a concept in the specification (@Pauli, Nov 20). Until Nov 20, @Pauli
  used it for all that the application reads from an opened OCAD or Mapper file, held and structured. That use was not
  exact, and it meant most nearly `Map` (@Pauli, Nov 20). In the proposals of the session of Nov 20 that became the
  decisions of Nov 20, it meant the instances of pass 1.

  The decisions of Nov 20 say "the model" (in Finnish *malli* and *karttamalli*). @Pauli decided their readings by the
  structure of the passes on Nov 20:
  - "the map model keeps the ideal forms" (The ideal forms): `Map` keeps the ideal forms, that is the information that
    an encoding needs. The derivations are not ideal forms, but practical adaptations of them to the needs of the
    channels. They keep the dashes, the runs and the paths of the repeated placements on a curve as Bezier pieces or as
    polylines, as the implementations that the channels choose need, and possibly in both forms (@Pauli, Nov 20).
  - "the implementation of the channel does not agree with the model" (The ideal forms): with the ideal form of `Map`.
    When the decision was made, `Map` did not offer derivations yet. Now `Map` also offers the means to make and to
    keep derivations, where the shared use of the channels or a cache of the results makes it sensible (@Pauli,
    Nov 20).
  - "the model keeps it, and the encoding reads the structure from the model" (The model keeps the information of the
    encodings; the rule of 000): `Map` keeps the information that an encoding needs, and the intermediate layer gives
    access to it in forms that are more usable for the needs of the channels (@Pauli, Nov 20).
  - "the model keeps the structure of a line" (The structure of a line): `Map` keeps the information that is
    necessary, that is the ideal forms: the path of the `LineSymbol`, the `DashInfo`, the `DoubleLineInfo` and the
    metric. The segments, the fitted dash patterns, the merged end dashes and the exceptions of fix 020 are derivations,
    which `Map` can keep for the channels where the implementation finds it sensible (@Pauli, Nov 20).
  - "the measure of the model" (The composed dashed lines, 011): the metric that the source means to be used, which
    `Map` keeps in the ideal form (`Map.UseEuclideanMetric`). `Map` offers the means to convert the metric to suit a
    channel, for example into runs in the euclidean measure, and to keep the derivations (@Pauli, Nov 20).
  - "an SVG or a PDF file carries the model" and "Purple Pen reads the model from the file" (Self-supporting vector
    files): the content of `Map` (@Pauli, Nov 20). The drawn objects of the file keep a link back to the ideal forms
    that is as direct as possible, also through the hierarchy of the file, so that the SVG graphics can be a part of a
    web user interface (@Pauli, Nov 20).
  - "the parts of the model" (Self-supporting vector files): the parts of the content of `Map`. A drawn object links
    directly to them. It can take the geometry that it draws by reference from the shared parts of the intermediate
    layer. The link and the reuse are different references, and they do not limit each other (@Pauli, Nov 20).
  - "the epic includes a model that makes it possible" (Self-supporting vector files): `Map` with the parts to which an
    encoding can refer. They make it possible to open a file as a map. The intermediate layer is also a part of the
    epic, but this decision does not concern it (@Pauli, Nov 20).
  - "the model offers the known conversions" (The conversions of the model): `Map`. It offers the known channel
    conversions as means to make derivations from its ideal forms, and keeps their results where the shared use of the
    channels or a cache makes it sensible. An encoding chooses a channel conversion that its channel can use. With
    channel conversions of its own, it makes only what no channel conversion of `Map` covers (@Pauli, Nov 20).
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
- **separaatio, erottelu** · *separation* — Not a term alone. *Separaatio* is *erottelu* in Finnish. The terms are
  *värierottelu* · *ink separation* and *palettierottelu* · *palette separation* (@Pauli, Nov 20).
- **kerros** — Not a term (@Pauli, Nov 20). The decision concerns the Finnish word.
- **ryhmä** — Not a term (@Pauli, Nov 20). The decision concerns the Finnish word. The specification uses *group*
  as a verb in pass 2 ("Group instances by palette entry"), and for the SVG element `<g>` that encodes the
  transparency group of a palette color (spec 05). The decision concerns *ryhmä* alone: *palettivärin
  läpinäkyvyysryhmä* is a term (@Pauli, Nov 21).
- **ajo** · *run* — Not a term in the sense of a sequence, where it translates *run* word for word. The terms are
  *katkojuoste* · *run of dashes* and *tekstijuoste* · *text run* (`TextShape`) (@Pauli, Nov 20). A run of the tests
  (*täysi ajo*) is a word of the process.
- **osa** · *part* — Not a term without a qualifier. The terms are *segmentti* · *segment*, *viivan osatekijä* · *line
  component*, *glyyfin osa* · *glyph part* (`Glyph.GlyphPart`), *yhdistelmän osamäärittely* · *component definition*
  and *osapolku* · *subpath* (`SymPath.GetSubpaths`) (@Pauli, Nov 20).
- **reuna** · *border* — Not a term without a qualifier. The terms are *kaksoisviivan reuna* · *double line border*
  and *alueen reuna* · *area border* (@Pauli, Nov 20).
- **täyttö** · *fill* — Not a term without a qualifier. The terms are *täytetty muoto* · *filled shape*
  (`FilledShape`), *täyttösääntö* · *fill rule*, *alueen täyttö* · *area fill*, *kaksoisviivan täyttö* · *double line
  fill*, *kuviotäyttö* · *pattern fill* and *täyttötila* · *fill mode* (@Pauli, Nov 20).
- **kuvio** · *pattern* — Not a term without a qualifier. The terms are *aluekuvio* · *area pattern* (`PatternInfo`),
  *katkokuvio* · *dash pattern* (`Stroke.DashPattern`), *sovitettu katkokuvio* · *fitted dash pattern*, *kuviotäyttö*
  · *pattern fill* and *kuvioruutu* · *pattern tile* (@Pauli, Nov 20).
- **toisto, toistuva** · *repetition, repetitive* — Not a term without a qualifier (@Pauli, Nov 20). The terms are
  *toistosijoitus* · *repeated placement* (@Pauli, Nov 20) and *jaksollinen peitto* · *periodic coverage* (@Pauli,
  Nov 21).
- **litistys, litistäminen** · *flattening* — Not a term without a qualifier. The terms are *läpinäkyvyyden litistys* ·
  *transparency flattening* (spec 08) and *polun litistäminen* · *path flattening* (@Pauli, Nov 20).
- **muunnos** · *transformation, conversion* — Too generic to be a term without a qualifier, as *malli* is (@Pauli,
  Nov 20). The terms are *affiininen muunnos* · *affine transformation*, for example M, L, E and the steps of a
  placement, and *kanavamuunnos* · *channel conversion* (@Pauli, Nov 20).

## Words with more than one meaning · Monimerkityksiset sanat

- *symboli* · *symbol*: `Symbol`, an object of a map, in the code of upstream and in the Finnish tickets, and `SymDef`
  in the specification. In the same way, *viivasymboli* · *line symbol* is `LineSymbol` in the code of upstream, and
  `LineSymDef` in 011. Decided: *symboli* · *symbol* is `Symbol`, and *viivasymboli* is `LineSymbol` (@Pauli,
  Nov 20).
- *vaihe*: a pass (*vaihe 1*, *vaihe 2*), the phase of a dash pattern (`Stroke.DashPhase`), and a step of a placement
  (`Placement.Steps`). Decided: *vaihe* is a pass. The phase is *katkokuvion alkukohta*, and the steps of a placement
  are *sijoituksen sisäkkäiset affiiniset muunnokset* (@Pauli, Nov 20).
- *viiva*: a line (`LineSymDef` or `LineSymbol`), and a stroke (`Stroke`). Decided: *viiva* is a line, and a stroke
  is *kynä* (@Pauli, Nov 20).
- *taitepiste*, *jana* · *vertex*, *segment*: of the path that the user draws, or of the flattened path. Decided: of
  the flattened path, and the English term of *jana* is *edge* (@Pauli, Nov 20).
- *step*: the step of a repeated placement (*askel*), and the steps of a placement (*sisäkkäiset affiiniset
  muunnokset*). The Finnish terms separate them (@Pauli, Nov 20).
- Two English words for one concept: *graphics target* (upstream) and *drawing target* (the documents of the
  palette-layer work). Decided: *graphics target* (@Pauli, Nov 21).
- Two Finnish words for one concept: *peittävyys* and *opasiteetti* (α, 000 and 001), *täysi peitto* and *yhtenäinen
  geometria* (G = 1, 005), *paikka* and *kohta* (a position, 011), *perustapaus* and *perustilanne* (a basic case,
  011). Decided: *peittävyys*, *täysi peitto* and *perustapaus* (@Pauli, Nov 21). *Paikka* and *kohta* name two
  concepts: *asema* · *pose* and *kohta* · *position along the path* (@Pauli, Nov 21).

## Not in this glossary

The words of the process and of the tests: the collector (*kerääjä*, work: `Map.CollectLayerInstances`), the oracle
(*oraakkeli*), the geometry method (*geometriametodi*), the baseline, the full run (*täysi ajo*) and the coverage
comparison (*peiton vertailu*).

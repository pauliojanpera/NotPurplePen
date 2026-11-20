# Palette-layer vector graphics architecture

> This is a working document converted from `Palette_Layer_Vector_Graphics_Specification_Revised.docx` on Nov 17, 2026.
> This specification was never the basis of NotPurplePen. Rather this is the reference for an architecture change of upstream's map model, to be later on integrated into NotPurplePen. Where NotPurplePen already knows how to take the model further, this specification does not bind as such, and the knowledge can be used to develop this specification further.
> By default, the branch feature/multiply-blending is the oracle of this work (@Pauli, Nov 17).
> That branch is an oracle of the appearance only, not a model of the implementation. The goal of this work is the architecture that this specification separates: pass 1 makes the instances, and pass 2 composites them. feature/multiply-blending gets its appearance in another way: it blends an ink as a layer around the drawing of the symbols. Where this specification leaves room to do the same, the room is not meant. The work does not read the code of that branch (@Pauli, Nov 18).
> OCAD and OpenOrienteering Mapper are the authorities on how their own maps render. For Mapper maps, Mapper is an oracle of the appearance: this architecture may organize the vector structure in its own way, but where Mapper implements the function, the result must look as Mapper draws it, except where we find a bug in Mapper (@Pauli, Nov 17).
> **The model keeps the information, and an encoding recognizes nothing** (@Pauli, Nov 20). When an encoding of the instances needs information, the model keeps it, and the encoding reads it from the model. No heuristic that recognizes a structure goes into an encoding. This applies to each encoding of the instances: for the rendering channels Skia, PDF and SVG, and the SVG encoding of section 04.

Design specification / Cartographic rendering

**A generalized vector color-separation model with symbol-driven instantiation of affine-transformed periodic geometry and palette-ordered compositing.**

The model is intended for application within a surrounding system design. It captures the essential rendering architecture of OCAD and OpenOrienteering Mapper (OOM) in SVG-compatible terms, with color-space independence and a separate optimization task for vector transparency flattening.

## Relationship to OCAD and OOM

Both applications organize rendering by a prioritized color table, rather than simply by the drawing order of complete objects. Symbol elements contribute geometry to their respective colors. OCAD supports color-specific blend modes for PDF export; OOM supports spot-color overprinting simulation. Both have object-specific pattern orientation facilities. [1–5]

| Cartographic concept              | Corresponding model                                            |
|-----------------------------------|----------------------------------------------------------------|
| Color table and color priority    | Ordered palette entries / compositing layers                   |
| Symbol elements                   | Graphical clipping objects                                     |
| Solid area fills                  | Constant repetitive geometry G = 1                             |
| Hatched and structured area fills | Periodic geometry G, or instantiated clipping objects          |
| Pattern offset and orientation    | Per-instance affine transformations of lattice and element     |
| Opacity / alpha and blend mode    | Independent palette-entry compositing properties               |
| Overprint (OCAD) / knockout (OOM) | Inverse terminology for printing behavior; separate from alpha |

## Scope of equivalence

The architecture distinguishes palette identity from ink identity. It supports direct color compositing and ink-based color separations. RGB and CMYK are color representations, not geometry properties. [1, 2, 4]

References: [\[1\] OCAD: Colors](https://www.ocad.com/wiki/ocad/en/index.php?title=Colors) • [\[2\] OCAD: Blend Mode](https://www.ocad.com/wiki/ocad/en/index.php?title=Blend_Mode) • [\[3\] OCAD: Area Symbols](https://www.ocad.com/wiki/ocad/en/index.php?title=Create_a_New_Area_Symbol) • [\[4\] OOM: Color Window](https://www.openorienteering.org/mapper-manual/pages/color_dock_widget.html) • [\[5\] OOM: Pattern Rotation](https://github.com/OpenOrienteering/mapper/wiki/Notes-on-.ocd-format-and-Mapper-compatibility)

## 01 / Logical model: Palette layers and geometry

The drawing comprises an ordered stack of palette layers. In the initial single-instance model a layer is described by the following properties. Ink and knockout semantics extend the palette definition independently of geometry (section 06).

| Property           | Meaning                                                         |
|--------------------|-----------------------------------------------------------------|
| C — Color          | Spatially constant color, independent of color space.           |
| α — Alpha          | Spatially constant opacity.                                     |
| B — Blend mode     | Compositing operation.                                          |
| G — Geometry       | Binary, spatially repetitive, origin-bound geometry selector.   |
| M — Transformation | Invertible 2D affine transformation of G; defaults to identity. |

G determines whether the layer participates at a given position. Color and alpha belong exclusively to the palette layer. Patterns are instances of repetitive geometry, not independent paint functions or additional rendering properties.

Full coverage is G(x, y) = 1, the constant case of repetitive geometry. An opaque default layer with full coverage is an ordinary instance of the architecture, not a special architectural component.

### Geometry transformations

Each instance can independently translate, rotate, scale, shear or reflect its geometry. Translation and rotation are currently required; the model admits all invertible 2D affine transformations. Lattice and element transformations are distinguished in section 03.

For a point p in the drawing coordinate system, the effective periodic selector is G(M⁻¹p). The prototype is origin-bound; its affine transformation is relative to the global coordinate system. The binary and repetitive character of G is preserved, and constant G = 1 is invariant.

Independently transformed repetitive geometries need not share a common repeating unit.

### Clipping objects

Graphical clipping objects determine the spatial extent over which the respective periodic geometry applies. They do not supply separate color or alpha. Multiple clipping objects associated with the same instance contribute their combined coverage by union.

![Figure 1](figures/figure-1.png)

Figure 1. A clipping object limits the extent of repetitive geometry. The effective coverage remains binary.

## 02 / Extended architecture: Symbol-driven layer instantiation

Object-specific pattern offsets and rotations require implicit instantiation of patterned layers, not duplication of the original geometry prototypes. The final layer setup is determined by symbols and individual map objects, including independently rotated patterned elements.

![Figure 2](figures/figure-2.png)

Figure 2. Symbol processing derives instances from prototypes; composition follows the palette order.

### Two-pass architecture

**Pass 1 — Layer instantiation.** Process symbols and individual map objects. Instantiate each required combination of palette entry, geometry prototype and effective lattice/element transformations; associate its clipping objects. Equivalent configurations may share an instance.

**Pass 2 — Layer composition.** Group instances by palette entry and composite the color separations in palette-priority order under the selected color configuration.

| Entity             | Properties / role                                                   |
|--------------------|---------------------------------------------------------------------|
| Palette entry      | Color, alpha, blend mode, knockout, ink contributions and priority. |
| Geometry prototype | Reusable origin-bound periodic geometry G.                          |
| Layer instance     | Palette and prototype references; effective affine transformations. |
| Clipping object    | Vector geometry associated with a layer instance.                   |

Geometry instantiation and color compositing occur at different architectural levels. The number of instances can grow independently of the number of palette entries. Ink identity is independent of both.

## 03 / Geometry instantiation: Structured fills and transformations

OCAD and OOM distinguish clipped structured fills from modes that select complete pattern elements according to their relationship with an area. The distinction is resolved during symbol-driven instantiation, not in the palette compositing model. [3, 5] The center of an element is its origin, the point that the lattice places at a lattice vector.

![Figure 3](figures/figure-3.png)

Figure 3. Boundary modes: clip elements, or select complete elements that are entirely inside, centered inside or partially inside the area.

Clipped structured fills retain implicit repetitive geometry through SVG pattern fills. Selectively placed complete elements are instantiated as individual clipping objects. Nonperiodic structured fills likewise become individual clipping objects. Composite representations of selected elements are deferred to later refinement.

### Independent lattice and element transformations

The repetition lattice and the repeated geometry within it can be transformed independently. OCAD distinguishes rotation of the arrangement from rotation of its elements. [3]

The element transformation E applies in the drawing's frame, to each element about its own lattice point: the point e of the element at lattice vector v lies at L(v) + E(e), where L is the lattice transformation. In OCAD, the structure angle of the symbol turns the lattice alone, and the angle of an area object turns the lattice and its elements together.

![Figure 4](figures/figure-4.png)

Figure 4. The lattice can rotate independently of its elements; both can also rotate together.

Effective transformations may vary by symbol, individual map object and its patterned elements. Instantiate additional layers for each previously unrepresented configuration, including individually rotated elements, rather than only per symbol. Equivalent configurations may reuse an instance; palette-level compositing is unchanged.

## 04 / SVG encoding: Reusable definitions and instances

The model can be expressed using existing SVG primitives. These are alternative encoding mechanisms for the same underlying logical model, rather than distinctions in the graphics architecture itself.

| Logical property          | SVG representation                                           |
|---------------------------|--------------------------------------------------------------|
| Uniform color layers      | Filled rectangles                                            |
| Binary geometry selectors | Clipping paths                                               |
| Repetitive geometry       | Origin-bound, single-color pattern fills with unpainted gaps |
| Affine transformations    | patternTransform                                             |
| Layer transparency        | Opacity                                                      |
| Layer blending            | Blend modes (mix-blend-mode)                                 |

SVG pattern references can inherit reusable definitions while overriding selected attributes. A transformed pattern instance therefore requires only a reference and its transformation, not a duplicate of the prototype geometry. [6–7]

```xml
<defs>
  <!-- Prototype: one definition of the repetitive geometry -->
  <pattern id="hatch" patternUnits="userSpaceOnUse"
           width="8" height="8">
    <rect width="2" height="8" fill="#008000"/>
  </pattern>

  <!-- Instances reference the prototype -->
  <pattern id="hatch-30" href="#hatch"
           patternTransform="rotate(30)"/>
  <pattern id="hatch-60" href="#hatch"
           patternTransform="rotate(60)"/>
  <pattern id="hatch-offset" href="#hatch"
           patternTransform="translate(2 4)"/>
</defs>
```

![Figure 5](figures/figure-5.png)

Figure 5. Pattern definitions share one prototype; instances independently determine phase and orientation.

A pattern is used as a paint server in SVG, but logically represents repetitive binary coverage. Pattern gaps are unpainted. A pattern fill inside a clipPath does not itself define repeated clipping coverage; the clipped paint element is used instead.

References: [\[6\] MDN: pattern](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Element/pattern) • [\[7\] MDN: patternTransform](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Attribute/patternTransform) • [\[8\] MDN: clipPath](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Element/clipPath)

## 05 / Compositing semantics: One compositing layer per palette entry

Distinct instances sharing a palette entry collectively define its binary coverage by union. In direct-color compositing, the palette color, alpha and blend mode apply once to that coverage, not independently to each instance. Ink-based operations are a separate concern (section 06).

![Figure 6](figures/figure-6.png)

Figure 6. The overlap of two instances of one palette entry must not receive that entry’s alpha twice. Solid coverage isolates the compositing principle.

For direct RGB compositing, SVG groups instances painted with the entry’s opaque color and applies opacity and blending to the group. The example assumes that the referenced clipping paths are defined. [9–10]

```xml
<g id="palette-green" opacity="0.6"
   style="mix-blend-mode:multiply">
  <rect width="100%" height="100%"
        fill="url(#hatch)"
        clip-path="url(#geometry-1)"/>
  <rect width="100%" height="100%"
        fill="url(#hatch-30)"
        clip-path="url(#geometry-2)"/>
  <rect width="100%" height="100%"
        fill="url(#hatch-60)"
        clip-path="url(#geometry-3)"/>
</g>
```

The complete palette stack may be isolated using isolation: isolate to constrain its blending to the drawing. Antialiasing of boundaries is a rendering concern, not a property of the logical binary coverage model.

### Consequence for flattening

Overlaps among instances sharing a palette entry do not themselves introduce new compositing operations or new overlap colors. Their geometric relationships can remain implicit until needed; no explicit vector union is required merely to render the group.

> **The union can stay implicit in the rendering, but the instances cannot be left out** (@Pauli, Nov 18). The paragraph above relates mainly to the flattening (section 08), as its heading says. It does not let the instances be left out. The group of a palette entry is its instances, which pass 1 makes (section 02), and pass 2 reads the instances only. To draw the shapes of a palette entry into one layer while the symbols are drawn, and to apply α and B to that layer, gives the same appearance, but not this architecture.

SVG examples describe the encoding mechanisms; their behavior across different rendering engines has not been tested in this specification.

References: [\[9\] MDN: SVG opacity](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Attribute/opacity) • [\[10\] W3C: Compositing and Blending](https://www.w3.org/TR/compositing-1/)

## 06 / Color and ink model: Ink identity and color-space independence

Ink is a colorant identity distinct from palette identity. A palette entry may reference one ink at a specified percentage, a mixture of inks, or a direct color. CMYK process colors form a four-ink case of the generalized ink model; spot inks extend it. RGB describes direct color or simulated appearance. [1, 4]

| Entity             | Role                                                                  |
|--------------------|-----------------------------------------------------------------------|
| Ink                | Independent printing colorant identity and color definition.          |
| Palette entry      | Color or ink contributions; alpha, blend mode, knockout and priority. |
| Geometry prototype | Reusable origin-bound periodic geometry.                              |
| Layer instance     | Palette/prototype references and effective transformations.           |
| Clipping object    | Vector coverage associated with an instance.                          |

### Non-idempotent blending

Prototype reuse avoids geometry duplication. Collective compositing prevents repeated application of a palette entry. Neither alone resolves shared inks across different palette entries. For example, cyan in a green ink mixture and cyan in a blue entry should be resolved within the same ink separation before mixing with other inks.

![Figure 7](figures/figure-7.png)

Figure 7. Illustrative comparison: overlapping cyan-plus-yellow green and cyan blue; the ink-based example assumes overprinting preserves yellow. Colors are not color-managed simulations.

### Knockout and overprint

OCAD opacity corresponds to alpha. OCAD’s process-color Overprint flag and OOM’s Knockout flag convey inverse printing intent: preserve or remove underlying inks. Keep knockout distinct from opacity and blend mode. OCAD spot separations additionally distinguish an undefined ink contribution (leave unchanged), 0% (erase), and a specified positive percentage (set coverage). [1, 4, 12–13]

References: [\[12\] OCAD: Export Files](https://www.ocad.com/wiki/ocad/en/index.php?title=Export_Files) • [\[13\] OOM: Overprint export discussion](https://github.com/OpenOrienteering/mapper/issues/2228)

## 07 / Color representation: Color serialization and rendering modes

### Color-space independence

The logical color model is independent of color space. Support RGB, CMYK and spot ink compositions without changing the geometry architecture. Preserve explicit CMYK components during SVG serialization where applicable, with an RGB representation for display. OOM similarly distinguishes desktop RGB, process CMYK and spot ink settings. [4]

### SVG compatibility for CMYK

SVG 1.1 supports ICC-based color definitions alongside an sRGB fallback. Inkscape documents this representation; Scribus documents importing four-component ICC colors as CMYK without applying the referenced profile. Compatibility depends on the renderer and version. [14–16]

```xml
<defs>
  <color-profile name="print-cmyk"
                 xlink:href="print-profile.icc"/>
</defs>
<rect width="100%" height="100%"
      fill="#009933 icc-color(print-cmyk, 1, 0, 1, 0)"
      clip-path="url(#green-geometry)"/>
```

Illustrative fragment: the surrounding SVG must declare the xlink namespace and supply the actual ICC profile and clipping geometry. The RGB fallback is illustrative, not a profile-derived conversion. Some browsers reject ICC paint declarations instead of displaying the fallback. CMYK serialization alone does not implement ink-aware overprinting.

### Rendering-mode independence

The instantiated geometry is independent of the color compositing configuration. The same geometry can serve normal editing views, simulated overprinting, physical ink separations or transparency-flattened vector output. This is an architectural capability, not a prescription of rendering-mode implementations. OCAD distinguishes normal view and print-related color behavior; OOM provides overprint simulation. [2, 4, 11]

In an ink-aware flattened output, preserve computed CMYK or ink values where relevant rather than necessarily converting an RGB simulation back into ink components.

The SVG fragments are illustrative. Their combined behavior in Inkscape, Scribus and browser rendering engines has not been tested.

References: [\[14\] W3C: SVG 1.1 Painting](https://www.w3.org/TR/SVG11/painting.html#SpecifyingPaint) • [\[15\] Inkscape: ICC color support](https://wiki.inkscape.org/wiki/Release_notes/0.46) • [\[16\] Scribus: SVG import](https://sources.debian.org/data/non-free/s/scribus-doc/1.4.8%2Bdfsg-1/doc/de/scribus-svg.html)

## 08 / Optimization specification: Vector transparency flattening

The task is to determine an optimal strategy for flattening the transparencies of this configuration into opaque vector objects while preserving the composited appearance. The architecture is a given design constraint; flattening is a separate optimization task.

At any position, clipping objects and transformed periodic geometry determine the participating palette entries. Their uniform color and compositing properties are evaluated in palette order, using the selected color configuration. The resulting opaque colors must be associated with corresponding vector geometry.

![Figure 8](figures/figure-8.png)

Figure 8. Two overlapping palette separations can produce three different opaque color regions in addition to the default color. This illustrates the task, not a prescribed output organization.

### Primary objective — geometric computation cost

Minimize the computational cost arising from geometric intersection calculations during transparency flattening. Consider the number and complexity of geometric operations, redundant calculations, intermediate and derived geometry, and unnecessary expansion of repetitive geometry. Counting intersections alone is insufficient, since the cost of individual operations can vary substantially.

### Secondary objective — output representation size

Minimize the total encoded size of the flattened vector drawing, including palette colors, geometry, references and repetitive definitions. Output compactness is desirable, but subordinate to minimizing geometric computation cost.

### Mandatory constraints and scope

Flattening must take place entirely in the vector domain. Rasterization, raster sampling, resolution-dependent geometry processing and reconstruction from raster data are excluded. The output must remain resolution-independent and preserve the original composited appearance and spatial alignment.

No particular partitioning method, intersection algorithm or output organization is prescribed. Exhaustive enumeration of all possible layer combinations is not a viable general strategy because of exponential growth. The choice of algorithm remains open.

OOM advertises overprinting simulation for raster output; the objective here is to derive the flattened appearance as vector objects. [11]

References: [\[11\] OpenOrienteering Mapper: features](https://www.openorienteering.org/apps/mapper/)

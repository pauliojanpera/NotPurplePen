# Overlapping hatchings

@Pauli's case of Nov 12, for task 2 of [the handover](../SvgTargetProgress.md): where two areas of one symbol overlap,
their horizontal hatching looks thicker and darker. @Pauli saw it in Chrome's drawing of NotPurplePen's SVG and PDF
exports, in a PDF viewer and in OpenOrienteering Mapper's own drawing on screen, with the export's pattern fills and
with "Draw area patterns as separate shapes" alike, and in NotPurplePen's PNG export at 100 dpi, though not at the
higher resolution he exported at first. He agreed that his files go into this repository (Nov 12).

`case/` holds:

- `suo.ocd`: the map, with three overlapping marsh areas of symbol 308.0 (Suo), georeferenced in ETRS-TM35FIN
  (EPSG:3067);
- `suo.ppen`: its event, whose page of all controls prints at 1:10,000;
- `suo-notpurplepen-export.svg`: that page as NotPurplePen's SVG export wrote it, with "Draw area patterns as separate
  shapes";
- `suo-notpurplepen-export.pdf`: that page as NotPurplePen's PDF export wrote it;
- `suo2.ocd`: a second map, in which overlapping rectangular marsh areas, none of them turned, show the same, so the
  artifact does not depend on the areas' rotation (@Pauli, Nov 12).

The investigation adds its scripts and findings beside them.

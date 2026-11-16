# The model of the flattening

Nov 16, 2026. This file gives the rules by which the flattening calculates the colour of each point of a page. The
requirements FL-2 to FL-5 of [README.md](README.md) say what the rules must do. This file says how. The code, the
raster reference and the probe values of the cases all follow this file. A port to another language follows it too.

The session keeps this file. An agent does not change it. An agent that finds a rule wrong, or a case that the rules
do not decide, stops and says so.

## Tones and channels

- The model calculates each of the four channels C, M, Y and K alone, as ISO 32000-1 does for a blend mode that is
  separable (11.3.5).
- The **tone** of a channel is 1 less the channel's value. Tone 1 is paper without ink. Tone 0 is full ink.
- The paper is white: its tone is 1 in each channel.
- At the end, the value of a channel is 1 less its tone.

To blend in CMYK is to blend the tones. ISO 32000-1 complements the values of a subtractive colour space before the
blend function and after it, which gives the same result.

## What a drawing is

- A **colour** has four CMYK values, an **opacity** `a` (above 0, at most 1) and an overprint flag.
- A **mark** is one shape drawn in one colour: a filled area, the outline of a stroked line, or the outline of a
  glyph. A mark covers a set of points.
- A **drawing** is its marks in the order in which they are drawn, the lowest first.
- A **run** is the marks of one colour that follow each other with no mark of another colour between them. A map draws
  the colours of its table one after the other, the lowest first, so the marks of one colour of the table are in one
  run. Colours of the table that follow each other with the same values, the same opacity and the same overprint flag
  are one run together, and a colour of the table that draws nothing has no run: `LordHill.ocd` has 43 colours and
  20 runs ([Measurements.md](Measurements.md), question 1).
- A **knockout colour** is a colour that is not overprinted.
- An **ink** is all the overprinted colours that have the same CMYK values, wherever they are in the order (FL-2).
  The opacity is not a part of the ink: it is the coverage that a mark asks for.
- The **blend mode** is darken or multiply. A page has one blend mode.

## How the stage reads the drawing calls

- A mark drawn between `PushBlending` and its `PopBlending` is a mark of an overprinted colour. Each other mark is a
  mark of a knockout colour.
- The CMYK values and the opacity of a mark are those of its brush or its pen.
- The blend mode is the mode that `PushBlending` gives.

## The rules

At each point, and for each channel, the model keeps a **base** tone `b` and, for each ink `i`, a **coverage** `A_i`.
At the start `b` is 1 and each `A_i` is 0. The model takes the marks that cover the point, the lowest first.

| Rule | The mark | What changes |
| --- | --- | --- |
| R1 | A mark of ink `i` with opacity `a` | `A_i` becomes the larger of `A_i` and `a` (FL-3). Nothing is added. |
| R2 | A mark of a knockout colour with opacity 1 and tone `t` | `b` becomes `t`. Each `A_i` becomes 0 (FL-4). |
| R3 | The first mark of a run of a knockout colour with opacity `a` below 1 and tone `t` | `b` becomes `(1 - a) * p + a * t`, where `p` is the tone that shows at that moment (see below). Each `A_i` becomes 0. The other marks of the same run change nothing at this point. |

**The tone that shows** (FL-5) is the base with the inks on it:

- By multiply: `p = b * (1 - A_1 + A_1 * t_1) * (1 - A_2 + A_2 * t_2) * ...`, where `t_i` is the tone of ink `i`.
  The order of the inks changes nothing.
- By darken: start with `p = b`. For each ink with a coverage above 0, in the order of the first mark of each ink in
  the drawing, `p` becomes `(1 - A_i) * p + A_i * min(p, t_i)`. When each coverage is 1, `p` is the smallest of `b`
  and the tones of the inks, and the order of the inks changes nothing.

**The result** at a point is the tone that shows after the last mark. A point that no mark covers has no result: it
is paper, and no piece covers it.

A **piece** is an area in which the result is one CMYK value. Areas that touch and have the same result are one
piece.

## Where the model and today's drawing agree

Today's drawing is the drawing of `feature/multiply-blending`: the consecutive colours of one ink are one layer,
which blends one time. In these conditions the model gives the same tones as today's drawing, channel by channel:

- knockout colours with opacity 1, in any number;
- an ink whose colours are consecutive, with marks that overlap;
- different inks that cross, by multiply and by darken;
- one mark of an ink or of a knockout colour with an opacity below 1.

## Where the model and today's drawing differ

These differences are the purpose of the work. A gate that compares with today's drawing does not apply at these
points (GATE-1 in [README.md](README.md)).

| | The condition | Today's drawing | The model |
| --- | --- | --- | --- |
| D1 | Two colours of one ink are not consecutive, and their marks overlap with no knockout colour between them. Blend mode multiply. | The ink is put down two times: `b * t * t`. | The ink has one coverage: `b * t` (R1). |
| D2 | Marks of one colour with an opacity `a` below 1 overlap. | The opacities add: the coverage is `1 - (1 - a) * (1 - a)`. | The coverage is `a` (R1, R3). |

By darken, D1 changes nothing, because the darker of a tone and itself is that tone.

## Examples

The colours of the examples:

| Colour | CMYK | Tones | Kind |
| --- | --- | --- | --- |
| Purple | 0.2, 0.8, 0, 0 | 0.8, 0.2, 1, 1 | Ink |
| Brown | 0, 0.5, 1, 0.2 | 1, 0.5, 0, 0.8 | Ink |
| Green | 0.6, 0, 0.8, 0 | 0.4, 1, 0.2, 1 | Knockout colour |

| The marks at the point, the lowest first | Mode | Tones | CMYK |
| --- | --- | --- | --- |
| Purple, Brown | multiply | 0.8, 0.1, 0, 0.8 | 0.2, 0.9, 1, 0.2 |
| Purple, Brown | darken | 0.8, 0.2, 0, 0.8 | 0.2, 0.8, 1, 0.2 |
| Purple, Purple of a colour that is not consecutive | multiply | 0.8, 0.2, 1, 1 | 0.2, 0.8, 0, 0 (today's drawing: 0.36, 0.96, 0, 0) |
| Purple, Green | either | 0.4, 1, 0.2, 1 | 0.6, 0, 0.8, 0 |
| Purple, Green, Purple | multiply | 0.32, 0.2, 0.2, 1 | 0.68, 0.8, 0.8, 0 |
| Purple, Green, Purple | darken | 0.4, 0.2, 0.2, 1 | 0.6, 0.8, 0.8, 0 |
| Purple at opacity 0.5, two marks | multiply | 0.9, 0.6, 1, 1 | 0.1, 0.4, 0, 0 (today's drawing: 0.15, 0.6, 0, 0) |

## Points that @Pauli has not decided

The model takes a position on each, so that the work can go on. None changes cases 1 to 5.

- **The opacity is not a part of an ink.** Two overprinted colours with the same CMYK values and different opacities
  are one ink, and the larger opacity is the coverage where their marks overlap. `Map.IsSameInk` of task 7 compares
  the opacity too. The handover's words "within an ink the marks take the maximum of their coverage" need marks of
  one ink that ask for different coverages, so the model follows them.
- **By darken, inks with a coverage below 1 depend on their order.** The model uses the order of the first mark of
  each ink. Multiply has no such order.
- **A knockout colour with an opacity below 1** closes the inks below it (R3). A later mark of one of those inks
  puts its ink down again on the part of the lower ink that shows through. Today's drawing does the same.

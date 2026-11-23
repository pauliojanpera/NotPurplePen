## General description
The course object key and the symbol keys of the course map. `CourseFormatter` and `TopologyFormatter` give each
course object that they add to a course layout a course object key (`CourseObj.Key`): the role of the place in the
data of `EventDB`, the identifiers of `EventDB` that the place uses, and the layer of the course object.
`CourseLayout.RenderToMap` gives the symbols of the course map keys from the course object key and the index of the
symbol among the symbols of its course object. So a course map that is made again from the same state of `EventDB`
gives the same keys to the same course objects. The drawing does not use the keys, and it does not change.

## Changes in detail
* `CourseObjKey.cs` (PurplePenCore): `CourseObjRole`, the 13 roles of the places of the formatters, and
  `CourseObjKey`, a value with the role, the identifiers and the layer. Its text form is
  `<role>.<identifier>….<layer>`, with the numbers in base 62 as the counter keys of ticket 025 and the name of the
  role with a lowercase first letter, for example `leg.4.5.0`. Two keys are the same when their text forms are the
  same.
* `CourseObj.Key` and the internal `CourseObj.SetKey`: a formatter gives the role and the identifiers where it makes
  the course object, and the key reads the layer from `CourseObj.layer` when it is read, as the formatters set the
  layer after they make the course object. The key is not a part of `CourseObj.Equals` or `ToString`, so the tests
  that compare layouts as text do not change. The fields `controlId`, `courseControlId`, `courseControlId2` and
  `specialId`, which the selection uses, do not change.
* The formatters add course objects in 11 places, which give the roles of the ticket: `Special`, `Description`,
  `Control`, `ExtraCourseControl` and `Label` in `CourseFormatter.FormatCourseToLayout`, with `Label` for the labels
  that the user placed and for those that the formatter placed; `Leg`, `LegBeginning`, `LegEnd` and
  `LegMapIssuePoint` in `CreateLeg`; and `TopologyControl`, `TopologyLeg`, `DropTarget` and `VariationCode` in
  `TopologyFormatter`. The class of a course object is not a part of the key, so a whole leg keeps its key when it
  becomes flagged.
* `Map.PrefixSymbolKeys(string prefix)` (MapModel) starts a period in which `AddSymbol` gives each symbol without a
  key, and each hole that the map gets with it, the key `<prefix>/<index>`. The index is in base 62, from 0, and it
  skips an index whose key a symbol of the map has, so two course objects of one key give no usage error. The period
  ends when its releaser is disposed. A period is in a write, and periods do not nest.
* `CourseLayout.RenderToMap` opens a period with the text form of the course object key for each
  `CourseObj.AddToMap`, so the 14 implementations of `AddToMap` do not change. A course object without a key gives
  its symbols keys from the counter.
* `CourseObjKeyTests` (PurplePen_Tests, 14 tests) checks the text form of a key, and that each course object of each
  layout of 11 test events has a key and that the keys of a layout are unique: each course, each part and each
  variation with the view of all the controls and the other courses, as `SelectionMgr` lays them out, the view of all
  the controls alone, and the topology views. The 584 layouts have 51,098 course objects. The tests check that the
  symbols of a course map have the keys `<course object key>/<index>`, the same in a course map that is made again,
  and that the keys stay over a move of a control, a bend of a leg, a move of a control number, the flagging of a
  whole leg and a map exchange at a control.
* `SymbolKeyTests` (Test_MapModel) gets three tests of the periods: their keys, with the holes, the keys that the
  maker gives and the counter after the period; the index that skips the keys of the map; and the usage errors.

## Issue ticket number
[026-course-object-keys](../tickets/026-course-object-keys.md)

## Missing functionality
* The encodings write the keys into SVG and PDF files. The epic does not open such a file as a map.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.

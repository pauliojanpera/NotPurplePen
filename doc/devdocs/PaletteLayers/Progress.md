# Palette layers: progress

Nov 18, 2026. This document tracks the branches and the decisions of the palette-layer work. The branches are not in
`basewithfixes` or `dev`, so `../NotPurplePenProgress.md` does not track them (@Pauli, Nov 18).

- [Specification.md](Specification.md) is the reference of the architecture.
- Each branch goes through a ticket in [tickets/](tickets/) and a pull request description in [prs/](prs/), as
  upstream pull requests would (@Pauli, Nov 18). The templates are in the same folders.
- [tools/](tools/) has the scripts of the work, and its README tells a ticket's loop.

## Branches

A branch is rooted where `base` meets upstream (d48f4456), and runs in full on `wip/linux/<name>`: a feature on
`basewithfixes`, and a fix alone on `base`. No branch is offered upstream yet.

| Branch | Ticket and description | State |
| --- | --- | --- |
| `feature/inks` | [001](tickets/001-inks.md), no description yet | All criteria met. Full run passed at a75c49aa (`wip/linux/inks`: Tests 44171873489, Build 44171873441). No existing baseline moves (collecting run 44155029726); the new Skia test `MapperOverprinting` has the baseline `skia_render/hakolaoverprinting.png`, from Tests run 44158232814. It merges `fix/mapper-export-color-numbers`. |
| `fix/mapper-export-color-numbers` | [002](tickets/002-mapper-export-color-numbers.md), [description](prs/002-mapper-export-color-numbers.md) | Full run passed (`wip/linux/mapper-export-color-numbers`, on `base`: Tests 44171502072, Build 44171502166). Its test `ExportNumbersColorsByPlace` fails without the fix. In `feature/inks` (a75c49aa), which had the same fix as 91994366. |
| `fix/ocad11-utf8-strings` | [003](tickets/003-ocad11-utf8-strings.md), [description](prs/003-ocad11-utf8-strings.md) | Full run passed (`wip/linux/ocad11-utf8-strings`, on `base`: Tests 44196794627, Build 44196794547). No baseline moves (collecting run 44176064791, of an earlier form of the fix). Its test file is the ISSprOM 2019 symbol set of the Finnish Orienteering Federation, which the federation gives out with no statement about redistribution (@Pauli, Nov 18: little harm in it). In no other branch. |

## Decisions

- **The tracking of the branches** (Nov 18). The palette-layer branches are tracked here, apart from
  `../NotPurplePenProgress.md`, as long as they are not in `dev`.
- **Tickets and pull request descriptions** (Nov 18). Every branch goes through a ticket and a pull request
  description, also when no pull request is made. A ticket names the fixes and features separated from it under
  "Erotetut alatehtävät".
- **The screens of the process inks** (Nov 18). Nobody prints from OCAD 6 to 8 files now. So the process inks are the
  same for all maps and have no halftone screen. OCAD 6 to 8 give screens to the process inks in their symbol header,
  and the screens stay in it through an OCAD 6 to 8 round trip.
- **No fallback for files that Purple Pen wrote** (Nov 18). Purple Pen does not read its course files back as maps.
  So `fix/ocad11-utf8-strings` reads an OCAD 11 or later file in UTF-8 only, also where an earlier Purple Pen wrote
  the file in the 8-bit code page.

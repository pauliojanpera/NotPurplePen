# Translations

The tools that find and fill the translations that the languages of NotPurplePen lack. @Pauli, Nov 16: find out
which translations each translated language lacks, and add them; first a table made mechanically, then the
translations in phases, with scripts where they make the work faster.

The table is [Status.md](Status.md). Make it again after each phase.

## What the tools found

On Nov 16, from `dev` at 4c6ca29d.

- **The languages.** The Avalonia application offers each language that has a satellite folder
  (`SwitchLanguageDialogViewModel.CreateDefaultLanguages`): 32 languages. Six resources hold its texts: `UIText` of
  AvPurplePen, and `MiscText`, `CommandNameText`, `StatusBarText`, `SelectionDescriptionText` and `ReportText` of
  PurplePenCore.
- **19,822 translations are missing.** Seven languages translate almost nothing: ca, he, id, ku, ru, vec and zh-HK.
- **868 of them are in the repository.** Upstream's TranslateTool writes the satellites of all resources into
  `src/PurplePen` (`doc/devdocs/SynchronizeTranslations.txt`). For ca, en-GB, he, id, it, ku, ru, sl, vec and zh-HK,
  the satellites of PurplePenCore's resources stayed there. `PurplePen.csproj` removes all files of these ten
  languages from its build, and PurplePenCore has no copy. So the application shows PurplePenCore's texts in English
  in these languages. The satellites hold en-GB 397, it 396, sl 68, ru 5, ca 1 and ku 1 of the missing translations.
- **The other sources of upstream give nothing more.** The satellites of the WinForms forms and the PO files in
  `loc/po/purplepen` hold no translation that a satellite of the six resources lacks under the same key.
  `packet.py` uses them for the same text under other keys.
- **Launchpad's table counts something else.** Upstream translates on <https://translations.launchpad.net/purple-pen>,
  whose page has a table of each language. It counts the 938 strings of the WinForms version 3.5.1, as last changed
  in 2024. It does not count the keys of the Avalonia application or of the fork. Its Italian has 934 of the 938;
  the PO file in the repository, 440.
- **Replaced controls lost their translations.** Where the Avalonia dialog has a new key for a control that replaced
  a WinForms control, the satellites have no translation of the new key. Example:
  `CourseAppearanceDialog_labelBlendPurple_Text` replaced the check box `checkBoxBlendPurple`, whose Finnish is
  "Läpinäkyvä violetti" (`loc/po/purplepen/fi.po`). `packet.py` shows such translations as "Same text".
- **22 errors in the existing translations.** `check.py` finds 13 strings, most of them both in UIText and in
  PurplePenCore, with a placeholder that the English does not have, or with a brace outside a placeholder. Where a
  text goes through `string.Format`, it throws; elsewhere the reader sees the braces.
  - hu `PrintingPage`: "Nyomtatás (0} / {1}. oldal".
  - ja `CannotCreatePdfs` and `CannotCreateOcadFiles`: `{0}` (task 9); `TextLine_AboveAllCourses` and
    `TextLine_BelowAllCourses`: "(0}".
  - nb-NO `LegLength_CourseInfoNoClimb`: "0} ({1} poster, {2})".
  - pt `CustomizedSymbolDesc`: `{0}`.
  - tr `PrintDescriptions_marginsLabel_Text` and `PrintPunches_marginsLabel_Text`: "Kenar Boşluğu ({0})".
  - zh-CN `Margins_LRTB`: "{三}"; `PartXOfY`: "{一}"; `LegLength_CourseInfoNoClimb`: "{一}" and "{二}".

## The phases

Each phase ends with a full run on `wip/linux/<name>`, the merges into `basewithfixes` and `dev` through full runs,
and Status.md made again. The numbers are of Nov 16.

1. **The translations in the repository.** Move the satellites of PurplePenCore's resources of the ten languages
   from `src/PurplePen` into `src/PurplePenCore` (`git mv`), and take their lines out of the `EmbeddedResource
   Remove` list of `PurplePen.csproj`. 868 translations. Nothing is translated.
2. **The faults.** Task 9's first branch (handover): "Include:" and its items, the faults that task 4 found, and the
   errors that `check.py` reports.
3. **The languages that lack fewer than 300 after phase 1:** bg, cs, de, en-GB, es, et, fi, fr, hu, it, ja, nb-NO,
   pt, pt-BR, sv, tr, uk, zh-CN and zh-TW. 2,516 keys of upstream.
4. **The languages that lack 300 to 1,000:** da, nl, nn-NO, pl and ro. 3,230 keys of upstream.
5. **The languages that lack more than 1,000:** ca, he, id, ku, ru, sl, vec and zh-HK. 12,450 keys of upstream.
6. **The fork's own keys** (the column "Fork's own" of Status.md): 758, on the branches that add the keys, as task
   4 did for the SVG export.

Phases 1 and 2 go on task 9's first `fix/` branch, and phases 3 to 5 on its second. Both are rooted where `base`
meets upstream, so that they can be offered upstream.

## The work of one language

1. Make the packet from a checkout of the branch where the translations go:
   `packet.py <checkout> <language> <work folder>`. On a branch of upstream's history, the packet has only upstream's
   keys. On a branch of the fork, `--upstream-only` or `--forks-own` takes one of the two.
2. The translator reads `<language>.md` and fills `<language>.tsv`: the translation, the context that decided it
   (`note`), and the existing keys whose translation it follows (`mirrors`). It uses the terms of the language's own
   translations, and keeps to the room that the dialogs give, as task 4 did. For a key that needs no translation in
   the language, it writes `=`.
3. `apply.py <checkout> <language> <language>.tsv` writes the translations into the satellites, and records the
   keys marked `=` in `not-needed/<language>.tsv` beside the scripts.
4. `check.py <checkout> --language <language> --since <commit before the language> --warnings`: no error, and each
   warning examined.
5. `review.py <language>.tsv > reviews/<language>.md`: the review table. Keep the filled TSV beside it.
6. Commit the satellites of the language in one commit on the branch, and `not-needed/` and `reviews/` on
   `docs/process-coordination`.

## The scripts

All take the path of a checkout of NotPurplePen. They need Python 3 and git, and nothing else. `--help` gives the
details.

| Script | Does |
| --- | --- |
| `status.py <checkout> <out folder>` | Writes `Status.md`, the table, and `missing/<language>.tsv`, the keys that each language lacks, with the English, the satellite in `src/PurplePen` that holds a translation, and whether the key is the fork's own. |
| `packet.py <checkout> <language> <out folder>` | Writes the packet of one language: `<language>.md` to read, with the language's terms and, for each key, the English, the same text in the language, and the key in the related languages; and `<language>.tsv` to fill. |
| `apply.py <checkout> <language> <tsv>...` | Writes the filled lines into the satellites, in the order of the English file, and checks each satellite after it. Leaves out a line whose English is not the English of now, and a key translated already, unless `--replace`. |
| `check.py <checkout>` | Reports errors (no parse, a repeated name, an empty value, a placeholder or brace that `string.Format` throws on) and, with `--warnings`, what to examine (a lost placeholder, an access-key mark, the spaces or the colon or ellipsis at the ends, a text that may not fit). |
| `review.py <tsv>...` | Writes the review table of filled TSV files. |
| `resxlib.py` | What the scripts share: the resources, the related languages, and the reading and writing of `.resx`, PO and TSV files. |

## The TSV files

UTF-8, a header line, one line for each key, the fields separated by tabs. Within a field, a line break is `\n`, a
tab `\t` and a backslash `\\`. A packet's TSV has the columns `resource`, `key`, `english`, `translation`, `note`
and `mirrors`.

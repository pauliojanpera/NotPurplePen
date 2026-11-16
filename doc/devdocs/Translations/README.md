# Translations

The tools that find and fill the translations that the languages of NotPurplePen lack. @Pauli, Nov 16: find out
which translations each translated language lacks, and add them; first a table made mechanically, then the
translations in phases, with scripts where they make the work faster. For each key, the translator can see the
other translations of the key, and each translation that a language model makes names the model and its effort
setting.

The table is [Status.md](Status.md). A session that translates one language follows [Session.md](Session.md). A
session that starts with no context reads [../Introduction.md](../Introduction.md) first.

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

The numbers are of Nov 16.

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

Phases 1 and 2 go on task 9's first `fix/` branch, `fix/translation-faults`, and phases 3 to 5 on its second,
`fix/complete-translations`. Both are rooted where `base` meets upstream (`git merge-base origin/base
upstream/master`), so that they can be offered upstream.

- Phases 1 and 2 are not translation. A session does them, runs `fix/translation-faults` in full on
  `wip/linux/translation-faults`, rooted at `base`, merges it into `basewithfixes` and `dev` through full runs, and
  then makes `fix/complete-translations` at the root and pushes it.
- In phases 3 to 5, one session translates and checks one language, as [Session.md](Session.md) tells (@Pauli,
  Nov 16). A session may instead start one agent for each language, one at a time, with Session.md as the brief and
  nothing that watches it. The languages follow each other with no build and no test run between them: a push of
  `fix/complete-translations` starts none, and a session does not wait for one (@Pauli, Nov 16). Each session makes
  Status.md again, from `fix/complete-translations`.
- After the last language, `fix/complete-translations` runs in full on `wip/linux/complete-translations`, rooted at
  `base`, and goes into `basewithfixes` and `dev` through full runs.

## The work of one language

[Session.md](Session.md) gives the steps of the session that does one language: the packet (`packet.py`), the
translation into the filled TSV, `apply.py`, `check.py`, the push after about each 200 keys, and at the end the
review table (`review.py`) in `reviews/<language>.md` and Status.md made again. The filled TSV of each language is
kept in `filled/<language>.tsv`, and its keys judged not needed in `not-needed/<language>.tsv`.

The rules of the translation are @Pauli's (handover, task 9, Nov 14 and 16):

- Find the exact meaning of the English from every translation of the key in the other languages, the related
  languages first, and from the other keys of the same control. `lookup.py` shows every translation of any key.
- Use the terms of the language's own translations, and keep to the room that the dialogs give, as task 4 did.
- Translate only keys that need translation. Mark a key that needs none `=`.
- Correct an existing translation only when completely sure that it is wrong, and say why.
- Write a review table for each language.

## Who made a translation

A translation that a language model made carries in the `<comment>` of its entry in the satellite:

```
translator=<model id>; effort=<effort setting>; date=<YYYY-MM-DD>
```

for example `translator=claude-opus-5-5; effort=max; date=2026-11-16` (@Pauli, Nov 16). The resource manager does not
read comments, so they change nothing that the application shows. The comment goes upstream with the translation,
where it tells a reviewer which translations a model made. A person who changes such a translation changes or
removes the comment.

`apply.py` writes the comment from the `translator` and `effort` columns of the TSV, or from its `--translator` and
`--effort`. A line without a translator is written without a comment, as a person's translation. A correction
gets the comment too, and the review table shows the translation that it replaces. The not-needed file
records the same columns. Status.md counts the translations of each language that a model made ("By a model"),
`lookup.py` marks them, and `review.py` gives the model of each review table.

## The scripts

All take the path of a checkout of NotPurplePen. They need Python 3 and git, and nothing else. `--help` gives the
details.

| Script | Does |
| --- | --- |
| `status.py <checkout> <out folder>` | Writes `Status.md`, the table, and `missing/<language>.tsv`, the keys that each language lacks, with the English, the satellite in `src/PurplePen` that holds a translation, and whether the key is the fork's own. |
| `packet.py <checkout> <language> <out folder>` | Writes the packet of one language: `<language>.md` to read, with the language's terms and, for each key, the English, the other keys of the same control, the same text in the language, and the key in every other language that translates it, the related languages first; and `<language>.tsv` to fill. |
| `lookup.py <checkout> <key>...` | Shows every translation of keys, with the language model that made each, for the translator and the reviewer. `--regex` takes patterns of key names. |
| `apply.py <checkout> <language> <tsv>...` | Writes the filled lines into the satellites, in the order of the English file, with the comment that names the language model, and checks each satellite after it. Leaves out a line whose English is not the English of now, and a key translated already, unless the line corrects it (`old`) or `--replace` is given. |
| `check.py <checkout>` | Reports errors (no parse, a repeated name, an empty value, a placeholder or brace that `string.Format` throws on) and, with `--warnings`, what to examine (a lost placeholder, an access-key mark, the spaces or the colon or ellipsis at the ends, a text that may not fit). |
| `review.py <tsv>...` | Writes the review table of filled TSV files. |
| `resxlib.py` | What the scripts share: the resources, the related languages, and the reading and writing of `.resx`, PO and TSV files. |

## The TSV files

UTF-8, a header line, one line for each key, the fields separated by tabs. Within a field, a line break is `\n`, a
tab `\t` and a backslash `\\`. A packet's TSV has the columns `resource`, `key`, `english`, `translation`, `old`,
`note`, `mirrors`, `translator` and `effort`. `old` is empty except in a correction.

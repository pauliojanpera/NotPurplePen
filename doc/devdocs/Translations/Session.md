# A session that translates one language

Nov 16, 2026. This is the brief of a session that starts with no context and translates one language of NotPurplePen
(@Pauli, Nov 16: one session translates and checks one language). Do the steps in order. The scripts and the facts
behind them are in [README.md](README.md).

@Pauli starts the session with a prompt such as:

> Translate the next language of NotPurplePen. Follow `doc/devdocs/Translations/Session.md` on the branch
> `docs/process-coordination` of `pauliojanpera/NotPurplePen`. You are `<model id>`, at effort `<effort setting>`.

The model id and the effort setting go into the metadata of each translation. If the prompt does not give them, give
your own model id, and ask @Pauli for the effort setting before you write a translation.

## 1. Get the context and the files

1. Attach `pauliojanpera/NotPurplePen` with push access.
2. Clone the documents, then read [`doc/devdocs/Introduction.md`](../Introduction.md) and [README.md](README.md):

   ```
   git clone --depth 1 --no-checkout -b docs/process-coordination https://github.com/pauliojanpera/NotPurplePen docs
   git -C docs sparse-checkout set --no-cone /doc/devdocs/
   git -C docs checkout
   ```

3. Clone the code at the branch of the translations, and fetch `base` for the check. Give each command up to ten
   minutes.

   ```
   git clone --depth 1 -b fix/complete-translations https://github.com/pauliojanpera/NotPurplePen code
   git -C code fetch --depth 1 origin base:refs/remotes/origin/base
   ```

   If `fix/complete-translations` is not on GitHub, phases 1 and 2 are not done. Stop, and tell @Pauli.

## 2. Find your language

Your language is the first language of this list that has no file `docs/doc/devdocs/Translations/reviews/<language>.md`. The list is the
languages of phases 3, 4 and 5 (README.md, "The phases"), in that order:

bg, cs, de, en-GB, es, et, fi, fr, hu, it, ja, nb-NO, pt, pt-BR, sv, tr, uk, zh-CN, zh-TW,
da, nl, nn-NO, pl, ro,
ca, he, id, ku, ru, sl, vec, zh-HK.

If every language has its review, stop, and tell @Pauli that the languages are done.

If `docs/doc/devdocs/Translations/filled/<language>.tsv` exists, an earlier session stopped in this language. Go on from its file: the next
step keeps its lines.

## 3. Make the packet

```
python3 docs/doc/devdocs/Translations/packet.py code <language> work --tsv docs/doc/devdocs/Translations/filled/<language>.tsv --translator <model id> --effort <effort setting>
```

`work/<language>.md` is what you read. `docs/doc/devdocs/Translations/filled/<language>.tsv` is the file that you fill. It has a line for each key
that the language lacks.

## 4. Translate

Read `work/<language>.md` in parts (the Read tool's offset and limit), one dialog after another. For each key, fill
its line of `docs/doc/devdocs/Translations/filled/<language>.tsv`:

- **translation**: the translation. Before you write it, find the exact meaning of the English. Read the other keys
  of the same control, the same text in the language, and the key in every other language, the related languages
  first. `python3 docs/doc/devdocs/Translations/lookup.py code <key>` shows every translation of any key.
- Use the terms of the language's own translations ("Terms", and "Same text"). Keep the access-key mark (`_` or `&`)
  where the English has one, on a letter that no other item of the same menu uses. Keep every placeholder `{0}`,
  `{1}` of the English, and add none.
- Keep to the room that the dialogs give. Where a text must fit a control, keep it about as long as the English and
  as the language's other texts of the same dialog. Task 4 measured the room by rendering the dialogs
  ([SvgTranslations.md](../SvgTranslations.md), "Which strings, and how room was checked").
- **note**: the context that decided the translation, in a few words.
- **mirrors**: the existing keys whose translation you follow.
- A key that needs no translation in the language: write `=` as the translation, and why in note.
- A key whose meaning stays unclear: leave its translation empty, and write why in note. It stays missing.
- **A correction.** If you are completely sure that an existing translation of the language is wrong, add a line for
  its key: the English, the existing translation exactly as the satellite has it in `old` (`lookup.py` gives it), the
  correction in `translation`, and why the existing one is wrong in note. A correction in another language goes into
  `docs/doc/devdocs/Translations/filled/<that language>.tsv`, and `apply.py` writes it for that language.

In the TSV, write a line break as `\n` and a tab as `\t`.

## 5. Write, check and push, after about each 200 keys and at the end

So that a session that stops loses little, do this after about each 200 keys, and at the end.

1. Write the translations into the satellites:

   ```
   python3 docs/doc/devdocs/Translations/apply.py code <language> docs/doc/devdocs/Translations/filled/<language>.tsv
   ```

   To change a translation that this session wrote already, change its line, and run the command with `--replace`.
2. Check them. There must be no error. Examine each warning, and correct the line where the warning is right.

   ```
   python3 docs/doc/devdocs/Translations/check.py code --language <language> --since origin/base --warnings
   ```

3. Commit and push the satellites. Do not wait for a build or a test run: a push of this branch starts none, and
   the full run comes after all languages (README.md, "The phases").

   ```
   git -C code add src
   git -C code commit -m "Translate the <Language> strings that the satellites lacked" -m "<how many keys of which resources; the model id and the effort setting>"
   git -C code -c push.negotiate=true push origin fix/complete-translations
   ```

4. Commit and push the filled file and the not-needed file:

   ```
   git -C docs add doc/devdocs/Translations
   git -C docs commit -m "Record the <Language> translations so far"
   git -C docs -c push.negotiate=true push origin docs/process-coordination
   ```

   If a push is refused because the branch moved, pull with `--rebase` and push again.

## 6. Finish the language

1. Make the review table, and write above it: how many keys the session translated, marked not needed, corrected and
   left empty; the keys left empty, and why; and your doubts.

   ```
   python3 docs/doc/devdocs/Translations/review.py docs/doc/devdocs/Translations/filled/<language>.tsv > docs/doc/devdocs/Translations/reviews/<language>.md
   ```

2. Make the table again, from the branch of the translations:

   ```
   python3 docs/doc/devdocs/Translations/status.py code docs/doc/devdocs/Translations
   ```

3. Commit and push the documents as in step 5.4, with the message "Finish the <Language> translations".
4. Archive this session's transcript, as the README of the branch `sessions` of `pauliojanpera/windows-fonts` tells
   (`archive-session.sh`). Attach that repository with push access for it.
5. Tell @Pauli, in a few lines: the language; how many keys were translated, marked not needed, corrected and left
   empty; and the doubts that he should see.

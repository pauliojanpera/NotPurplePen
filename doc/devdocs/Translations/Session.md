# A session that translates key groups

Nov 16, 2026. This is the brief of a session, or of an agent that a session starts, that begins with no context and
translates the next key groups of NotPurplePen, each for all translated languages at once (@Pauli, Nov 16: the work
goes key by key, so that each key is translated with all its translations in view). Do the steps in order. The
scripts and the facts behind them are in [README.md](README.md). The terms are settled before this work
([Terms.md](Terms.md)).

@Pauli, or the session that starts the agent, gives a prompt such as:

> Translate the next key groups of NotPurplePen. Follow `doc/devdocs/Translations/Session.md` on the branch
> `docs/process-coordination` of `pauliojanpera/NotPurplePen`. You are `claude-opus-5-5`, at effort `max`.

The model id and the effort setting go into the metadata of each translation. @Pauli chose `claude-opus-5-5` at
effort max on Nov 16, to start with. If the prompt gives others, use those of the prompt.

Nothing in this work waits for a build or a test run: a push of `fix/complete-translations` starts none, and the full
run comes after all groups.

## 1. Get the files

1. Clone the documents, then read [`doc/devdocs/Introduction.md`](../Introduction.md) and [README.md](README.md):

   ```
   git clone --depth 1 --no-checkout -b docs/process-coordination https://github.com/pauliojanpera/NotPurplePen docs
   git -C docs sparse-checkout set --no-cone /doc/devdocs/
   git -C docs checkout
   ```

2. Clone the code at the branch of the translations, fetch `base` for the check, and fetch the branch of phases 1
   and 2. Give each command up to ten minutes.

   ```
   git clone --depth 200 -b fix/complete-translations https://github.com/pauliojanpera/NotPurplePen code
   git -C code fetch --depth 1 origin base:refs/remotes/origin/base
   git -C code fetch --depth 200 origin fix/translation-faults:refs/remotes/origin/fix/translation-faults
   git -C code merge-base --is-ancestor origin/fix/translation-faults HEAD
   ```

   If the last command fails, phases 1 and 2 are not in the branch: stop, and tell @Pauli.

## 2. Take the next group

```
python3 docs/doc/devdocs/Translations/grouppacket.py code work
```

Without `--group`, the script takes the first group, in their order, that has no review and lacks a translation, and
prints its name. If it prints "all groups are done", stop, and say so (step 6). Then make the packet into the filled
file of the group:

```
python3 docs/doc/devdocs/Translations/grouppacket.py code work --group <group> --tsv docs/doc/devdocs/Translations/filled/groups/<group>.tsv --translator <model id> --effort <effort setting>
```

If `filled/groups/<group>.tsv` exists, an earlier session stopped in this group. The command keeps its lines and adds
the ones that it lacks.

`work/<group>.md` is what you read. `filled/groups/<group>.tsv` is what you fill: a line for each language and key
that the language lacks.

## 3. Translate

For each key of the packet, first find the exact meaning of the English: read the key's translations in all
languages, the other keys of the same control, and the copy or original of the key. Then fill the key's lines, one
for each language that lacks it:

- **translation**: the translation. Aim at natural language (@Pauli, Nov 16): it reads as text written in the
  language, not as the English put into the language's words.
- Use the terms that the packet gives under "Terms" ([terms.csv](terms.csv); `grep -i` it for others), and the
  language's own translations under "Same text". Keep the access-key mark (`_` or `&`) where the English has one, on a
  letter that no other item of the same menu uses. Keep every placeholder `{0}`, `{1}` of the English, and add none.
- A key of PurplePenCore and its copy in UIText get the same translation.
- Keep to the room that the dialogs give. Where a text must fit a control, keep it about as long as the English and as
  the language's other texts of the same dialog. Task 4 measured the room by rendering the dialogs
  ([SvgTranslations.md](../SvgTranslations.md), "Which strings, and how room was checked").
- **note**: the context that decided the translation, in a few words. **mirrors**: the existing keys whose
  translation you follow.
- A key that needs no translation in a language: write `=` as the translation, and why in note.
- A key whose meaning stays unclear in a language: leave its translation empty, and write why in note.
- **A correction.** If you are completely sure that an existing translation is wrong, add a line for its language
  and key: the English, the existing translation exactly as the satellite has it in `old` (`lookup.py` gives it), the
  correction in `translation`, and why the existing one is wrong in note. A translation that uses a term against
  terms.csv is such a correction where you are completely sure.

In the TSV, write a line break as `\n` and a tab as `\t`. `python3 docs/doc/devdocs/Translations/lookup.py code
<key>` shows every translation of any key.

## 4. Write, check and push, after about each 200 lines and at the end of each group

1. Write the translations into the satellites of every language of the TSV:

   ```
   python3 docs/doc/devdocs/Translations/apply.py code - docs/doc/devdocs/Translations/filled/groups/<group>.tsv
   ```

   To change a translation that this session wrote already, change its line, and run the command with `--replace`.
2. Check them. There must be no error. Examine each warning, and correct the line where the warning is right.

   ```
   python3 docs/doc/devdocs/Translations/check.py code --since origin/base --warnings
   ```

3. Commit and push the satellites:

   ```
   git -C code add src
   git -C code commit -m "Translate the key group <group>" -m "<how many translations into how many languages; the model id and the effort setting>"
   git -C code -c push.negotiate=true push origin fix/complete-translations
   ```

4. Commit and push the filled file and the not-needed files:

   ```
   git -C docs add doc/devdocs/Translations
   git -C docs commit -m "Record the translations of the key group <group> so far"
   git -C docs -c push.negotiate=true push origin docs/process-coordination
   ```

   If a push is refused because the branch moved, pull with `--rebase` and push again.

## 5. Finish the group, and take the next

1. Write the group's review, which marks the group as done, and make the reviews of the languages again:

   ```
   python3 docs/doc/devdocs/Translations/review.py docs/doc/devdocs/Translations/filled/groups/<group>.tsv > docs/doc/devdocs/Translations/reviews/groups/<group>.md
   python3 docs/doc/devdocs/Translations/review.py docs/doc/devdocs/Translations/filled/groups/*.tsv --out-dir docs/doc/devdocs/Translations/reviews/languages
   ```

   Write above the group's review how many lines were translated, marked not needed, corrected and left empty, the
   keys left empty and why, and your doubts.
2. Make the table again: `python3 docs/doc/devdocs/Translations/status.py code docs/doc/devdocs/Translations`.
3. Commit and push the documents as in step 4.4, with the message "Finish the key group <group>".
4. Take the next group (step 2), until the groups that this session did lacked about 400 translations in all. Then
   stop.

## 6. Stop

1. A session that @Pauli started archives its transcript, as the README of the branch `sessions` of
   `pauliojanpera/windows-fonts` tells (`archive-session.sh`). An agent skips this: the session that started it
   archives the transcripts of the machine.
2. Tell @Pauli, or the session that started you, in a few lines: the groups done; how many lines were translated,
   marked not needed, corrected and left empty; and the doubts that he should see. Say "all groups are done" if
   `grouppacket.py` said so.

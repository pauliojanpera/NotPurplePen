# A session that settles the terms

Nov 16, 2026. This is the brief of a session that starts with no context and settles the terms of NotPurplePen's
translations in [terms.csv](terms.csv), before the languages are translated (@Pauli, Nov 16). Do the steps in order.
The scripts and the facts behind them are in [README.md](README.md).

@Pauli starts the session with a prompt such as:

> Settle the terms of NotPurplePen's translations. Follow `doc/devdocs/Translations/Terms.md` on the branch
> `docs/process-coordination` of `pauliojanpera/NotPurplePen`. You are `claude-opus-5-5`, at effort `max`.

## What the terms are for

The translation of a key uses the language's term for each term in the English, so that one language says one thing
in one way everywhere. [terms.csv](terms.csv) gives the terms: one line for each term, the English first, then a
column for each language in the order of the language codes. `terms.py` made it on Nov 16 from the existing
translations, so it shows what the languages say now, with their gaps and their disagreements. This session settles
it. After this session, `terms.csv` is kept by hand; `terms.py` writes a fresh extraction only to another file, to
find new terms.

The work is on the 26 translated languages. The columns of ca, he, id, ku, vec and zh-HK stay empty: these languages
are not translated (`resxlib.NOT_TRANSLATED`). On Nov 16, the 26 columns hold 2,624 empty cells (ru 494, sl 331, nl
262, nn-NO 206, ro 206, da 160, pl 134, ja 93, and fewer in the others) and 568 cells with several translations, in
87 terms.

## 1. Get the files

1. Attach `pauliojanpera/NotPurplePen` with push access.
2. Clone the documents, then read [`doc/devdocs/Introduction.md`](../Introduction.md) and [README.md](README.md):

   ```
   git clone --depth 1 --no-checkout -b docs/process-coordination https://github.com/pauliojanpera/NotPurplePen docs
   git -C docs sparse-checkout set --no-cone /doc/devdocs/
   git -C docs checkout
   ```

3. Clone the code of the translations, to read the satellites. Give the command up to ten minutes.

   ```
   git clone --depth 1 -b fix/complete-translations https://github.com/pauliojanpera/NotPurplePen code
   ```

## 2. Settle each term, for all languages at once

Go through `terms.csv` term by term. For each term, look at all its translations together, as the comparison of the
languages shows best what the term means and where a language is wrong.

- **A term with more than one sense** gets one line for each sense, with the sense in brackets in the English column:
  for example "Leg (between two controls)" and "Leg (of a relay)", "Start (of a course)" and "Start (the verb)". Find
  the senses from the keys whose English is the term or holds it: `python3 docs/doc/devdocs/Translations/lookup.py
  code <key>` shows every translation of a key, and `grep` on `code/src/AvPurplePen/UIText.resx` and on PurplePenCore's
  English files finds the keys.
- **A cell with several translations** keeps one: the one that the language uses most, unless it is wrong. Where the
  translations differ because the senses differ, split the term instead.
- **An empty cell** gets the language's term: from the language's own translations of longer texts that hold the term
  (`grep` on its satellites), from the related languages (`resxlib.RELATED`), and from the language's own usage in
  orienteering, the IOF's control descriptions first.
- **A wrong term** is corrected: for example, where a language uses a word of another sense, such as zh-CN 课程 (a
  course of study) for a leg. The existing translations of the language that use the wrong term are corrected later,
  by the translation of the language (Session.md, "A correction").
- Aim at natural language: the term is the word that a speaker of the language uses, not the English put into the
  language's words.
- A term that is not a term of the application, such as a fragment of a sentence, comes out of the file.

Record each decision in `docs/doc/devdocs/Translations/terms-decisions.tsv`, one line for each cell that you set or
change, with the columns `en`, `language`, `value`, `was`, `note`, `translator`, `effort` and `date`: the term, the
language, the new value, the value before, why, and who decided it (your model id, your effort setting, today's
date). `terms.csv` itself stays the plain table.

## 3. Push as you go, and at the end

After about each 50 terms, and at the end, commit `terms.csv` and `terms-decisions.tsv` and push them. Nothing builds
or runs tests for these files, so there is nothing to wait for.

```
git -C docs add doc/devdocs/Translations
git -C docs commit -m "Settle the terms <first>–<last>"
git -C docs -c push.negotiate=true push origin docs/process-coordination
```

At the end:

1. Archive this session's transcript, as the README of the branch `sessions` of `pauliojanpera/windows-fonts` tells
   (`archive-session.sh`). Attach that repository with push access for it.
2. Tell @Pauli, in a few lines: how many terms were split by sense, how many cells were filled, chosen from several,
   and corrected; the terms that came out; and the doubts that he should see.

#!/usr/bin/env python3
"""Makes the review table of filled TSV files, as the review tables of SvgTranslations.md: for each
key, the English, the translation, the context that decided it, and the existing keys it mirrors.

usage: review.py <tsv>... > review.md
       review.py <tsv>... --out-dir FOLDER

The language of a line is its column language, as in a key group's TSV (grouppacket.py), or else
the TSV's file name without ".tsv". With --out-dir, the table of each language goes to
FOLDER/<language>.md, made from all lines of that language in the TSVs given, so that the review of a
language is whole after each key group. Lines without a translation are left out.
A key judged to need no translation ("=") is shown as such, and a correction with the translation that
it replaces. The language model and effort setting of
the translator and effort columns are given under the heading where all lines share them, and in a
column "Made by" where they differ."""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402


def cell(text):
    return (text or '').replace('\r\n', '\n').replace('\n', '<br>').replace('|', '\\|')


def main():
    argv = sys.argv[1:]
    if not argv or argv[0] in ('-h', '--help'):
        sys.exit(__doc__)
    out_dir = None
    if '--out-dir' in argv:
        i = argv.index('--out-dir')
        out_dir = argv[i + 1]
        argv = argv[:i] + argv[i + 2:]
    by_lang = {}
    for path in argv:
        name = os.path.basename(path)[:-len('.tsv')] if path.endswith('.tsv') else os.path.basename(path)
        for r in R.read_tsv(path):
            if r.get('translation', '') != '':
                by_lang.setdefault(r.get('language', '').strip() or name, []).append(r)
    texts = {}
    for lang in sorted(by_lang):
        out = []
        rows = by_lang[lang]
        def made(r):
            translator = r.get('translator', '').strip()
            effort = r.get('effort', '').strip()
            if not translator:
                return 'a person'
            return translator + (f', effort {effort}' if effort else '')
        makers = sorted({made(r) for r in rows})
        out.append(f'#### {lang}')
        out.append('')
        if len(makers) == 1:
            out.append(f'Made by {makers[0]}.')
            out.append('')
            out.append('| Key | English | Translation | Context that decided it | Mirrors |')
            out.append('| --- | --- | --- | --- | --- |')
        else:
            out.append('| Key | English | Translation | Context that decided it | Mirrors | Made by |')
            out.append('| --- | --- | --- | --- | --- | --- |')
        for r in rows:
            key = r['key'] if r['resource'] == 'UIText' else f'{r["resource"]}.{r["key"]}'
            translation = '(not needed)' if r['translation'] == '=' else cell(r['translation'])
            if r.get('old', ''):
                translation += f'<br>(corrects: {cell(r["old"])})'
            line = f'| `{key}` | {cell(r["english"])} | {translation} | {cell(r.get("note", ""))} | {cell(r.get("mirrors", ""))} |'
            if len(makers) > 1:
                line += f' {made(r)} |'
            out.append(line)
        out.append('')
        texts[lang] = out
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
        for lang, out in texts.items():
            with open(os.path.join(out_dir, f'{lang}.md'), 'w', encoding='utf-8', newline='\n') as f:
                f.write(f'# Review of the translations into {lang}\n\nMade by `review.py` from the filled key groups.\n\n'
                        + '\n'.join(out))
        print(f'{len(texts)} languages written to {out_dir}')
    else:
        sys.stdout.write('\n'.join(line for lang in sorted(texts) for line in texts[lang]))


if __name__ == '__main__':
    main()

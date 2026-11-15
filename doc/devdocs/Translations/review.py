#!/usr/bin/env python3
"""Makes the review table of filled TSV files, as the review tables of SvgTranslations.md: for each
key, the English, the translation, the context that decided it, and the existing keys it mirrors.

usage: review.py <tsv>... > review.md

The language of each TSV is its file name without ".tsv". Lines without a translation are left out.
A key judged to need no translation ("=") is shown as such."""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402


def cell(text):
    return (text or '').replace('\r\n', '\n').replace('\n', '<br>').replace('|', '\\|')


def main():
    if len(sys.argv) < 2 or sys.argv[1] in ('-h', '--help'):
        sys.exit(__doc__)
    out = []
    for path in sys.argv[1:]:
        lang = os.path.basename(path)[:-len('.tsv')] if path.endswith('.tsv') else os.path.basename(path)
        rows = [r for r in R.read_tsv(path) if r.get('translation', '') != '']
        if not rows:
            continue
        out.append(f'#### {lang}')
        out.append('')
        out.append('| Key | English | Translation | Context that decided it | Mirrors |')
        out.append('| --- | --- | --- | --- | --- |')
        for r in rows:
            key = r['key'] if r['resource'] == 'UIText' else f'{r["resource"]}.{r["key"]}'
            translation = '(not needed)' if r['translation'] == '=' else cell(r['translation'])
            out.append(f'| `{key}` | {cell(r["english"])} | {translation} | {cell(r.get("note", ""))} | {cell(r.get("mirrors", ""))} |')
        out.append('')
    sys.stdout.write('\n'.join(out))


if __name__ == '__main__':
    main()

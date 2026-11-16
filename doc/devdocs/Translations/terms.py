#!/usr/bin/env python3
"""Makes terms.csv: the terms of the application and their translations in every language, one term
on each line, so that a term and all its translations are found with grep.

usage: terms.py <checkout> <out csv> [--max-words N]

A term is an English text of the six resources (resxlib.RESOURCES) that has at most --max-words words
(default 3) once its access-key mark, its trailing colon or ellipsis and its placeholders are taken
away. The first column is the term, as the English file writes it most often. The other columns are
the languages, in the order of the language codes (resxlib.languages). A cell holds the language's
translations of the term, taken from the same keys and reduced the same way, the most frequent first,
separated by " / ". Several translations in one cell show where the language translates the term in
more than one way. An empty cell is a term that the language does not translate yet.

The file is comma-separated UTF-8 without a byte-order mark, with a header line and a field in quotes
where it holds a comma or a quote."""

import argparse
import collections
import csv
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402

CJK_MARK = re.compile(r'\s*[(（][&_][A-Za-z0-9][)）]')
END = re.compile(r'(\.\.\.|…|⋯|:|：)\s*$')


def reduce(text):
    """The text without its access-key mark, its trailing colon or ellipsis, and outer spaces."""
    t = CJK_MARK.sub('', text or '')
    t = t.replace('&&', '\x00').replace('&', '').replace('\x00', '&')
    t = re.sub(r'(?<=\w)_(?=\w)|^_|(?<=\s)_', '', t)
    t = t.strip()
    t = END.sub('', t).strip()
    return re.sub(r'\s+', ' ', t)


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('out')
    parser.add_argument('--max-words', type=int, default=3)
    args = parser.parse_args()
    checkout = args.checkout
    langs = R.languages(checkout)

    forms = collections.defaultdict(collections.Counter)            # term -> English forms
    found = collections.defaultdict(lambda: collections.defaultdict(collections.Counter))  # term -> lang -> forms
    for res, base in R.RESOURCES:
        english = R.english_texts(checkout, res)
        satellites = {l: R.read_resource(checkout, base, l) or {} for l in langs}
        for key, entry in english.items():
            if '{' in entry.value or not R.has_letter(entry.value):
                continue
            eng = reduce(entry.value)
            if not eng or len(eng.split(' ')) > args.max_words:
                continue
            term = eng.lower()
            forms[term][eng] += 1
            for l in langs:
                own = satellites[l].get(key)
                if own is not None and own.has_text():
                    tr = reduce(own.value)
                    if tr:
                        found[term][l][tr] += 1

    rows = []
    for term in sorted(forms):
        row = [forms[term].most_common(1)[0][0]]
        for l in langs:
            variants = collections.Counter()
            shown = {}
            for tr, n in found[term][l].items():
                variants[tr.lower()] += n
                shown.setdefault(tr.lower(), tr)
            row.append(' / '.join(shown[v] for v, _ in variants.most_common()))
        rows.append(row)
    with open(args.out, 'w', encoding='utf-8', newline='') as f:
        w = csv.writer(f, lineterminator='\n')
        w.writerow(['en'] + langs)
        w.writerows(rows)
    filled = sum(1 for r in rows for c in r[1:] if c)
    several = sum(1 for r in rows for c in r[1:] if ' / ' in c)
    print(f'{len(rows)} terms, {len(langs)} languages: {filled} cells filled, {several} with several translations, '
          f'{len(rows) * len(langs) - filled} empty')


if __name__ == '__main__':
    main()

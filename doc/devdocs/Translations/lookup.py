#!/usr/bin/env python3
"""Shows every translation of keys: what a translator reads to find the exact meaning of an English
text, for a key of its packet or for any other key.

usage: lookup.py <checkout> <key or pattern>... [--resource NAME] [--language L] [--regex]

For each key, gives the English and the comment of the English file, then the value of the key in
each language that translates it, the related languages of --language first, each with the language
model that made it if its comment names one. A key is found in any of the six resources unless
--resource names one. With --regex, each argument is a regular expression that the whole key name
matches, for example "CourseAppearanceDialog_.*BlendPurple.*"."""

import argparse
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('keys', nargs='+')
    parser.add_argument('--resource')
    parser.add_argument('--language')
    parser.add_argument('--regex', action='store_true')
    args = parser.parse_args()
    checkout = args.checkout

    langs = R.languages(checkout)
    resources = [(res, base) for res, base in R.RESOURCES if not args.resource or res == args.resource]
    english = {res: R.english_texts(checkout, res) for res, _ in resources}
    satellites = {(l, res): R.read_resource(checkout, base, l) or {} for l in langs for res, base in resources}
    completeness = {l: sum(1 for res, _ in resources for k in english[res]
                           if k in satellites[(l, res)] and satellites[(l, res)][k].has_text()) for l in langs}
    order = R.related_order(args.language, langs, completeness) if args.language else \
        sorted(langs, key=lambda l: (-completeness[l], l))

    found = 0
    for res, _ in resources:
        for key, entry in english[res].items():
            if args.regex:
                if not any(re.fullmatch(p, key) for p in args.keys):
                    continue
            elif key not in args.keys:
                continue
            found += 1
            print(f'{res}.{key}')
            print(f'  en     {entry.value}')
            if entry.comment:
                print(f'  (comment: {entry.comment})')
            for l in order:
                own = satellites[(l, res)].get(key)
                if own is None or not own.has_text():
                    continue
                maker = R.made_by(own)
                print(f'  {l:6} {own.value}' + (f'   [{maker}]' if maker else ''))
            missing = [l for l in order if not (satellites[(l, res)].get(key) and satellites[(l, res)][key].has_text())]
            if missing:
                print(f'  missing in: {" ".join(missing)}')
            print()
    if not found:
        sys.exit('no such key')


if __name__ == '__main__':
    main()

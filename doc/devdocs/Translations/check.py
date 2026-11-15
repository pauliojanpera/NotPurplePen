#!/usr/bin/env python3
"""Checks the translations of the satellites against the English files.

usage: check.py <checkout> [--language L ...] [--since REV] [--warnings]

Errors, which make the exit code 1:
- a satellite that does not parse, or that gives a name more than once;
- a translation that is empty;
- a translation with a placeholder {n} whose number the English does not have, or with a brace that
  is not part of a placeholder where the English has none: string.Format throws on these.

Warnings, shown with --warnings:
- a placeholder of the English that the translation leaves out;
- an access-key mark (& or _ before a letter) in the English and not in the translation, or the
  other way;
- a space at the start or end that the English and the translation do not share;
- a colon or ellipsis at the end that the English and the translation do not share;
- for a text of a control (a key that ends with _Text or has _Items) whose English has at most 30
  characters, a translation more than 1.6 times as long, plus 4 characters: it may not fit.

--since checks only the translations that are new or changed since a revision: the work of a phase.
Keys that a satellite has and the English file has not are not checked."""

import argparse
import collections
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402

PLACEHOLDER = re.compile(r'\{(\d+)(?:,[^}:]*)?(?::[^}]*)?\}')


def placeholders(text):
    return set(PLACEHOLDER.findall(text or ''))


def stray_braces(text):
    """True if the text has a brace that is not part of a placeholder or of an escaped brace."""
    t = (text or '').replace('{{', '').replace('}}', '')
    t = PLACEHOLDER.sub('', t)
    return '{' in t or '}' in t


def access_mark(text):
    t = (text or '').replace('&&', '').replace('__', '')
    return bool(re.search(r'&\w', t) or re.search(r'(?:^|(?<=\W)|(?<=\w))_\w', t))


def ending(text):
    """'colon' or 'ellipsis' if the text ends with one, in any of their forms; otherwise ''."""
    t = (text or '').rstrip()
    if t.endswith(('...', '…', '⋯')):
        return 'ellipsis'
    if t.endswith((':', '：')):
        return 'colon'
    return ''


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('--language', action='append')
    parser.add_argument('--since')
    parser.add_argument('--warnings', action='store_true')
    args = parser.parse_args()
    checkout = args.checkout
    langs = args.language or R.languages(checkout)

    errors = collections.Counter()
    warnings = collections.Counter()
    checked = collections.Counter()

    def report(level, lang, res, key, message):
        if level == 'ERROR':
            errors[lang] += 1
        else:
            warnings[lang] += 1
            if not args.warnings:
                return
        print(f'{level} {lang} {res}.{key}: {message}')

    for res, base in R.RESOURCES:
        english = R.english_texts(checkout, res)
        for lang in langs:
            path = os.path.join(checkout, f'{base}.{lang}.resx')
            if not os.path.exists(path):
                continue
            try:
                text = R.read_text(path)
                satellite = R.parse_entries(text.encode('utf-8'))
            except Exception as e:  # noqa: BLE001
                report('ERROR', lang, res, '-', f'does not parse: {e}')
                continue
            names = re.findall(r'<data name="([^"]+)"', text[R.body_start(text):])
            for name, count in collections.Counter(names).items():
                if count > 1:
                    report('ERROR', lang, res, name, f'given {count} times')
            old = R.read_resource(checkout, base, lang, rev=args.since) if args.since else None
            for key, entry in satellite.items():
                if entry.kind != 'string' or key not in english:
                    continue
                if args.since and old is not None and key in old and old[key].kind == 'string' and old[key].value == entry.value:
                    continue
                checked[lang] += 1
                eng = english[key].value
                tr = entry.value
                if tr == '':
                    report('ERROR', lang, res, key, 'empty')
                    continue
                extra = placeholders(tr) - placeholders(eng)
                if extra:
                    report('ERROR', lang, res, key, f'placeholder {sorted(extra)} that the English does not have: "{tr}"')
                if stray_braces(tr) and not stray_braces(eng):
                    report('ERROR', lang, res, key, f'a brace outside a placeholder: "{tr}"')
                lost = placeholders(eng) - placeholders(tr)
                if lost:
                    report('WARNING', lang, res, key, f'leaves out placeholder {sorted(lost)}: "{tr}"')
                if access_mark(eng) != access_mark(tr):
                    report('WARNING', lang, res, key, f'access-key mark in {"the English only" if access_mark(eng) else "the translation only"}: "{eng}" / "{tr}"')
                if (eng[:1].isspace(), eng[-1:].isspace()) != (tr[:1].isspace(), tr[-1:].isspace()):
                    report('WARNING', lang, res, key, f'spaces at the ends differ: "{eng}" / "{tr}"')
                if ending(eng) != ending(tr):
                    report('WARNING', lang, res, key, f'the end differs ({ending(eng) or "none"} / {ending(tr) or "none"}): "{eng}" / "{tr}"')
                if (key.endswith('_Text') or '_Items' in key) and len(eng) <= 30 and len(tr) > 1.6 * len(eng) + 4:
                    report('WARNING', lang, res, key, f'{len(tr)} characters against {len(eng)}: "{tr}"')

    for lang in langs:
        if checked[lang] or errors[lang] or warnings[lang]:
            print(f'{lang}: {checked[lang]} checked, {errors[lang]} errors, {warnings[lang]} warnings')
    sys.exit(1 if sum(errors.values()) else 0)


if __name__ == '__main__':
    main()

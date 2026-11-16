#!/usr/bin/env python3
"""Writes the translations of filled TSV files into a language's satellites.

usage: apply.py <checkout> <language or -> <tsv>... [--translator MODEL --effort LEVEL] [--replace] [--dry-run] [--not-needed FOLDER]

With "-" as the language, each line is written for the language of its column language, as in
a key group's TSV (grouppacket.py). A TSV has the columns resource, key, english and translation (packet.py makes it; status.py's and
other columns are ignored). A line whose translation is "=" records the key as not needed in the
language's not-needed file (default: not-needed beside the scripts), with its english and note, and
writes nothing into the satellite. A line is written if its translation is not empty, the key is in the
resource's English file, and its english is the English file's text now: a line made from an older
English is left out. A key that the satellite translates already is left out, unless the line gives
in its old column the satellite's translation exactly (a correction), or --replace is given. A line
whose old is not the satellite's translation now is left out. A null or empty entry of the key is replaced. A new entry goes into the place that the key has
in the English file. A satellite that does not exist is made from the English file's header.

A line whose translator column, or else --translator, names a language model gets the comment
"translator=<model id>; effort=<effort setting>; date=<today>" in its entry (resxlib), with the effort
of its effort column or of --effort. A line without a translator is written without a comment, as a
person's translation. The not-needed file records the same of each key marked "=".

After writing, each satellite is read again and checked: it must parse, have each name once, and give
each written key its translation and comment. --dry-run writes nothing and reports what it would
write."""

import argparse
import collections
import datetime
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402


def entry_names(text):
    """The names of the entries in the body of a .resx text, in order, with repeats."""
    return [m.group('name') for m in R.ENTRY_PATTERN.finditer(text, R.body_start(text))]


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('language')
    parser.add_argument('tsv', nargs='+')
    parser.add_argument('--replace', action='store_true')
    parser.add_argument('--dry-run', action='store_true')
    parser.add_argument('--not-needed', default=None)
    parser.add_argument('--translator', default='', help='the model id of the language model that translated')
    parser.add_argument('--effort', default='', help="the language model's effort setting")
    args = parser.parse_args()
    today = datetime.date.today().isoformat()

    def made_by(row):
        translator = (row.get('translator') or args.translator).strip()
        effort = (row.get('effort') or args.effort).strip()
        return translator, (effort if translator else '')
    checkout = args.checkout
    if args.language != '-' and not R.LANGUAGE_PATTERN.match(args.language):
        sys.exit(f'{args.language}: not a language code')
    per_language = collections.OrderedDict()
    for path in args.tsv:
        for row in R.read_tsv(path):
            if row.get('translation', '') == '':
                continue
            lang = row.get('language', '').strip() if args.language == '-' else args.language
            per_language.setdefault(lang, []).append(row)

    counts = collections.Counter()
    failed = False
    for lang, lang_rows in per_language.items():
        if not R.LANGUAGE_PATTERN.match(lang):
            print(f'{lang!r}: not a language code; {len(lang_rows)} lines left out')
            failed = True
            continue
        wanted = collections.OrderedDict()
        marked = []
        for row in lang_rows:
            if row['translation'] == '=':
                marked.append(row)
                continue
            wanted.setdefault(row['resource'], {})[row['key']] = row
        if marked:
            not_needed = R.read_not_needed(lang, args.not_needed)
            for row in marked:
                english = R.english_texts(checkout, row['resource']) if row['resource'] in R.RESOURCE_PATHS else None
                if english is None or row['key'] not in english or english[row['key']].value != row['english']:
                    print(f'{row["resource"]}.{row["key"]}: not in the English file with this English; not recorded as not needed')
                    counts['English changed'] += 1
                    continue
                translator, effort = made_by(row)
                not_needed[(row['resource'], row['key'])] = {'resource': row['resource'], 'key': row['key'],
                                                             'english': row['english'], 'note': row.get('note', ''),
                                                             'translator': translator, 'effort': effort,
                                                             'date': today if translator else ''}
                counts['not needed'] += 1
            if not args.dry_run:
                R.write_not_needed(lang, not_needed, args.not_needed)
        for res, rows in wanted.items():
            if res not in R.RESOURCE_PATHS:
                print(f'{res}: not a resource; {len(rows)} lines left out')
                counts['unknown resource'] += len(rows)
                continue
            base = R.RESOURCE_PATHS[res]
            english_path = os.path.join(checkout, base + '.resx')
            english_text = R.read_text(english_path)
            english = R.parse_entries(english_text.encode('utf-8'))
            english_order = [n for n, e in english.items() if e.kind == 'string']
            sat_path = os.path.join(checkout, f'{base}.{lang}.resx')
            if os.path.exists(sat_path):
                text = R.read_text(sat_path)
                new_file = False
            else:
                text = R.empty_satellite(english_text)
                new_file = True
            before = R.parse_entries(text.encode('utf-8'))
            names = entry_names(text)
            if len(names) != len(before) or len(set(names)) != len(names):
                print(f'{sat_path}: its entries are not in the form that this script writes, or a name repeats; nothing written')
                failed = True
                continue

            values = {}
            comments = {}
            for key, row in rows.items():
                entry = english.get(key)
                if entry is None or entry.kind != 'string':
                    print(f'{res}.{key}: not in the English file; left out')
                    counts['not in English'] += 1
                    continue
                if row['english'] != entry.value:
                    print(f'{res}.{key}: the English is now "{entry.value}", not "{row["english"]}"; left out')
                    counts['English changed'] += 1
                    continue
                own = before.get(key)
                old = row.get('old', '')
                if own is not None and own.has_text():
                    if old != '':
                        if own.value != old:
                            print(f'{res}.{key}: the translation is now "{own.value}", not "{old}"; left out')
                            counts['old changed'] += 1
                            continue
                        counts['corrected'] += 1
                    elif not args.replace:
                        counts['translated already'] += 1
                        continue
                elif old != '':
                    print(f'{res}.{key}: the satellite has no translation to correct; written as a new one')
                values[key] = row['translation']
                translator, effort = made_by(row)
                if translator:
                    comments[key] = R.translator_comment(translator, effort, today)
                    counts['by a model'] += 1
            if not values:
                continue
            counts['written'] += len(values)
            if args.dry_run:
                print(f'{sat_path}: would write {len(values)}' + (' (new file)' if new_file else ''))
                continue
            text = R.set_entries(text, english_order, values, comments)
            after = R.parse_entries(text.encode('utf-8'))
            names = entry_names(text)
            problems = [n for n, c in collections.Counter(names).items() if c > 1]
            problems += [k for k, v in values.items() if after.get(k) is None or after[k].value != v
                         or (after[k].comment or None) != comments.get(k)]
            if problems:
                print(f'{sat_path}: check failed for {problems[:5]}; nothing written')
                failed = True
                counts['written'] -= len(values)
                continue
            R.write_text(sat_path, text)
            print(f'{sat_path}: wrote {len(values)}' + (' (new file)' if new_file else ''))

    print(', '.join(f'{k} {v}' for k, v in counts.items()) or 'nothing to write')
    sys.exit(1 if failed else 0)


if __name__ == '__main__':
    main()

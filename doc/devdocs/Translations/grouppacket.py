#!/usr/bin/env python3
"""Makes the packet of one key group, for all translated languages at once (@Pauli, Nov 16).

usage: grouppacket.py <checkout> <out folder> [--group NAME] [--tsv PATH] [--translator MODEL --effort LEVEL]
       grouppacket.py <checkout> --list

The groups are resxlib.key_groups: the dialogs of UIText, then the resources of PurplePenCore with
their copies in UIText, cut into parts of at most resxlib.GROUP_SIZE keys. Without --group, the
packet is of the first group, in their order, that has no review in reviews/groups/ and lacks a
translation in a translated language; the script prints its name, or "all groups are done". --list
prints every group with its keys, the translations it lacks, and whether it is done.

Writes <out folder>/<group>.md to read, and a TSV to fill: <out folder>/<group>.tsv, or PATH. If PATH
exists, its lines stay as they are, and the packet adds the lines that it lacks. The TSV has a line
for each language and key that the language lacks, with the columns language, resource, key,
english, translation, old, note, mirrors, translator and effort.

For each key that a language lacks, the Markdown gives the English and its comment, the copy or the
original of the key, the other keys of the same control, the key's translation in every translated
language that has one, the languages that lack it, the terms of terms.csv that the English holds with
their values in these languages, and the translations of the same text under other keys in these
languages."""

import argparse
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402
import packet as P  # noqa: E402

HEADER = ['language', 'resource', 'key', 'english', 'translation', 'old', 'note', 'mirrors', 'translator', 'effort']


def same_index(checkout, lang, english):
    """The language's translations by normalized English: from its satellites, the WinForms forms'
    satellites and upstream's PO file."""
    index = {}

    def add(label, eng, tr):
        index.setdefault(R.normalize(eng), []).append((label, tr))

    for res, base in R.RESOURCES:
        sat = R.read_resource(checkout, base, lang) or {}
        for key, entry in english[res].items():
            if key in sat and sat[key].has_text():
                add(f'{res}.{key}', entry.value, sat[key].value)
    for label, eng, tr in P.winforms_sources(checkout, lang):
        add(label, eng, tr)
    for refs, eng, tr in R.read_po(checkout, lang):
        add(f'{R.PO_DIR}/{R.PO_NAMES.get(lang, lang)}.po ' + (refs[0].replace(' ', '') if refs else ''), eng, tr)
    return index


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('out', nargs='?')
    parser.add_argument('--group')
    parser.add_argument('--tsv')
    parser.add_argument('--translator', default='')
    parser.add_argument('--effort', default='')
    parser.add_argument('--list', action='store_true')
    args = parser.parse_args()
    checkout = args.checkout
    if not args.list and not args.out:
        parser.error('give the out folder, or --list')

    langs = R.translated_languages(checkout)
    english = {res: R.english_texts(checkout, res) for res, _ in R.RESOURCES}
    sats = {(l, res): R.read_resource(checkout, base, l) or {} for l in langs for res, base in R.RESOURCES}
    not_needed = {l: R.read_not_needed(l) for l in langs}

    def lacking(res, key):
        entry = english[res][key]
        return [l for l in langs if R.missing_keys({key: entry}, sats[(l, res)], res, not_needed[l])]

    groups = R.key_groups(checkout)
    if args.list:
        for name, keys in groups:
            cells = sum(len(lacking(r, k)) for r, k in keys)
            print(f'{name}\t{len(keys)} keys\t{cells} missing\t{"done" if R.group_done(name) else ""}')
        return
    chosen = None
    for name, keys in groups:
        if args.group:
            if name == args.group:
                chosen = (name, keys)
                break
        elif not R.group_done(name) and any(lacking(r, k) for r, k in keys):
            chosen = (name, keys)
            break
    if chosen is None:
        if args.group:
            sys.exit(f'{args.group}: no such group')
        print('all groups are done')
        return
    name, keys = chosen

    terms = sorted(R.read_terms(), key=lambda t: -len(t[0]))
    indexes = {}
    controls = {}
    for key in english['UIText']:
        c = R.control_of(key)
        if c is not None:
            controls.setdefault(c, []).append(key)
    commit = subprocess.run(['git', '-C', checkout, 'rev-parse', '--short=8', 'HEAD'],
                            capture_output=True, text=True).stdout.strip()

    body = []
    rows = []
    cells = 0
    missing_langs = set()
    for res, key in keys:
        lack = lacking(res, key)
        if not lack:
            continue
        cells += len(lack)
        missing_langs.update(lack)
        entry = english[res][key]
        body.append(f'#### `{res}.{key}`')
        body.append(f'- English: {P.md_text(entry.value)}')
        if entry.comment:
            body.append(f'- Comment: {P.md_text(entry.comment)}')
        if res == 'UIText' and key.split('_')[0] in R.TWIN_RESOURCES:
            body.append(f'- Copy of `{key.split("_")[0]}.{key.split("_", 1)[1]}`: give it the same translation.')
        if res == 'UIText':
            control = R.control_of(key)
            siblings = [k for k in controls.get(control, []) if k != key] if control else []
            if siblings:
                body.append('- Same control: ' + '; '.join(
                    f'`{k}` "{P.md_text(english["UIText"][k].value)}"' for k in siblings))
        have = [f'{l} "{P.md_text(sats[(l, res)][key].value)}"' for l in langs
                if key in sats[(l, res)] and sats[(l, res)][key].has_text()]
        body.append('- Translations: ' + ('; '.join(have) if have else 'none'))
        body.append('- Missing in: ' + ' '.join(lack))
        norm = R.normalize(entry.value)
        found = []
        for term, label, values in terms:
            if len(term) >= 3 and re.search(r'(?<!\w)' + re.escape(term) + r'(?!\w)', norm):
                shown = [f'{l} {values[l]}' for l in lack if values.get(l)]
                if shown:
                    found.append(f'`{label}` → ' + ' · '.join(shown))
            if len(found) >= 6:
                break
        if found:
            body.append('- Terms: ' + '; '.join(found))
        same_lines = []
        mirrors = {}
        for l in lack:
            if l not in indexes:
                indexes[l] = same_index(checkout, l, english)
            matches = [m for m in indexes[l].get(norm, []) if m[0] != f'{res}.{key}'][:2]
            if matches:
                same_lines.append(f'{l} ' + ', '.join(f'`{lab}` "{P.md_text(tr)}"' for lab, tr in matches))
                mirrors[l] = ' '.join(lab.split(' ')[-1] for lab, _ in matches)
        if same_lines:
            body.append('- Same text: ' + '; '.join(same_lines))
        body.append('')
        for l in lack:
            rows.append([l, res, key, entry.value, '', '', '', mirrors.get(l, ''), args.translator, args.effort])

    os.makedirs(args.out, exist_ok=True)
    head = [f'# Key group packet: {name}', '',
            f'`grouppacket.py` made this packet from {commit}: {len(keys)} keys, {cells} translations missing in '
            f'{len(missing_langs)} languages ({" ".join(sorted(missing_langs))}).', '',
            'Fill the TSV: for each line, a language and a key that it lacks, the translation, the context that '
            'decided it (note), and the existing keys whose translation it follows (mirrors). Before you translate a '
            'key, find the exact meaning of its English from all its translations and from the other keys of the '
            'same control. Use the terms that the packet gives, and aim at natural language. Write `=` for a key '
            'that needs no translation in a language, and say why in note. Leave the translation empty where the '
            'meaning stays unclear, and say why in note. To correct an existing translation that you are completely '
            'sure is wrong, add a line with the existing translation in `old`. In the TSV, write a line break as \\n '
            'and a tab as \\t. `lookup.py <checkout> <key>` shows every translation of any other key.', '',
            '## Keys', '']
    with open(os.path.join(args.out, f'{name}.md'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(head + body))
    tsv_path = args.tsv or os.path.join(args.out, f'{name}.tsv')
    kept = []
    if args.tsv and os.path.exists(tsv_path):
        kept = [[r.get(h, '') for h in HEADER] for r in R.read_tsv(tsv_path)]
        have = {(r[0], r[1], r[2]) for r in kept}
        rows = [r for r in rows if (r[0], r[1], r[2]) not in have]
    R.write_tsv(tsv_path, HEADER, kept + rows)
    print(f'{name}\t{len(keys)} keys\t{cells} missing\t{len(rows)} lines added to {tsv_path}')


if __name__ == '__main__':
    main()

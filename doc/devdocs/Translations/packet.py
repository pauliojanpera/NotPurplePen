#!/usr/bin/env python3
"""Makes the work packet of one language: what its translator reads, and the TSV that the
translator fills.

usage: packet.py <checkout> <language> <out folder> [--tsv PATH] [--others N] [--resource NAME ...] [--forks-own | --upstream-only] [--upstream REV] [--translator MODEL --effort LEVEL]

Writes <out folder>/<language>.md and <out folder>/<language>.tsv. With --tsv, the TSV goes to PATH
instead; if PATH exists, its lines stay as they are, and the packet adds a line for each key that
PATH does not have, so that a translator who stopped can go on from its filled file. The TSV has a line for each key
that the language lacks, with the columns resource, key, english, translation, old, note, mirrors,
translator and effort. The translator fills translation, writes in note the context that decided it,
and keeps in mirrors the existing keys whose translation it follows. To correct an existing
translation that it is completely sure is wrong, the translator adds a line for the key with the
existing translation in old (@Pauli, Nov 16). translator and effort name the
language model and its effort setting that make the translations: --translator and --effort fill
them in. apply.py writes the filled lines into the satellites, and review.py makes the review table
of them.

For each key, the Markdown gives:
- the English, and the comment of the English file if it has one;
- the same control: the other keys of the dialog whose control has the same name without the words
  of control types (a label and the combo box it names, and the combo box's items), with their
  translation in the language;
- the same text in the language: the translations that the language has of other keys whose English
  is the same text, without access-key marks, a trailing colon or ellipsis, and case. They come from
  the language's satellites, from the satellites that upstream's TranslateTool left in src/PurplePen,
  from the satellites of the WinForms forms, and from upstream's PO files in loc/po/purplepen, which
  keep the English of strings that the .resx files have since changed or dropped. mirrors starts
  with these keys;
- the key in other languages: every language that translates it, the related languages first
  (resxlib.RELATED), then the others by how much they translate. --others N shows only the first N.
  lookup.py shows the same for any key while the translator works.

Before the keys, "Terms" lists the language's existing translations of English texts of up to three
words, for the terms of its own translations.

--forks-own takes only the keys that --upstream (default origin/base) does not have, and
--upstream-only only the others."""

import argparse
import glob
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402


def winforms_sources(checkout, lang):
    """The translations of the WinForms forms' satellites in src/PurplePen: (label, English,
    translation) for each text that the language's satellite of a form gives."""
    result = []
    for neutral in sorted(glob.glob(os.path.join(checkout, R.STRANDED_DIR, '*.resx'))):
        name = os.path.basename(neutral)[:-len('.resx')]
        if '.' in name:
            continue
        sat_path = os.path.join(checkout, R.STRANDED_DIR, f'{name}.{lang}.resx')
        if not os.path.exists(sat_path):
            continue
        try:
            with open(neutral, 'rb') as f:
                english = R.parse_entries(f.read())
            with open(sat_path, 'rb') as f:
                satellite = R.parse_entries(f.read())
        except Exception:
            continue
        for key, entry in satellite.items():
            if not entry.has_text() or key not in english or not english[key].has_text():
                continue
            if not (key.endswith('.Text') or '.Items' in key or key.endswith('.ToolTipText') or '.' not in key):
                continue
            result.append((f'{R.STRANDED_DIR}/{name}.{lang}.resx {key}', english[key].value, entry.value))
    return result


def md_text(text):
    """A text in a Markdown table cell or line: line breaks shown as ⏎, pipes escaped."""
    return (text or '').replace('\r\n', '\n').replace('\n', ' ⏎ ').replace('|', '\\|')


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('language')
    parser.add_argument('out')
    parser.add_argument('--others', type=int, default=0, help='show only the first N other languages (0: all)')
    parser.add_argument('--resource', action='append')
    parser.add_argument('--translator', default='', help='the model id of the language model that translates')
    parser.add_argument('--effort', default='', help="the language model's effort setting")
    parser.add_argument('--upstream', default='origin/base')
    parser.add_argument('--tsv', help='the TSV to write or to add the new keys to (default: <out folder>/<language>.tsv)')
    which = parser.add_mutually_exclusive_group()
    which.add_argument('--forks-own', action='store_true')
    which.add_argument('--upstream-only', action='store_true')
    args = parser.parse_args()
    checkout = args.checkout
    lang = args.language

    all_langs = R.languages(checkout)
    if lang not in all_langs:
        sys.exit(f'{lang}: no satellite of any resource')
    resources = [(res, base) for res, base in R.RESOURCES if not args.resource or res in args.resource]

    english = {res: R.english_texts(checkout, res) for res, _ in R.RESOURCES}
    upstream = None
    if args.forks_own or args.upstream_only:
        upstream = {res: R.english_texts(checkout, res, rev=args.upstream) or {} for res, _ in R.RESOURCES}

    satellites = {}
    completeness = {}
    for other in all_langs:
        count = 0
        for res, base in R.RESOURCES:
            sat = R.read_resource(checkout, base, other) or {}
            satellites[(other, res)] = sat
            count += sum(1 for k in english[res] if k in sat and sat[k].has_text())
        completeness[other] = count
    order = R.related_order(lang, all_langs, completeness)

    # The language's texts by normalized English, for "same text" and "Terms".
    same = {}

    def add_same(label, eng, translation):
        same.setdefault(R.normalize(eng), []).append((label, eng, translation))

    for res, base in R.RESOURCES:
        sat = satellites[(lang, res)]
        for key, entry in english[res].items():
            if key in sat and sat[key].has_text():
                add_same(f'{res}.{key}', entry.value, sat[key].value)
        if not base.startswith('src/AvPurplePen/'):
            stranded = R.read_resource(checkout, base, lang, directory=R.STRANDED_DIR) or {}
            for key, entry in english[res].items():
                if key in stranded and stranded[key].has_text() and not (key in sat and sat[key].has_text()):
                    add_same(f'{R.STRANDED_DIR}/{os.path.basename(base)}.{lang}.resx {key}', entry.value, stranded[key].value)
    for label, eng, translation in winforms_sources(checkout, lang):
        add_same(label, eng, translation)
    for refs, eng, translation in R.read_po(checkout, lang):
        add_same(f'{R.PO_DIR}/{R.PO_NAMES.get(lang, lang)}.po ' + (refs[0].replace(' ', '') if refs else ''), eng, translation)

    controls = {}
    for key in english['UIText']:
        control = R.control_of(key)
        if control is not None:
            controls.setdefault(control, []).append(key)

    rows = []
    sections = []
    not_needed = R.read_not_needed(lang)
    for res, base in resources:
        sat = satellites[(lang, res)]
        missing = R.missing_keys(english[res], sat, res, not_needed)
        if upstream is not None:
            missing = [k for k in missing if (k not in upstream[res]) == args.forks_own]
        groups = {}
        for key in missing:
            group = key.split('_')[0] if res == 'UIText' and '_' in key else res
            groups.setdefault(group, []).append(key)
        for group, keys in groups.items():
            body = [f'### {res}: {group}' if group != res else f'### {res}', '']
            for key in keys:
                entry = english[res][key]
                body.append(f'#### `{key}`')
                body.append(f'- English: {md_text(entry.value)}')
                if entry.comment:
                    body.append(f'- Comment: {md_text(entry.comment)}')
                control = R.control_of(key) if res == 'UIText' else None
                if control is not None:
                    siblings = []
                    for other_key in controls.get(control, []):
                        if other_key == key:
                            continue
                        own = sat.get(other_key)
                        shown = f'"{md_text(own.value)}"' if own is not None and own.has_text() else '(missing)'
                        siblings.append(f'`{other_key}` "{md_text(english[res][other_key].value)}" → {shown}')
                    if siblings:
                        body.append('- Same control: ' + '; '.join(siblings))
                matches = [m for m in same.get(R.normalize(entry.value), []) if m[0] != f'{res}.{key}']
                if matches:
                    body.append('- Same text in this language: ' + '; '.join(
                        f'`{label}` "{md_text(tr)}"' for label, _, tr in matches[:6]))
                others = []
                for other in order:
                    o = satellites[(other, res)].get(key)
                    if o is not None and o.has_text():
                        others.append(f'{other} "{md_text(o.value)}"')
                    if args.others and len(others) >= args.others:
                        break
                body.append('- Other languages: ' + ('; '.join(others) if others else 'none'))
                body.append('')
                mirrors = ' '.join(label.split(' ')[-1] if ' ' in label else label for label, _, _ in matches[:3])
                rows.append([res, key, entry.value, '', '', '', mirrors, args.translator, args.effort])
            sections.append('\n'.join(body))

    terms = {}
    for norm, items in same.items():
        if 0 < len(norm.split(' ')) <= 3 and norm not in terms:
            terms[norm] = items[0]
    commit = subprocess.run(['git', '-C', checkout, 'rev-parse', '--short=8', 'HEAD'], capture_output=True, text=True).stdout.strip()

    os.makedirs(args.out, exist_ok=True)
    head = [f'# Translation packet: {lang}', '',
            f'`packet.py` made this packet from {commit}. {len(rows)} keys to translate. Fill `{lang}.tsv`: one line '
            'for each key, with the translation, the context that decided it (note), and the existing keys whose '
            'translation it follows (mirrors). For a key that needs no translation in this language, write `=` as '
            'the translation and say why in note: apply.py then records the key as not needed, and writes nothing '
            'into the satellite. Leave the translation empty for a key that is not done. If you are completely sure '
            'that an existing translation of this language is wrong, correct it: add a line for its key with the '
            'English, the existing translation in `old` exactly as the satellite has it (`lookup.py` gives it), the '
            'correction in `translation`, and in `note` why the existing one is wrong. In the TSV, write a line '
            'break as \\n and a tab as \\t.', '',
            f'The other languages are read in this order: {", ".join(order)}. To see every translation of any '
            'key, for example of a key in "Same control" or "mirrors", run `lookup.py <checkout> <key>`.', '',
            '## Terms', '',
            f'The existing translations of English texts of up to three words in {lang}.', '',
            f'| English | {lang} | Key |', '| --- | --- | --- |']
    for norm in sorted(terms):
        label, eng, translation = terms[norm]
        head.append(f'| {md_text(eng)} | {md_text(translation)} | `{label}` |')
    head += ['', '## Keys', '']
    with open(os.path.join(args.out, f'{lang}.md'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(head) + '\n' + '\n'.join(sections))
    header = ['resource', 'key', 'english', 'translation', 'old', 'note', 'mirrors', 'translator', 'effort']
    tsv_path = args.tsv or os.path.join(args.out, f'{lang}.tsv')
    kept = []
    if args.tsv and os.path.exists(tsv_path):
        kept = [[r.get(h, '') for h in header] for r in R.read_tsv(tsv_path)]
        have = {(r[0], r[1]) for r in kept}
        added = [r for r in rows if (r[0], r[1]) not in have]
    else:
        added = rows
    R.write_tsv(tsv_path, header, kept + added)
    print(f'{lang}: {len(rows)} keys in the packet, {len(added)} added to {tsv_path}, {len(terms)} terms')


if __name__ == '__main__':
    main()

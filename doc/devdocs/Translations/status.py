#!/usr/bin/env python3
"""Makes the table of the translations that each language lacks.

usage: status.py <checkout> <out folder> [--upstream REV] [--not-needed FOLDER]

Writes <out folder>/Status.md, the table, and <out folder>/missing/<language>.tsv, the keys that the
language lacks. A translation is missing where the language's satellite of a resource has no value
for a key of the English file, or a null or empty value. A key whose English holds no letter is not
counted, and neither is a key that the language's not-needed file gives with its English of now.

--upstream names the revision whose English files have the keys that are upstream's (default
origin/base). The other keys are the fork's own."""

import argparse
import datetime
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import resxlib as R  # noqa: E402

NAMES = {
    'bg': 'Bulgarian', 'ca': 'Catalan', 'cs': 'Czech', 'da': 'Danish', 'de': 'German',
    'en-GB': 'English (UK)', 'es': 'Spanish', 'et': 'Estonian', 'fi': 'Finnish', 'fr': 'French',
    'he': 'Hebrew', 'hu': 'Hungarian', 'id': 'Indonesian', 'it': 'Italian', 'ja': 'Japanese',
    'ku': 'Kurdish (Kurmanji)', 'nb-NO': 'Norwegian Bokmål', 'nl': 'Dutch', 'nn-NO': 'Norwegian Nynorsk',
    'pl': 'Polish', 'pt': 'Portuguese', 'pt-BR': 'Portuguese (Brazil)', 'ro': 'Romanian', 'ru': 'Russian',
    'sl': 'Slovenian', 'sv': 'Swedish', 'tr': 'Turkish', 'uk': 'Ukrainian', 'vec': 'Venetian',
    'zh-CN': 'Chinese (Simplified)', 'zh-HK': 'Chinese (Hong Kong)', 'zh-TW': 'Chinese (Traditional)',
}


def git(checkout, *args):
    result = subprocess.run(['git', '-C', checkout] + list(args), capture_output=True, text=True)
    return result.stdout.strip() if result.returncode == 0 else ''


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('checkout')
    parser.add_argument('out')
    parser.add_argument('--upstream', default='origin/base')
    parser.add_argument('--not-needed', default=None,
                        help='folder of the not-needed TSV files (default: not-needed beside the scripts)')
    args = parser.parse_args()
    checkout = args.checkout

    english = {res: R.english_texts(checkout, res) for res, _ in R.RESOURCES}
    upstream = {res: R.english_texts(checkout, res, rev=args.upstream) for res, _ in R.RESOURCES}
    have_upstream = all(v is not None for v in upstream.values())
    counted = {res: [k for k, e in english[res].items() if R.has_letter(e.value)] for res, _ in R.RESOURCES}
    langs = R.languages(checkout)

    os.makedirs(os.path.join(args.out, 'missing'), exist_ok=True)
    rows = []
    totals = [0] * (len(R.RESOURCES) + 4)
    for lang in langs:
        not_needed = R.read_not_needed(lang, args.not_needed)
        per_resource = []
        in_repo = 0
        own = 0
        tsv_rows = []
        for res, base in R.RESOURCES:
            satellite = R.read_resource(checkout, base, lang)
            stranded = None
            if not base.startswith('src/AvPurplePen/'):
                stranded = R.read_resource(checkout, base, lang, directory=R.STRANDED_DIR)
            missing = R.missing_keys(english[res], satellite, res, not_needed)
            per_resource.append(len(missing))
            for key in missing:
                found = ''
                if stranded is not None and key in stranded and stranded[key].has_text():
                    found = f'{R.STRANDED_DIR}/{os.path.basename(base)}.{lang}.resx'
                    in_repo += 1
                is_own = have_upstream and key not in upstream[res]
                if is_own:
                    own += 1
                tsv_rows.append([res, key, english[res][key].value, found, 'yes' if is_own else ''])
        total = sum(per_resource)
        judged = sum(1 for (res, key), row in not_needed.items()
                     if res in english and key in english[res] and english[res][key].value == row['english'])
        R.write_tsv(os.path.join(args.out, 'missing', f'{lang}.tsv'),
                    ['resource', 'key', 'english', 'in_repo', 'forks_own'], tsv_rows)
        rows.append((lang, per_resource, total, judged, in_repo, own))
        for i, v in enumerate(per_resource + [total, judged, in_repo, own]):
            totals[i] += v

    all_counted = sum(len(v) for v in counted.values())
    commit = git(checkout, 'rev-parse', '--short=8', 'HEAD')
    commit_date = git(checkout, 'log', '-1', '--format=%cd', '--date=short')
    short = {'CommandNameText': 'Command', 'StatusBarText': 'Status bar',
             'SelectionDescriptionText': 'Selection', 'ReportText': 'Report'}

    lines = []
    lines.append('# Missing translations')
    lines.append('')
    lines.append(f'`status.py` made this table from {commit} ({commit_date}) on {datetime.date.today().isoformat()}. '
                 'Make it again after each phase (README.md).')
    lines.append('')
    lines.append('A translation is missing where the satellite of a language has no value for a key of the English '
                 'file, or a null or empty value. A key whose English holds no letter is not counted, and neither is a '
                 "key that the language's translator judged to need no translation ([`not-needed/`](not-needed/)). "
                 'The keys of each language are in [`missing/<language>.tsv`](missing/).')
    lines.append('')
    header = ['Language', 'Done'] + [short.get(res, res) for res, _ in R.RESOURCES] + ['Missing', 'Not needed', 'In the repo']
    if have_upstream:
        header.append("Fork's own")
    lines.append('| ' + ' | '.join(header) + ' |')
    lines.append('|' + '|'.join([' --- '] * 2 + [' ---: '] * (len(header) - 2)) + '|')
    for lang, per_resource, total, judged, in_repo, own in rows:
        done = 100.0 * (all_counted - total) / all_counted
        cells = [f'{NAMES.get(lang, lang)} (`{lang}`)', f'{done:.0f} %'] + [str(v) for v in per_resource] + [f'**{total}**', str(judged), str(in_repo)]
        if have_upstream:
            cells.append(str(own))
        lines.append('| ' + ' | '.join(cells) + ' |')
    cells = ['All', ''] + [str(v) for v in totals[:len(R.RESOURCES)]] + [f'**{totals[-4]}**', str(totals[-3]), str(totals[-2])]
    if have_upstream:
        cells.append(str(totals[-1]))
    lines.append('| ' + ' | '.join(cells) + ' |')
    lines.append('')
    lines.append('Keys counted in the English files: ' + ', '.join(f'{res} {len(counted[res])}' for res, _ in R.RESOURCES)
                 + f'; {all_counted} in all.')
    lines.append('')
    lines.append(f'- **Done** is the share of the {all_counted} keys that the language translates or does not need.')
    lines.append("- **Not needed** counts the keys that the language's translator judged to need no translation, "
                 'in `not-needed/<language>.tsv`. They are not in **Missing**.')
    lines.append(f'- **In the repo** counts the missing translations that a satellite in `{R.STRANDED_DIR}` holds. '
                 "Upstream's TranslateTool writes the satellites of PurplePenCore's resources there. PurplePen.csproj "
                 'removes them from its build, and PurplePenCore has no copy, so the application does not show them.')
    if have_upstream:
        lines.append(f"- **Fork's own** counts the missing translations of keys that `{args.upstream}` does not have. "
                     'They are translated on the branch that adds the key.')
    lines.append('')
    with open(os.path.join(args.out, 'Status.md'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines))
    print(f'{len(langs)} languages, {totals[-4]} missing, {totals[-3]} not needed, {totals[-2]} in the repo'
          + (f", {totals[-1]} fork's own" if have_upstream else ''))


if __name__ == '__main__':
    main()

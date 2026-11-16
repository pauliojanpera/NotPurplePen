#!/usr/bin/env python3
"""Makes the Translation Logbook, the page of the translation work on the site, as the SVG Logbook is the page
of the SVG target: https://pauliojanpera.github.io/NotPurplePen/translations/ (@Pauli, Nov 16).

usage: logbook.py <checkout> <out folder>

<checkout> is a checkout of fix/complete-translations, as Session.md's `code`. The page shows:

- the plan, drawn as an orienteering course: the steps of logbook.json's "plan". A step is done when it has a
  date in "done". The step of phase 3 is measured from the key groups, and is done when each group has its review.
- where the work stands: the key groups done, and for each translated language the translations that it lacks
  and those that a language model made, counted in the checkout's satellites as status.py counts them.
- the log, newest first: each key group that has its review, with the summary that its session wrote above the
  review and the figures of its filled TSV, and the entries of logbook.json's "log".

Writes <out folder>/index.html. publish-logbook.sh puts the folder on the site, with the fonts of the SVG Logbook.
"""

import datetime
import html
import json
import os
import re
import subprocess
import sys
from urllib.parse import urljoin

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import resxlib as R  # noqa: E402

try:
    from status import NAMES  # noqa: E402
except Exception:  # The page names a language by its code where no name is known.
    NAMES = {}

DOCS_URL = 'https://github.com/pauliojanpera/NotPurplePen/blob/docs/process-coordination/doc/devdocs/Translations/'
COMMIT_URL = 'https://github.com/pauliojanpera/NotPurplePen/commit/'
DATE = re.compile(r'(?:^|;\s*)date=(\d{4}-\d{2}-\d{2})')


def git(folder, *args):
    """The output of a git command in a folder, or '' when it fails."""
    result = subprocess.run(['git', '-C', folder] + list(args), capture_output=True, text=True)
    return result.stdout.strip() if result.returncode == 0 else ''


def nice_date(iso):
    """'2026-11-16' as 'Nov 16, 2026'."""
    try:
        d = datetime.date.fromisoformat(iso)
    except ValueError:
        return iso
    return f'{d.strftime("%b")} {d.day}, {d.year}'


def esc(text):
    return html.escape(str(text), quote=True)


# --- Markdown, as much of it as the summaries use ----------------------------------------------------------------

def link(target, base_url):
    """A link of the Markdown, made absolute against the URL of its file."""
    target = html.unescape(target)
    if re.match(r'[a-z][a-z0-9+.-]*:', target) or target.startswith('#'):
        return esc(target)
    return esc(urljoin(base_url, target))


def inline(text, base_url):
    """Code spans, bold and links of one line of Markdown, as HTML."""
    out = []
    for part in re.split(r'(`[^`]*`)', text):
        if len(part) >= 2 and part.startswith('`') and part.endswith('`'):
            out.append(f'<code>{esc(part[1:-1])}</code>')
            continue
        s = html.escape(part, quote=False)
        s = re.sub(r'\*\*(.+?)\*\*', r'<strong>\1</strong>', s)
        s = re.sub(r'\[([^\]]+)\]\(([^)\s]+)\)', lambda m: f'<a href="{link(m.group(2), base_url)}">{m.group(1)}</a>', s)
        out.append(s)
    return ''.join(out)


def markdown(text, base_url):
    """Paragraphs, lists and headings of Markdown, as HTML. Tables are left out: the review links to them."""
    out = []
    for block in re.split(r'\n\s*\n', text.strip()):
        lines = [l for l in block.split('\n') if l.strip()]
        if not lines or lines[0].lstrip().startswith('|'):
            continue
        if lines[0].startswith('#'):
            out.append(f'<h4>{inline(lines[0].lstrip("#").strip(), base_url)}</h4>')
            lines = lines[1:]
            if not lines:
                continue
        if lines[0].lstrip().startswith(('- ', '* ')):
            items = []
            for l in lines:
                if l.lstrip().startswith(('- ', '* ')):
                    items.append(l.lstrip()[2:].strip())
                elif items:
                    items[-1] += ' ' + l.strip()
            out.append('<ul>' + ''.join(f'<li>{inline(i, base_url)}</li>' for i in items) + '</ul>')
        else:
            out.append(f'<p>{inline(" ".join(l.strip() for l in lines), base_url)}</p>')
    return '\n'.join(out)


# --- What the checkout and the documents hold --------------------------------------------------------------------

def standing(checkout):
    """For each translated language: the translations that it lacks, those that a language model made, and the
    date of the newest of them; with the number of keys counted and the satellites, by language and resource."""
    english = {res: R.english_texts(checkout, res) for res, _ in R.RESOURCES}
    counted = {res: {k for k, e in english[res].items() if R.has_letter(e.value)} for res, _ in R.RESOURCES}
    total = sum(len(v) for v in counted.values())
    rows = []
    satellites = {}
    for lang in R.translated_languages(checkout):
        not_needed = R.read_not_needed(lang)
        missing = by_model = 0
        newest = ''
        for res, base in R.RESOURCES:
            satellite = R.read_resource(checkout, base, lang) or {}
            satellites[(lang, res)] = satellite
            missing += len(R.missing_keys(english[res], satellite, res, not_needed))
            for key in counted[res]:
                entry = satellite.get(key)
                if entry is not None and entry.has_text() and R.made_by(entry):
                    by_model += 1
                    m = DATE.search(entry.comment or '')
                    if m and m.group(1) > newest:
                        newest = m.group(1)
        rows.append({'lang': lang, 'missing': missing, 'by_model': by_model, 'newest': newest,
                     'done': 1 - missing / total if total else 1})
    return rows, total, satellites


def group_entries(checkout, satellites, today):
    """The log entries of the key groups that have their review, and the number of groups done and in all."""
    groups = R.key_groups(checkout)
    entries = []
    done = 0
    for seq, (name, _keys) in enumerate(groups):
        review = os.path.join(R.REVIEWS_DIR, 'groups', f'{name}.md')
        if not os.path.exists(review):
            continue
        done += 1
        text = R.read_text(review)
        cut = re.search(r'^#### ', text, re.M)
        summary = text[:cut.start()] if cut else text
        tsv = os.path.join(HERE, 'filled', 'groups', f'{name}.tsv')
        rows = R.read_tsv(tsv) if os.path.exists(tsv) else []
        made = sorted({r.get('translator', '').strip() + (f', effort {r["effort"].strip()}' if r.get('effort', '').strip() else '')
                       for r in rows if r.get('translator', '').strip()})
        date = ''
        for r in rows:
            entry = satellites.get((r.get('language', ''), r.get('resource', '')), {}).get(r.get('key', ''))
            m = DATE.search(entry.comment or '') if entry is not None else None
            if m and m.group(1) > date:
                date = m.group(1)
        figures = {
            'translated': sum(1 for r in rows if r.get('translation', '') not in ('', '=') and not r.get('old', '')),
            'corrected': sum(1 for r in rows if r.get('translation', '') not in ('', '=') and r.get('old', '')),
            'not needed': sum(1 for r in rows if r.get('translation', '') == '='),
            'left empty': sum(1 for r in rows if r.get('translation', '') == ''),
        }
        languages = sorted({r['language'] for r in rows if r.get('translation', '') not in ('', '=')})
        entries.append({'kind': 'group', 'seq': seq, 'date': date or today, 'title': f'Key group {name}',
                        'by': '; '.join(made), 'figures': figures, 'languages': languages,
                        'html': markdown(summary, DOCS_URL + 'reviews/groups/'),
                        'links': [(f'The review of {name}', DOCS_URL + f'reviews/groups/{name}.md')]})
    return entries, done, len(groups)


def terms_count():
    """The number of terms in terms.csv, and of languages whose cells it fills."""
    import csv
    path = os.path.join(HERE, 'terms.csv')
    if not os.path.exists(path):
        return 0, 0
    with open(path, encoding='utf-8') as f:
        rows = list(csv.reader(f))
    langs = [l for i, l in enumerate(rows[0][1:], 1) if any(r[i] for r in rows[1:] if len(r) > i)]
    return len(rows) - 1, len([l for l in langs if l not in R.NOT_TRANSLATED])


# --- The page -----------------------------------------------------------------------------------------------------

def wrap(text, width):
    """The words of a text in lines of at most about width characters."""
    lines, line = [], ''
    for word in text.split():
        if line and len(line) + 1 + len(word) > width:
            lines.append(line)
            line = word
        else:
            line = f'{line} {word}'.strip()
    return lines + [line] if line else lines


def course_svg(plan, states):
    """The plan drawn as an orienteering course: a start triangle, a control for each step but the last, and the
    last step as the finish. states gives each step's class and the line under its name."""
    count = len(plan)
    width, top, low = 1000, 112, 178
    xs = [96 + i * (width - 192) / count for i in range(count + 1)]
    ys = [low if i % 2 == 0 else top for i in range(count + 1)]
    radius = [24] + [19] * (count - 1) + [22]
    parts = []
    for i in range(count):
        x1, y1, x2, y2 = xs[i], ys[i], xs[i + 1], ys[i + 1]
        dx, dy = x2 - x1, y2 - y1
        length = (dx * dx + dy * dy) ** 0.5
        a, b = (radius[i] + 4) / length, (radius[i + 1] + 4) / length
        cls = 'leg' + (' is-upcoming' if states[i][0] == 'is-upcoming' else '')
        parts.append(f'<line class="{cls}" x1="{x1 + dx * a:.1f}" y1="{y1 + dy * a:.1f}" '
                     f'x2="{x2 - dx * b:.1f}" y2="{y2 - dy * b:.1f}"/>')
    # The start triangle points to the first control.
    x, y = xs[0], ys[0]
    import math
    angle = math.atan2(ys[1] - y, xs[1] - x)
    corners = [(x + 24 * math.cos(angle + k * 2 * math.pi / 3), y + 24 * math.sin(angle + k * 2 * math.pi / 3))
               for k in range(3)]
    parts.append('<g class="is-done"><polygon class="symbol" points="'
                 + ' '.join(f'{px:.1f},{py:.1f}' for px, py in corners) + '"/>'
                 + label(x, y, ['Start'], 'Upstream\'s translations', 24, y == top) + '</g>')
    for i, step in enumerate(plan, 1):
        x, y = xs[i], ys[i]
        cls, state = states[i - 1]
        g = [f'<g class="{cls}">']
        if cls == 'is-current':
            g.append(f'<circle class="halo" cx="{x:.1f}" cy="{y}" r="{radius[i]}"/>')
        if i < count:
            g.append(f'<circle class="symbol" cx="{x:.1f}" cy="{y}" r="19"/>')
            g.append(f'<text class="number" x="{x + 20:.1f}" y="{y - 17}">{i}</text>')
        else:
            g.append(f'<circle class="symbol" cx="{x:.1f}" cy="{y}" r="14"/>'
                     f'<circle class="symbol" cx="{x:.1f}" cy="{y}" r="22"/>')
        g.append(label(x, y, wrap(step['short'], 15), state, radius[i], y == top))
        g.append('</g>')
        parts.append(''.join(g))
    return (f'<svg viewBox="0 0 {width} 290" role="img" aria-label="The plan of the translation work, drawn as a '
            f'course">' + ''.join(parts) + '</svg>')


def label(x, y, lines, state, radius, above):
    """The name of a step and its state, above the symbol of an upper step and below that of a lower one."""
    step = 15
    if above:
        first = y - radius - 14 - step * len(lines)
    else:
        first = y + radius + 20
    out = [f'<text class="name" x="{x:.1f}" y="{first}" text-anchor="middle">']
    for k, line in enumerate(lines):
        out.append(f'<tspan x="{x:.1f}" dy="{0 if k == 0 else step}">{esc(line)}</tspan>')
    out.append('</text>')
    if state:
        out.append(f'<text class="state" x="{x:.1f}" y="{first + step * len(lines)}" text-anchor="middle">'
                   f'{esc(state)}</text>')
    return ''.join(out)


STYLE = """
@font-face { font-family: "Barlow"; font-style: normal; font-weight: 400; font-display: swap; src: url("fonts/barlow-latin-400-normal.woff2") format("woff2"); }
@font-face { font-family: "Barlow"; font-style: normal; font-weight: 500; font-display: swap; src: url("fonts/barlow-latin-500-normal.woff2") format("woff2"); }
@font-face { font-family: "Barlow"; font-style: normal; font-weight: 600; font-display: swap; src: url("fonts/barlow-latin-600-normal.woff2") format("woff2"); }
@font-face { font-family: "Barlow Condensed"; font-style: normal; font-weight: 600; font-display: swap; src: url("fonts/barlow-condensed-latin-600-normal.woff2") format("woff2"); }
@font-face { font-family: "IBM Plex Mono"; font-style: normal; font-weight: 400; font-display: swap; src: url("fonts/ibm-plex-mono-latin-400-normal.woff2") format("woff2"); }
:root {
  color-scheme: light;
  --paper: #f6f5f8; --sheet: #fdfcfe; --ink: #1b1720; --ink-2: #48424f; --muted: #6c6575;
  --rule: #e3dfe8; --rule-strong: #cbc5d2; --overprint: #cc00ff; --accent: #8f00bd; --bar: #e9dff0;
  --font-display: "Barlow Condensed", "Arial Narrow", "Roboto Condensed", sans-serif;
  --font-body: "Barlow", "Segoe UI", system-ui, -apple-system, sans-serif;
  --font-mono: "IBM Plex Mono", ui-monospace, "SFMono-Regular", Menlo, Consolas, monospace;
}
@media (prefers-color-scheme: dark) {
  :root { color-scheme: dark; --paper: #131016; --sheet: #1c1821; --ink: #ece8f1; --ink-2: #cbc4d4; --muted: #a49cae;
          --rule: #2e2935; --rule-strong: #463f4f; --overprint: #d65cff; --accent: #df7bff; --bar: #34283d; }
}
*, *::before, *::after { box-sizing: border-box; }
body { margin: 0; background: var(--paper); color: var(--ink); font: 16px/1.55 var(--font-body); }
a { color: var(--accent); }
code { font: 13.5px var(--font-mono); }
.page { max-width: 1040px; margin: 0 auto; padding: 32px max(16px, env(safe-area-inset-right, 0px)) 72px max(16px, env(safe-area-inset-left, 0px)); display: grid; gap: 56px; }
h1, h2, h3, h4 { font-family: var(--font-display); font-weight: 600; line-height: 1.1; margin: 0; text-wrap: balance; }
h1 { font-size: clamp(38px, 7vw, 60px); letter-spacing: -.01em; }
h2 { font-size: clamp(28px, 4.4vw, 36px); }
h3 { font-size: clamp(24px, 3.6vw, 30px); }
h4 { font-size: 20px; }
p { margin: 0; }
.eyebrow { font: 500 13px/1.3 var(--font-body); letter-spacing: .1em; text-transform: uppercase; color: var(--muted); }
.meta { color: var(--muted); font-size: 14px; }
.masthead { display: grid; gap: 14px; }
.lede { max-width: 66ch; color: var(--ink-2); font-size: 17px; }
.course { margin: 18px 0 0; display: grid; gap: 10px; }
.course svg { display: block; width: 100%; height: auto; overflow: visible; }
.course .symbol { fill: none; stroke: var(--overprint); stroke-width: 2.5; }
.course .leg { stroke: var(--overprint); stroke-width: 2.5; }
.course .number { fill: var(--overprint); font: 600 19px var(--font-display); }
.course .name { fill: var(--ink); font: 500 13.5px var(--font-body); }
.course .state { fill: var(--muted); font: 500 11px var(--font-body); letter-spacing: .08em; text-transform: uppercase; }
.course .is-upcoming .symbol, .course .is-upcoming .number, .course .leg.is-upcoming { opacity: .38; }
.course .is-upcoming .name { fill: var(--muted); }
.course .is-current .state { fill: var(--accent); }
.course .halo { fill: none; stroke: var(--overprint); stroke-width: 2; opacity: .3; transform-box: fill-box; transform-origin: center; transform: scale(1.45); }
.course figcaption { color: var(--muted); font-size: 14px; max-width: 70ch; }
.standing { display: grid; gap: 22px; }
.tally { display: grid; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); gap: 18px 32px; }
.stat { display: grid; gap: 2px; padding-top: 10px; border-top: 3px solid var(--overprint); }
.stat .value { font: 600 clamp(34px, 5vw, 44px)/1 var(--font-display); }
.stat .what { color: var(--ink-2); font-size: 14px; }
.callout { display: grid; gap: 8px; padding: 18px 20px; background: var(--sheet); border: 1px solid var(--rule); border-left: 3px solid var(--overprint); border-radius: 6px; max-width: 760px; }
.table-scroll { overflow-x: auto; }
table { border-collapse: collapse; font-size: 14px; min-width: 560px; }
th, td { text-align: left; padding: 5px 14px 5px 0; border-bottom: 1px solid var(--rule); vertical-align: middle; }
th { font-weight: 600; color: var(--muted); font-size: 12px; letter-spacing: .06em; text-transform: uppercase; }
td.num, th.num { text-align: right; font-variant-numeric: tabular-nums; }
td.bar { width: 34%; }
.bar span { display: block; height: 8px; border-radius: 4px; background: linear-gradient(to right, var(--overprint) var(--done), var(--bar) var(--done)); }
.log, .entries { display: grid; gap: 40px; }
.entry { display: grid; gap: 12px; padding-top: 22px; border-top: 1px solid var(--rule-strong); max-width: 820px; }
.entry ul { margin: 0; padding-left: 1.2em; display: grid; gap: 6px; }
.figures { display: flex; flex-wrap: wrap; gap: 6px 18px; font-size: 14px; color: var(--ink-2); }
.figures b { font-family: var(--font-display); font-size: 19px; color: var(--ink); }
.links { display: flex; flex-wrap: wrap; gap: 6px 16px; font-size: 14px; }
footer { color: var(--muted); font-size: 14px; display: grid; gap: 8px; max-width: 72ch; padding-top: 22px; border-top: 1px solid var(--rule-strong); }
"""


def page(plan_html, stand_html, log_html, footer_html):
    title = 'NotPurplePen Translation Logbook'
    description = 'How the translation of NotPurplePen into 26 languages goes, step by step.'
    return f"""<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
<title>{title}</title>
<meta name="description" content="{description}">
<meta property="og:type" content="website">
<meta property="og:title" content="{title}">
<meta property="og:description" content="{description}">
<link rel="icon" href="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'%3E%3Ccircle cx='16' cy='16' r='11' fill='none' stroke='%23cc00ff' stroke-width='3.5'/%3E%3C/svg%3E">
<style>{STYLE}</style>
</head>
<body>
<div class="page">
{plan_html}
{stand_html}
{log_html}
{footer_html}
</div>
</body>
</html>
"""


def main():
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    checkout, out = sys.argv[1], sys.argv[2]
    with open(os.path.join(HERE, 'logbook.json'), encoding='utf-8') as f:
        data = json.load(f)
    today = datetime.datetime.now(datetime.timezone.utc).date().isoformat()

    rows, total_keys, satellites = standing(checkout)
    groups, groups_done, groups_all = group_entries(checkout, satellites, today)
    missing = sum(r['missing'] for r in rows)
    by_model = sum(r['by_model'] for r in rows)
    terms, term_langs = terms_count()

    # The plan: the first step that is not done is the current one.
    plan = data['plan']
    states, current_seen = [], False
    for step in plan:
        done = step.get('done', '')
        if step.get('groups'):
            done = done or (today if groups_all and groups_done == groups_all else '')
        if done:
            states.append(('is-done', 'Done ' + nice_date(done).rsplit(',', 1)[0]))
        elif not current_seen:
            current_seen = True
            note = f'{groups_done} of {groups_all} groups' if step.get('groups') else 'Now'
            states.append(('is-current', note))
        else:
            states.append(('is-upcoming', ''))
    steps_text = '; '.join(f'{i}. {s["name"]}' for i, s in enumerate(plan[:-1], 1))
    plan_html = f"""<header class="masthead">
  <p class="eyebrow">NotPurplePen · Translations</p>
  <h1>Translation Logbook</h1>
  <p class="lede">NotPurplePen, a fork of the orienteering course-setting program Purple Pen, completes the translations
  of its 26 translated languages, one key group at a time, each for all languages at once. After each group, this log
  shows what each language still lacks and what the group brought. The translators are language models; each
  translation names the model and its effort setting.</p>
  <figure class="course">
    {course_svg(plan, states)}
    <figcaption>The plan, drawn as a course. The controls are the steps: {esc(steps_text)}. The finish:
    {esc(plan[-1]['name'])}.</figcaption>
  </figure>
</header>"""

    table = []
    for r in sorted(rows, key=lambda r: (-r['missing'], r['lang'])):
        name = NAMES.get(r['lang'], r['lang'])
        done = r['done'] * 100
        table.append(f'<tr><td>{esc(name)} <code>{esc(r["lang"])}</code></td>'
                     f'<td class="bar"><span style="--done: {done:.1f}%"></span></td>'
                     f'<td class="num">{done:.0f} %</td><td class="num">{r["missing"]:,}</td>'
                     f'<td class="num">{r["by_model"]:,}</td><td>{esc(nice_date(r["newest"])) if r["newest"] else ""}</td></tr>')
    start = data.get('start_missing') or 0
    code_commit = git(checkout, 'rev-parse', '--short=8', 'HEAD')
    stand_html = f"""<section class="standing" aria-labelledby="standing-title">
  <h2 id="standing-title">Where it stands</h2>
  <p class="meta">From <code>fix/complete-translations</code> at <a href="{COMMIT_URL}{esc(code_commit)}">{esc(code_commit)}</a>, on {esc(nice_date(today))}.</p>
  <div class="tally">
    <div class="stat"><span class="value">{groups_done} of {groups_all}</span><span class="what">key groups done</span></div>
    <div class="stat"><span class="value">{missing:,}</span><span class="what">translations still missing{f' (of {start:,} at the start of phase 3)' if start else ''}</span></div>
    <div class="stat"><span class="value">{by_model:,}</span><span class="what">translations made by a language model</span></div>
    <div class="stat"><span class="value">{terms}</span><span class="what">terms settled in <a href="{DOCS_URL}terms.csv">terms.csv</a>, in {term_langs} languages</span></div>
  </div>
  <div class="table-scroll"><table>
    <thead><tr><th>Language</th><th></th><th class="num">Done</th><th class="num">Missing</th><th class="num">By a model</th><th>Newest</th></tr></thead>
    <tbody>
    {''.join(table)}
    </tbody>
  </table></div>
  <p class="meta">A translation is missing where the satellite has no text for a key whose English holds a letter,
  and no session has judged it not needed. The full table is <a href="{DOCS_URL}Status.md">Status.md</a>.</p>
</section>"""

    entries = []
    for n, e in enumerate(data.get('log', [])):
        entries.append({'kind': 'note', 'seq': -n, 'date': e['date'], 'title': e['title'], 'by': e.get('by', ''),
                        'figures': {}, 'languages': [], 'html': markdown(e.get('text', ''), DOCS_URL),
                        'links': [(c, COMMIT_URL + c) for c in e.get('commits', [])]})
    entries += groups
    entries.sort(key=lambda e: (e['date'], e['kind'] == 'group', e['seq']), reverse=True)
    blocks = []
    for e in entries:
        figures = ''
        if e['figures']:
            figures = '<p class="figures">' + ''.join(f'<span><b>{v}</b> {esc(k)}</span>' for k, v in e['figures'].items())
            if e['languages']:
                figures += f'<span>in <b>{len(e["languages"])}</b> languages</span>'
            figures += '</p>'
        links = ''
        if e['links']:
            links = '<p class="links">' + ''.join(f'<a href="{esc(u)}">{esc(t)}</a>' for t, u in e['links']) + '</p>'
        by = f' · {esc(e["by"])}' if e['by'] else ''
        blocks.append(f'<article class="entry"><p class="meta">{esc(nice_date(e["date"]))}{by}</p>'
                      f'<h3>{esc(e["title"])}</h3>{figures}{e["html"]}{links}</article>')
    log_html = f"""<section class="log" aria-labelledby="log-title">
  <h2 id="log-title">Log</h2>
  <div class="entries">
  {''.join(blocks)}
  </div>
</section>"""

    docs_commit = git(HERE, 'rev-parse', '--short=8', 'HEAD')
    footer_html = f"""<footer>
  <p><code>logbook.py</code> made this page on {esc(nice_date(today))} from <code>fix/complete-translations</code>
  at {esc(code_commit)} and <code>docs/process-coordination</code> at {esc(docs_commit)}. The sessions that translate
  make it again after each key group (<a href="{DOCS_URL}Session.md">Session.md</a>).</p>
  <p><a href="../">The downloads</a> · <a href="../svg/">The SVG Logbook</a> · <a href="{DOCS_URL}README.md">How the translation works</a></p>
</footer>"""

    os.makedirs(out, exist_ok=True)
    with open(os.path.join(out, 'index.html'), 'w', encoding='utf-8', newline='\n') as f:
        f.write(page(plan_html, stand_html, log_html, footer_html))
    print(f'{os.path.join(out, "index.html")}: {groups_done} of {groups_all} groups done, {missing} missing')


if __name__ == '__main__':
    main()

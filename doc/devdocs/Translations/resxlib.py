"""Shared parts of the translation scripts: the resources that the application translates, its
languages, and the reading and writing of .resx files and of the scripts' TSV files.

Each script takes the path of a checkout of NotPurplePen. Nothing here changes a file except
`write_text`, which `apply.py` calls."""

import collections
import os
import re
import subprocess
import xml.etree.ElementTree as ET

# The resources that AvPurplePen shows, by name, with their English file without ".resx".
# A satellite is the same path with ".<language>" before ".resx".
RESOURCES = [
    ('UIText', 'src/AvPurplePen/UIText'),
    ('MiscText', 'src/PurplePenCore/MiscText'),
    ('CommandNameText', 'src/PurplePenCore/CommandNameText'),
    ('StatusBarText', 'src/PurplePenCore/StatusBarText'),
    ('SelectionDescriptionText', 'src/PurplePenCore/SelectionDescriptionText'),
    ('ReportText', 'src/PurplePenCore/ReportText'),
]
RESOURCE_PATHS = dict(RESOURCES)

# Upstream's TranslateTool writes the satellites of all resources into the WinForms project
# (doc/devdocs/SynchronizeTranslations.txt). The satellites of PurplePenCore's resources that it
# left there are in no build: PurplePen.csproj removes them, and PurplePenCore has no copy.
STRANDED_DIR = 'src/PurplePen'

# The languages that each language's translator reads first, nearest first. All other languages
# follow these, the most complete first.
RELATED = {
    'bg': ['ru', 'uk', 'sl', 'cs', 'pl'],
    'ca': ['es', 'fr', 'it', 'pt'],
    'cs': ['pl', 'sl', 'uk', 'ru', 'bg'],
    'da': ['nb-NO', 'nn-NO', 'sv'],
    'de': ['nl'],
    'en-GB': [],
    'es': ['ca', 'pt', 'it', 'fr'],
    'et': ['fi'],
    'fi': ['et'],
    'fr': ['it', 'es', 'ca', 'pt'],
    'he': [],
    'hu': [],
    'id': [],
    'it': ['vec', 'es', 'fr', 'pt'],
    'ja': ['zh-TW', 'zh-CN'],
    'ku': [],
    'nb-NO': ['nn-NO', 'da', 'sv'],
    'nl': ['de'],
    'nn-NO': ['nb-NO', 'da', 'sv'],
    'pl': ['cs', 'uk', 'ru', 'sl', 'bg'],
    'pt': ['pt-BR', 'es', 'it', 'fr'],
    'pt-BR': ['pt', 'es', 'it', 'fr'],
    'ro': ['it', 'fr', 'es', 'pt'],
    'ru': ['uk', 'bg', 'pl', 'cs', 'sl'],
    'sl': ['cs', 'pl', 'bg', 'uk', 'ru'],
    'sv': ['nb-NO', 'da', 'nn-NO'],
    'tr': [],
    'uk': ['ru', 'pl', 'bg', 'cs', 'sl'],
    'vec': ['it', 'es', 'fr'],
    'zh-CN': ['zh-TW', 'zh-HK', 'ja'],
    'zh-HK': ['zh-TW', 'zh-CN', 'ja'],
    'zh-TW': ['zh-HK', 'zh-CN', 'ja'],
}

LANGUAGE_PATTERN = re.compile(r'^[a-z]{2,3}(-[A-Za-z0-9]+)*$')

# The languages that translate almost nothing (0 to 1 percent of the keys on Nov 16) and are not to be
# translated (@Pauli, Nov 16). ru, also almost untranslated, is translated: @Pauli chose it.
NOT_TRANSLATED = ['ca', 'he', 'id', 'ku', 'vec', 'zh-HK']

# The keys that a language's translator judged to need no translation in the language, as
# not-needed/<language>.tsv beside these scripts (columns resource, key, english, note, translator,
# effort, date). A key counts
# as not needed only while its English is the english of the line. Such a key gets no entry in the
# satellite, so it follows the English file when the English changes.
NOT_NEEDED_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'not-needed')


class Entry:
    """One <data> element of a .resx file. kind is 'string' for a text, 'null' for a ResXNullRef
    (which the resource manager reads as no value), and 'other' for anything else."""

    __slots__ = ('name', 'value', 'comment', 'kind')

    def __init__(self, name, value, comment, kind):
        self.name = name
        self.value = value
        self.comment = comment
        self.kind = kind

    def has_text(self):
        """True if the entry gives a text that is not empty."""
        return self.kind == 'string' and self.value is not None and self.value != ''


def parse_entries(data):
    """Reads the <data> elements of a .resx file, given as bytes, in their order. Returns a dict
    from name to Entry. The examples in the standard header are in a comment, so they are not read."""
    root = ET.fromstring(data)
    entries = {}
    for element in root.findall('data'):
        name = element.get('name')
        if name is None:
            continue
        value_element = element.find('value')
        comment_element = element.find('comment')
        value = value_element.text if value_element is not None else None
        comment = comment_element.text if comment_element is not None else None
        type_name = element.get('type') or ''
        if 'ResXNullRef' in type_name:
            kind = 'null'
        elif type_name or element.get('mimetype'):
            kind = 'other'
        else:
            kind = 'string'
            if value is None:
                value = ''
        entries[name] = Entry(name, value, comment, kind)
    return entries


def read_bytes(checkout, path, rev=None):
    """The bytes of a file of the checkout, or of the file at a revision of its repository.
    None if the file does not exist there."""
    if rev is None:
        full = os.path.join(checkout, path)
        if not os.path.exists(full):
            return None
        with open(full, 'rb') as f:
            return f.read()
    result = subprocess.run(['git', '-C', checkout, 'show', f'{rev}:{path}'], capture_output=True)
    return result.stdout if result.returncode == 0 else None


def read_resource(checkout, base, language=None, rev=None, directory=None):
    """The entries of a resource's English file (language None) or of one of its satellites.
    directory replaces the folder of base, to read a satellite that is elsewhere. None if the
    file does not exist."""
    path = base if language is None else f'{base}.{language}'
    if directory is not None:
        path = os.path.join(directory, os.path.basename(path))
    data = read_bytes(checkout, path + '.resx', rev)
    return None if data is None else parse_entries(data)


def english_texts(checkout, resource, rev=None):
    """The keys and texts of a resource's English file that a translation can give: entries of
    kind 'string'. None if the file does not exist at that revision."""
    entries = read_resource(checkout, RESOURCE_PATHS[resource], rev=rev)
    if entries is None:
        return None
    return {name: entry for name, entry in entries.items() if entry.kind == 'string'}


def languages(checkout):
    """The languages that have a satellite of at least one resource, sorted."""
    found = set()
    for _, base in RESOURCES:
        folder = os.path.join(checkout, os.path.dirname(base))
        prefix = os.path.basename(base) + '.'
        for name in os.listdir(folder):
            if name.startswith(prefix) and name.endswith('.resx'):
                code = name[len(prefix):-len('.resx')]
                if LANGUAGE_PATTERN.match(code):
                    found.add(code)
    return sorted(found)


def has_letter(text):
    """True if the text holds a letter: a text without one, such as "{0}" or "-", needs no
    translation."""
    return any(ch.isalpha() for ch in re.sub(r'\{[^}]*\}', '', text or ''))


def normalize(text):
    """The text reduced for comparison: no access-key marks (& or _), no trailing colon or
    ellipsis, single spaces, lower case."""
    t = (text or '').replace('&&', '\x00').replace('&', '').replace('\x00', '&')
    t = re.sub(r'(?<=\w)_(?=\w)|^_|(?<=\s)_', '', t)
    t = t.strip()
    t = re.sub(r'(\.\.\.|\u2026|:)\s*$', '', t).strip()
    t = re.sub(r'\s+', ' ', t)
    return t.lower()


def read_not_needed(language, directory=None):
    """The not-needed lines of a language: a dict from (resource, key) to the row."""
    path = os.path.join(directory or NOT_NEEDED_DIR, f'{language}.tsv')
    if not os.path.exists(path):
        return {}
    return {(row['resource'], row['key']): row for row in read_tsv(path)}


def write_not_needed(language, rows, directory=None):
    """Writes the not-needed lines of a language, given as the dict that read_not_needed returns."""
    directory = directory or NOT_NEEDED_DIR
    os.makedirs(directory, exist_ok=True)
    order = {res: i for i, (res, _) in enumerate(RESOURCES)}
    items = sorted(rows.values(), key=lambda r: (order.get(r['resource'], 99), r['key']))
    write_tsv(os.path.join(directory, f'{language}.tsv'),
              ['resource', 'key', 'english', 'note', 'translator', 'effort', 'date'],
              [[r['resource'], r['key'], r['english'], r.get('note', ''), r.get('translator', ''),
                r.get('effort', ''), r.get('date', '')] for r in items])


def missing_keys(english, satellite, resource=None, not_needed=None):
    """The keys of the English texts that the satellite does not translate: absent, null, or
    empty. Keys whose English holds no letter are left out, and so are the keys that not_needed
    (the dict of read_not_needed) gives for the resource with their English of now."""
    satellite = satellite or {}
    not_needed = not_needed or {}
    result = []
    for name, entry in english.items():
        if not has_letter(entry.value):
            continue
        skip = not_needed.get((resource, name))
        if skip is not None and skip['english'] == entry.value:
            continue
        own = satellite.get(name)
        if own is None or not own.has_text():
            result.append(name)
    return result


CONTROL_WORDS = re.compile(r'radiobutton|checkbox|combobox|groupbox|numericupdown|listbox|textbox|toolstripbutton|'
                           r'toolstrip|button|label|menu|panel', re.I)


def control_of(key):
    """For a UIText key Dialog_control_Property, (dialog, stem): the name of the control without the
    words of control types, in lower case, so that a label and the combo box it names have the same
    stem (labelBlendPurple, comboBoxBlendPurple). None for another key, or a stem of digits only."""
    parts = key.split('_')
    if len(parts) < 3:
        return None
    stem = CONTROL_WORDS.sub('', '_'.join(parts[1:-1])).lower()
    if not stem or stem.isdigit():
        return None
    return parts[0], stem


# --- Key groups --------------------------------------------------------------------------------
# The translation goes key group by key group, each for all translated languages at once (@Pauli,
# Nov 16). A group is a dialog of UIText (the part of its keys before the first "_"; the keys without
# "_", such as OKButton, form the group "Shared"), or a resource of
# PurplePenCore with the copies of its keys that UIText has under the resource's name ("MiscText_X" is
# the copy of MiscText's X): a copy follows its key, so that both get the same translation. A group of
# more than GROUP_SIZE keys is cut into parts of GROUP_SIZE in the order of the English files, named
# "<group>.1", "<group>.2" and on; a key and its copy count as one.

TWIN_RESOURCES = ('MiscText', 'CommandNameText', 'StatusBarText', 'SelectionDescriptionText', 'ReportText')
GROUP_SIZE = 40
REVIEWS_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'reviews')


def key_groups(checkout):
    """The key groups, in order: the dialogs of UIText in the order of the English file, then the
    resources of PurplePenCore. Returns a list of (name, [(resource, key), ...]). Keys whose English
    holds no letter are left out."""
    english = {res: english_texts(checkout, res) for res, _ in RESOURCES}
    ui = english['UIText']
    units = collections.OrderedDict()
    for key, entry in ui.items():
        prefix = key.split('_')[0] if '_' in key else 'Shared'
        if prefix in TWIN_RESOURCES or not has_letter(entry.value):
            continue
        units.setdefault(prefix, []).append([('UIText', key)])
    for res in TWIN_RESOURCES:
        copies = {k[len(res) + 1:]: k for k in ui if k.startswith(res + '_') and has_letter(ui[k].value)}
        items = []
        for key, entry in english[res].items():
            if not has_letter(entry.value):
                continue
            unit = [(res, key)]
            copy = copies.pop(key, None)
            if copy is not None:
                unit.append(('UIText', copy))
            items.append(unit)
        items += [[('UIText', k)] for k in copies.values()]
        units[res] = items
    groups = []
    for name, items in units.items():
        parts = [items[i:i + GROUP_SIZE] for i in range(0, len(items), GROUP_SIZE)] or [[]]
        for i, part in enumerate(parts):
            label = name if len(parts) == 1 else f'{name}.{i + 1}'
            groups.append((label, [k for unit in part for k in unit]))
    return groups


def group_done(name):
    """True if the group has its review, which a session writes when it finishes the group."""
    return os.path.exists(os.path.join(REVIEWS_DIR, 'groups', f'{name}.md'))


def translated_languages(checkout):
    """The languages that are translated: all that have a satellite, except NOT_TRANSLATED."""
    return [l for l in languages(checkout) if l not in NOT_TRANSLATED]


def read_terms(path=None):
    """The rows of terms.csv as (English term without its sense in brackets, the English column, a
    dict from language to its value)."""
    import csv
    path = path or os.path.join(os.path.dirname(os.path.abspath(__file__)), 'terms.csv')
    if not os.path.exists(path):
        return []
    with open(path, encoding='utf-8') as f:
        rows = list(csv.reader(f))
    header = rows[0]
    result = []
    for row in rows[1:]:
        if not row or not row[0].strip():
            continue
        term = re.sub(r'\s*\([^)]*\)\s*$', '', row[0]).strip().lower()
        result.append((term, row[0], {header[i]: row[i] for i in range(1, min(len(header), len(row)))}))
    return result


def related_order(language, all_languages, completeness):
    """The other languages in the order that a translator of the language reads them: the related
    languages first, then the others by how many keys they translate, most first."""
    first = [l for l in RELATED.get(language, []) if l in all_languages and l != language]
    rest = [l for l in all_languages if l != language and l not in first]
    rest.sort(key=lambda l: (-completeness.get(l, 0), l))
    return first + rest


# --- Upstream's PO files -------------------------------------------------------------------------
# loc/po/purplepen holds the PO files of upstream's translations on Launchpad, as upstream last
# synchronized them (doc/devdocs/SynchronizeTranslations.txt). They keep the English of strings
# that the .resx files have since changed or dropped.

PO_DIR = 'loc/po/purplepen'
PO_NAMES = {'en-GB': 'en_GB', 'nb-NO': 'nb', 'nn-NO': 'nn', 'pt-BR': 'pt_BR',
            'zh-CN': 'zh_CN', 'zh-HK': 'zh_HK', 'zh-TW': 'zh_TW'}


def _po_string(lines):
    """The text of a PO string given as its quoted lines."""
    out = []
    for line in lines:
        s = line.strip()
        if not (s.startswith('"') and s.endswith('"')):
            continue
        s = s[1:-1]
        i = 0
        while i < len(s):
            ch = s[i]
            if ch == '\\' and i + 1 < len(s):
                out.append({'n': '\n', 't': '\t', '"': '"', '\\': '\\', 'r': '\r'}.get(s[i + 1], s[i + 1]))
                i += 2
            else:
                out.append(ch)
                i += 1
    return ''.join(out)


def read_po(checkout, language):
    """The translated, not fuzzy, entries of a language's PO file: a list of (references, English,
    translation). Empty if the language has no PO file."""
    path = os.path.join(checkout, PO_DIR, PO_NAMES.get(language, language) + '.po')
    if not os.path.exists(path):
        return []
    with open(path, encoding='utf-8') as f:
        blocks = f.read().replace('\r\n', '\n').split('\n\n')
    result = []
    for block in blocks:
        lines = block.split('\n')
        refs = [l[3:].strip() for l in lines if l.startswith('#: ')]
        fuzzy = any(l.startswith('#,') and 'fuzzy' in l for l in lines)
        msgid, msgstr, current = [], [], None
        for line in lines:
            if line.startswith('msgid '):
                current = msgid
                current.append(line[len('msgid '):])
            elif line.startswith('msgstr '):
                current = msgstr
                current.append(line[len('msgstr '):])
            elif line.startswith('"') and current is not None:
                current.append(line)
            elif not line.startswith('#'):
                current = None
        english, translation = _po_string(msgid), _po_string(msgstr)
        if english and translation and not fuzzy:
            result.append((refs, english, translation))
    return result


# --- TSV -----------------------------------------------------------------------------------------
# A TSV of the scripts has a header line and one line for each key. Within a field, a backslash, a
# tab and a line break are written \\, \t and \n.

def tsv_escape(text):
    return (text or '').replace('\\', '\\\\').replace('\t', '\\t').replace('\r\n', '\n').replace('\n', '\\n')


def tsv_unescape(text):
    out = []
    i = 0
    while i < len(text):
        ch = text[i]
        if ch == '\\' and i + 1 < len(text):
            nxt = text[i + 1]
            out.append({'n': '\n', 't': '\t', '\\': '\\'}.get(nxt, '\\' + nxt))
            i += 2
        else:
            out.append(ch)
            i += 1
    return ''.join(out)


def write_tsv(path, header, rows):
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\t'.join(header) + '\n')
        for row in rows:
            f.write('\t'.join(tsv_escape(str(v)) for v in row) + '\n')


def read_tsv(path):
    """The rows of a TSV as dicts by the names of its header."""
    with open(path, encoding='utf-8') as f:
        lines = f.read().split('\n')
    header = lines[0].split('\t')
    rows = []
    for line in lines[1:]:
        if not line.strip():
            continue
        fields = line.split('\t')
        fields += [''] * (len(header) - len(fields))
        rows.append({h: tsv_unescape(v) for h, v in zip(header, fields)})
    return rows


# --- Writing .resx -------------------------------------------------------------------------------
# The satellites are UTF-8 with a byte-order mark, CRLF line ends, no line end after </root>, and
# their entries in the order of the English file, each as
#   <data name="..." xml:space="preserve">
#     <value>...</value>
#   </data>

ENTRY_PATTERN = re.compile(r'\r\n  <data name="(?P<name>[^"]+)"[^>]*>.*?</data>', re.S)


def xml_text(text):
    return text.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')


def entry_block(name, value, comment=None):
    block = f'\r\n  <data name="{xml_text(name)}" xml:space="preserve">\r\n    <value>{xml_text(value)}</value>'
    if comment:
        block += f'\r\n    <comment>{xml_text(comment)}</comment>'
    return block + '\r\n  </data>'


# --- Who made a translation -------------------------------------------------------------------
# A translation that a language model made carries in the <comment> of its entry
#   translator=<model id>; effort=<effort setting>; date=<YYYY-MM-DD>
# The resource manager does not read comments, so they change nothing that the application shows.
# A person who changes such a translation changes or removes the comment.

TRANSLATOR_PATTERN = re.compile(r'(?:^|;\s*)translator=([^;]+)')


def translator_comment(translator, effort, date):
    """The comment of a translation that a language model made."""
    parts = [f'translator={translator.strip()}']
    if effort:
        parts.append(f'effort={effort.strip()}')
    if date:
        parts.append(f'date={date}')
    return '; '.join(parts)


def made_by(entry):
    """The translator that the comment of an entry names, or None."""
    if entry is None or not entry.comment:
        return None
    m = TRANSLATOR_PATTERN.search(entry.comment)
    return m.group(1).strip() if m else None


def read_text(path):
    with open(path, 'rb') as f:
        return f.read().decode('utf-8-sig')


def write_text(path, text):
    with open(path, 'wb') as f:
        f.write(b'\xef\xbb\xbf' + text.encode('utf-8'))


def empty_satellite(english_text):
    """A satellite with no entries, made from the English file: its header, schema and resheaders."""
    end = english_text.rindex('</resheader>') + len('</resheader>')
    return english_text[:end] + '\r\n</root>'


def body_start(text):
    """Where the entries of a .resx text start: after its schema and resheaders."""
    return text.rindex('</resheader>') + len('</resheader>') if '</resheader>' in text else text.index('</xsd:schema>')


def set_entries(text, english_order, values, comments=None):
    """Returns the .resx text with each name of values given its value, and the comment that
    comments gives for the name, if any. An entry of the name is replaced; otherwise the entry goes
    after the nearest name before it in english_order that the text has."""
    comments = comments or {}
    position = {name: i for i, name in enumerate(english_order)}
    for name in sorted(values, key=lambda n: position.get(n, len(position))):
        block = entry_block(name, values[name], comments.get(name))
        start = body_start(text)
        present = {m.group('name'): m for m in ENTRY_PATTERN.finditer(text, start)}
        if name in present:
            m = present[name]
            text = text[:m.start()] + block + text[m.end():]
            continue
        anchor = None
        for earlier in reversed(english_order[:position.get(name, len(english_order))]):
            if earlier in present:
                anchor = present[earlier]
                break
        if anchor is not None:
            at = anchor.end()
        else:
            following = [m for n, m in present.items() if position.get(n, len(position)) > position.get(name, len(position))]
            at = min(m.start() for m in following) if following else start
        text = text[:at] + block + text[at:]
    return text

"""Give an edited file back the line endings of its committed version.

Usage: python3 -I keep-eol.py <committed copy> <edited file>

An editor that wrote the edited file normalized its line endings. The script compares the lines of the two files
without their endings, keeps the ending of each line that the edit did not change, gives the new lines CRLF, and
writes the edited file.
"""
import difflib
import sys


def split(data):
    lines = data.split(b"\n")
    last = lines.pop()  # The text after the last LF, usually empty.
    result = []
    for line in lines:
        if line.endswith(b"\r"):
            result.append((line[:-1], b"\r\n"))
        else:
            result.append((line, b"\n"))
    return result, last


def main():
    with open(sys.argv[1], "rb") as file:
        original, original_tail = split(file.read())
    with open(sys.argv[2], "rb") as file:
        edited, edited_tail = split(file.read())

    matcher = difflib.SequenceMatcher(None, [text for text, _ in original], [text for text, _ in edited], autojunk=False)
    out = []
    for tag, i1, i2, j1, j2 in matcher.get_opcodes():
        if tag == "equal":
            out.extend(text + ending for text, ending in original[i1:i2])
        else:
            out.extend(text + b"\r\n" for text, _ in edited[j1:j2])
    with open(sys.argv[2], "wb") as file:
        file.write(b"".join(out) + edited_tail)


if __name__ == "__main__":
    sys.exit(main())

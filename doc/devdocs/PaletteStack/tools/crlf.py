"""Give files CRLF line endings again, after a tool wrote them with LF.

Usage: python3 -I crlf.py <file>...

The sources of the repository have CRLF line endings, and .gitattributes keeps git from changing them, so a file
that a script rewrote with LF shows every line as changed. The script changes each lone LF into CRLF, leaves the
other bytes as they are, and names each file that it changed. Check with git ls-files --eol <file>.
"""
import sys


def main():
    for path in sys.argv[1:]:
        with open(path, "rb") as file:
            data = file.read()
        fixed = data.replace(b"\r\n", b"\n").replace(b"\n", b"\r\n")
        if fixed != data:
            with open(path, "wb") as file:
                file.write(fixed)
            print("crlf:", path)


if __name__ == "__main__":
    sys.exit(main())

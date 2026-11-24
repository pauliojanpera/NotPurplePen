#!/usr/bin/env bash
# windows-fonts.sh <folder>
#
# Fetches the branch linux of pauliojanpera/windows-fonts into <folder>/fonts, and writes <folder>/fonts.conf, a
# fontconfig file that offers only the fonts of the branch: the 14 fonts of main at the root of the branch, and the
# fonts of the Windows machine of the workflow in system/ ("The fonts of the local Linux runs" in ../Record.md).
# The file names only that folder and includes no other configuration, so the fonts of this machine and the aliases
# of fontconfig, which give for example Liberation Sans for Arial, are not in it.
#
# The file gives one family to each pattern of fontconfig, Segoe UI, the font of the user interface of Windows. Skia
# then gives a typeface for a family that is not installed, as it does on Windows. Without a family, the font manager
# of Skia on Linux gives no typeface for it, and SkiaFontManager.CreateTypeface stops on a null reference: the tests
# of the maps that name such a family fail.
#
# A local run of the rendering tests sets FONTCONFIG_FILE to it, as the SVG tests do with their sentinel font:
#
#     FONTCONFIG_FILE=<folder>/fonts.conf TEST_SILENTRUN=True ~/.dotnet/dotnet test ...
#
# The repository is private, so the clone needs the credentials of @Pauli's account. A later call fetches the branch
# again. The clone keeps its git folder in <folder>/git, apart from the fonts, as fontconfig reads each file below
# the folder that the fontconfig file names. Prints the number of fonts that fontconfig finds, and their families.
set -euo pipefail
[ $# -eq 1 ] || { echo "usage: windows-fonts.sh <folder>" >&2; exit 2; }
mkdir -p "$1"
folder=$(cd "$1" && pwd)
remote=${FONTS_REMOTE:-https://github.com/pauliojanpera/windows-fonts}

if [ ! -e "$folder/git" ]; then
    git clone -q --depth 1 --branch linux --single-branch --separate-git-dir "$folder/git" "$remote" "$folder/fonts"
else
    git -C "$folder/fonts" fetch -q --depth 1 origin linux
    git -C "$folder/fonts" reset -q --hard FETCH_HEAD
fi

# fontconfig reads the folders below a <dir> too, so the one folder gives the fonts of the root and of system/.
cat > "$folder/fonts.conf" <<EOF
<?xml version="1.0"?>
<!DOCTYPE fontconfig SYSTEM "fonts.dtd">
<fontconfig>
  <dir>$folder/fonts</dir>
  <cachedir>$folder/cache</cachedir>
  <match target="pattern">
    <edit name="family" mode="append_last"><string>Segoe UI</string></edit>
  </match>
</fontconfig>
EOF

echo "$(git -C "$folder/fonts" log -1 --format='%h %s')"
echo "wrote $folder/fonts.conf"
if command -v fc-list > /dev/null; then
    files=$(FONTCONFIG_FILE="$folder/fonts.conf" fc-list --format '%{file}\n' | sort -u | wc -l)
    echo "$files font files, these families:"
    FONTCONFIG_FILE="$folder/fonts.conf" fc-list --format '%{family[0]}\n' | sort -u | paste -sd ',' | sed 's/,/, /g'
fi

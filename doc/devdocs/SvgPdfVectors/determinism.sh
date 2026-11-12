#!/usr/bin/env bash
# determinism.sh <checkout> <out> <pdf>...
#
# Writes the vector way's SVG of each PDF (the probe's vector-svg, PdfPig into the SVG target) under runtime settings
# and locales that a document must not depend on (F-2): the default, a Finnish locale and another time zone, the JIT
# without tiering, and the JIT without hardware intrinsics (SSE, AVX and their kin), and says for each whether the bytes
# are those of the default. Each PDF is a path within the checkout's src/TestFiles. Needs the .NET 10 SDK.
set -euo pipefail
[ $# -ge 3 ] || { echo "usage: determinism.sh <checkout> <out> <pdf>..." >&2; exit 2; }
here=$(cd "$(dirname "$0")" && pwd)
checkout=$(cd "$1" && pwd)
mkdir -p "$2"
out=$(cd "$2" && pwd)
shift 2
dotnet=${DOTNET:-$(command -v dotnet || echo /root/.dotnet/dotnet)}
"$dotnet" build "$here/probe/PdfVectorsProbe.csproj" -r linux-x64 -p:AppSrc="$checkout/src" -o "$out/probe-bin" -v quiet -nologo 2>&1 | grep -E 'error|Build succeeded' | sort -u
for relative in "$@"; do
    pdf="$checkout/src/TestFiles/$relative"
    name=$(basename "$relative" .pdf | tr ' .' '__')
    run() { env LANG=en_US.UTF-8 "$@" "$dotnet" "$out/probe-bin/PdfVectorsProbe.dll" vector-svg "$pdf" "$out/$name.$setting.svg" > /dev/null; }
    setting=default; run LC_ALL=C TZ=UTC
    for setting in locale tiering intrinsics; do
        case $setting in
        locale) run LC_ALL=fi_FI.UTF-8 LANG=fi_FI.UTF-8 TZ=Pacific/Auckland ;;
        tiering) run DOTNET_TieredCompilation=0 DOTNET_TieredPGO=0 ;;
        intrinsics) run DOTNET_EnableHWIntrinsic=0 ;;
        esac
        same=false
        cmp -s "$out/$name.default.svg" "$out/$name.$setting.svg" && same=true
        echo "{\"pdf\":\"$relative\",\"setting\":\"$setting\",\"identical_to_default\":$same,\"bytes\":$(stat -c %s "$out/$name.$setting.svg")}"
    done
done

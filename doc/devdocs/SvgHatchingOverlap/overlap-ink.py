"""How much more ink overlapping areas carry when each draws its own hatching, at several resolutions.

Usage: python3 -I overlap-ink.py <folder> <pixels per mm of map at 1 dpi> <dpi>...

The folder holds, for each resolution, the map drawn as before, res-<dpi>-once0.png, and with each overlap's hatching
drawn once, res-<dpi>-once1.png, as `harness render` draws them (run-all.sh). The overlaps are where the two drawings
differ, closed over a little more than the hatching's spacing of 0.3 mm so that the gaps between lines count too; in
them, the ink of each drawing is the mean of 1 - red / 255, as the hatching is cyan on white.
"""
import sys

from PIL import Image, ImageChops, ImageFilter


def main():
    folder, scale = sys.argv[1], float(sys.argv[2])
    print("| Resolution | Pixels of the overlaps | Ink before | Ink drawn once | More ink before |")
    print("| --- | --- | --- | --- | --- |")
    for dpi in (int(value) for value in sys.argv[3:]):
        before = Image.open("%s/res-%d-once0.png" % (folder, dpi)).convert("RGB")
        once = Image.open("%s/res-%d-once1.png" % (folder, dpi)).convert("RGB")
        pixels_per_mm = dpi * scale
        differ = ImageChops.difference(before, once).convert("L").point(lambda v: 255 if v > 0 else 0)
        size = max(3, int(0.35 * pixels_per_mm) * 2 + 1)
        overlaps = differ.filter(ImageFilter.MaxFilter(size)).filter(ImageFilter.MinFilter(size))
        b, o, m = before.load(), once.load(), overlaps.load()
        count = 0
        ink_before = ink_once = 0.0
        for y in range(before.height):
            for x in range(before.width):
                if m[x, y]:
                    count += 1
                    ink_before += 1 - b[x, y][0] / 255
                    ink_once += 1 - o[x, y][0] / 255
        print("| %d dpi | %d | %.3f | %.3f | %+.1f %% |" % (dpi, count, ink_before / count, ink_once / count, (ink_before / ink_once - 1) * 100))


if __name__ == "__main__":
    main()

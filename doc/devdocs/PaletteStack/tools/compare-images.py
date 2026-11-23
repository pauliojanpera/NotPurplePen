"""Compare two renderings pixel by pixel, and mark where they differ.

Usage: python3 -I compare-images.py <image> <other image> [<marked image>]

Prints how many pixels differ, the largest difference of one channel, and the box that holds the differences. With
a third path, writes the first image faded, with each differing pixel in red, to show where a change moved the
drawing: for example a rendering before and after a change, or a local rendering and the one of a Tests run.
"""
import sys
from functools import reduce

from PIL import Image, ImageChops


def main():
    if len(sys.argv) not in (3, 4):
        print(__doc__.strip().split("\n")[2], file=sys.stderr)
        return 2

    first = Image.open(sys.argv[1]).convert("RGB")
    second = Image.open(sys.argv[2]).convert("RGB")
    if first.size != second.size:
        print("the sizes differ:", first.size, second.size)
        return 1

    difference = ImageChops.difference(first, second)
    # The largest difference of the three channels at each pixel.
    channel = reduce(ImageChops.lighter, difference.split())
    histogram = channel.histogram()
    differing = sum(histogram[1:])
    largest = max(value for value, count in enumerate(histogram) if count)
    print("%d of %d pixels differ, by at most %d in a channel, within %s"
          % (differing, first.size[0] * first.size[1], largest, difference.getbbox()))

    if len(sys.argv) == 4:
        mask = channel.point(lambda value: 255 if value else 0)
        marked = Image.blend(first, Image.new("RGB", first.size, (255, 255, 255)), 0.6)
        marked.paste((255, 0, 0), mask=mask)
        marked.save(sys.argv[3])
    return 0 if differing == 0 else 1


if __name__ == "__main__":
    sys.exit(main())

"""The largest band excess at rescued pixels, and the largest channel difference, over the real renderings."""
import os, sys
import numpy as np
from PIL import Image
from scipy import ndimage
SLACK = 4
def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float64)
def window(image, size, highest):
    return (ndimage.maximum_filter if highest else ndimage.minimum_filter)(image, size=(size, size, 1), mode="constant", cval=255)
data = sys.argv[1]
largest_excess, largest_difference, cases_outside = 0.0, 0.0, 0
for name in sorted(n[:-4] for n in os.listdir(os.path.join(data, "references"))):
    e, r = load(os.path.join(data, "references", name + ".png")), load(os.path.join(data, "renderings", name + ".png"))
    excess = np.zeros(e.shape[:2])
    for one, other in ((e, r), (r, e)):
        low, high = window(one, 3, False) - SLACK, window(one, 3, True) + SLACK
        excess = np.maximum(excess, np.maximum(np.maximum(low - other, other - high), 0).max(axis=2))
    if (excess > 0).any():
        cases_outside += 1
        largest_excess = max(largest_excess, excess.max())
    largest_difference = max(largest_difference, np.abs(e - r).max())
print("cases with pixels outside the range:", cases_outside, "largest excess:", largest_excess, "largest difference:", largest_difference)

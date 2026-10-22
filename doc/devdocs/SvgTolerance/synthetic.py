"""Check the synthetic test images against the rule, grey levels only."""
import numpy as np
from scipy import ndimage
SLACK, SHARE = 4, 0.4
def window(image, size, highest):
    return (ndimage.maximum_filter if highest else ndimage.minimum_filter)(image, size=size, mode="constant", cval=255)
def failing(e, r, share):
    e, r = e.astype(float), r.astype(float)
    excess = np.zeros(e.shape)
    for one, other in ((e, r), (r, e)):
        excess = np.maximum(excess, np.maximum(np.maximum(window(one, 3, False) - SLACK - other, other - window(one, 3, True) - SLACK), 0))
    at_edge = ((window(e, 3, True) - window(e, 3, False)) > SLACK) | ((window(r, 3, True) - window(r, 3, False)) > SLACK)
    blur = lambda image: ndimage.gaussian_filter(image, 1, mode="constant", cval=255)
    contrast = np.maximum(window(e, 9, True) - window(e, 9, False), window(r, 9, True) - window(r, 9, False))
    d = np.abs(blur(e) - blur(r))
    passes = at_edge & (d <= np.minimum(40, share * contrast))
    fail = (excess > 0) & ~passes
    return int(fail.sum()), d[(excess > 0)].max() if (excess > 0).any() else 0, contrast[(excess > 0)].min() if (excess > 0).any() else 0
# Faint line shifted by two pixels.
e = np.full((12, 12), 255); e[:, 4:6] = 231
r = np.full((12, 12), 255); r[:, 6:8] = 231
print("faint shift: failing with share", failing(e, r, SHARE), "with a fixed 40 only", failing(e, r, 1e9))
# Faint seam.
e = np.full((9, 13), 255); r = np.full((9, 13), 255)
for image in (e, r):
    image[0:4, :] = 231; image[5:9, :] = 243
e[4, :] = 250; r[4, :] = 236
print("faint seam: failing", failing(e, r, SHARE))
# Existing dark seam test, for reference.
e = np.full((9, 9), 60); r = np.full((9, 9), 60)
for image in (e, r):
    image[0:4, :] = 0
e[4, :] = 100; r[4, :] = 30
print("dark seam: failing", failing(e, r, SHARE))

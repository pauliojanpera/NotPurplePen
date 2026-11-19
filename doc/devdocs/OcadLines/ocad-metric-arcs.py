"""Compare an even euclidean step with the steps of the OCAD metric along circular arcs.

Usage: ocad-metric-arcs.py

The OCAD metric measures a step (dx, dy) as max(|dx|, |dy|) + min(|dx|, |dy|) / 2. Along a direction at the angle a to
the x axis of the paper, a step of euclidean length 1 has the length k(a) in the OCAD metric.

For an arc of a circle that turns by a given angle from a given start direction, the script finds the positions along
the arc where the length in the OCAD metric is a given value. It compares them with an even euclidean step, which it
chooses so that the first and the last position are the same in both. The error is the largest distance between the
two positions of the same step, measured along the arc. The script gives it for the worst start direction and as the
mean of all the start directions, at steps of 0.5 degrees. The error is in proportion to the radius of the arc.

The first table gives the error per mm of radius and per mm of arc length, for arcs that turn by 5 to 180 degrees. The
second table gives the error in mm for some arcs, for the whole arc and for a quarter of it, as a quarter of a Bezier
curve that is close to the arc turns by a quarter of the angle."""
import math


def factor(angle):
    """The length in the OCAD metric of a euclidean step of 1 in the direction angle, in radians."""
    c, s = abs(math.cos(angle)), abs(math.sin(angle))
    return max(c, s) + min(c, s) / 2


def error(start_degrees, turn_degrees, samples=4000):
    """The largest error along an arc of radius 1, in mm per mm of radius."""
    start, turn = math.radians(start_degrees), math.radians(turn_degrees)
    step = turn / samples
    lengths = [0.0]
    for i in range(samples):
        lengths.append(lengths[-1] + factor(start + (i + 0.5) * step) * step)
    mean_factor = lengths[-1] / turn
    return max(abs(i * step - lengths[i] / mean_factor) for i in range(samples + 1))


def errors(turn_degrees):
    """The errors for all the start directions, at steps of 0.5 degrees over a period of 90 degrees."""
    return [error(start, turn_degrees) for start in [x * 0.5 for x in range(180)]]


def main():
    print("Factor: %.4f at 0 degrees, %.4f at %.2f degrees, %.4f at 45 degrees; mean %.4f" % (
        factor(0), factor(math.atan(0.5)), math.degrees(math.atan(0.5)), factor(math.pi / 4),
        sum(factor(math.radians(x * 0.01)) for x in range(9000)) / 9000))
    print()
    print("Turn    Worst E/R  Mean E/R  Worst E/L")
    for turn in (5, 10, 15, 22.5, 30, 45, 60, 90, 180):
        values = errors(turn)
        worst = max(values)
        print("%5.1f   %.5f    %.5f   %.5f" % (turn, worst, sum(values) / len(values), worst / math.radians(turn)))
    print()
    print("Arc                          Whole arc: worst, mean   Quarter: worst, mean (mm)")
    for radius, turn in ((5, 90), (10, 90), (20, 90), (40, 22.5), (100, 10)):
        whole, quarter = errors(turn), errors(turn / 4)
        print("R %3d mm, %5.1f deg, L %5.1f mm   %.3f, %.3f              %.3f, %.3f" % (
            radius, turn, radius * math.radians(turn), max(whole) * radius, sum(whole) / len(whole) * radius,
            max(quarter) * radius, sum(quarter) / len(quarter) * radius))


if __name__ == "__main__":
    main()

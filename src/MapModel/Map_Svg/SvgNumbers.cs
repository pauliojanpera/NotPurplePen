using System;

namespace PurplePen.MapModel
{
    // Formats the numbers of an SVG document with routines of its own, so that the text is the same in every culture
    // and in a translation of this code (C-5): lengths as whole steps of 0.001 mm of map (S-1), and other values with
    // as few decimals as they need (S-2). Rounding is always half up, which every runtime does alike.
    public static class SvgNumbers
    {
        // The steps in a millimetre of map.
        public const double StepsPerMillimetre = 1000;

        // The most decimals a value is written with; a double holds no more that mean anything.
        public const int MaximumDecimals = 15;

        // The largest scaled value that FormatDecimal rounds exactly, as a double holds integers up to 2^53.
        private const double LargestExactlyScaledValue = 1e15;

        // The powers of ten that decimals are rounded with, each exact as a double.
        private static readonly double[] powersOfTen = {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000,
            10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000, 1000000000000000
        };

        // Round a value to the nearest integer, halves up.
        // value: the value to round.
        public static long RoundHalfUp(double value)
        {
            return (long)Math.Floor(value + 0.5);
        }

        // Round a length in millimetres of map to whole steps.
        // millimetres: the length.
        public static long ToSteps(double millimetres)
        {
            return RoundHalfUp(millimetres * StepsPerMillimetre);
        }

        // Format an integer.
        // value: the integer.
        public static string FormatInteger(long value)
        {
            if (value == 0)
                return "0";

            bool negative = value < 0;
            long magnitude = negative ? -value : value;
            char[] digits = new char[20];
            int start = digits.Length;
            while (magnitude != 0) {
                digits[--start] = (char)('0' + (int)(magnitude % 10));
                magnitude /= 10;
            }

            string text = new string(digits, start, digits.Length - start);
            return negative ? "-" + text : text;
        }

        // Format a value rounded to at most a number of decimals, in its shortest form: without trailing zeros, without
        // the zero before the point, without the point when the value is whole, and without a sign when it rounds to
        // zero. A large value keeps only the decimals that a double can scale exactly.
        // value: the value.
        // decimals: the most decimals to keep, from 0 to MaximumDecimals.
        public static string FormatDecimal(double value, int decimals)
        {
            while (decimals > 0 && Math.Abs(value) * powersOfTen[decimals] >= LargestExactlyScaledValue)
                --decimals;

            long scaled = RoundHalfUp(value * powersOfTen[decimals]);
            if (scaled == 0)
                return "0";

            bool negative = scaled < 0;
            long magnitude = negative ? -scaled : scaled;
            long unit = (long)powersOfTen[decimals];
            long whole = magnitude / unit;
            long fraction = magnitude % unit;
            int fractionDigits = decimals;
            while (fractionDigits > 0 && fraction % 10 == 0) {
                fraction /= 10;
                --fractionDigits;
            }

            string text = (whole == 0 && fractionDigits > 0) ? "" : FormatInteger(whole);
            if (fractionDigits > 0)
                text += "." + FormatInteger(fraction).PadLeft(fractionDigits, '0');
            return negative ? "-" + text : text;
        }

        // The fewest decimals whose rounding error, at most half a unit of the last place, stays within a tolerance.
        // tolerance: the largest error allowed.
        public static int DecimalsWithin(double tolerance)
        {
            int decimals = 0;
            while (decimals < MaximumDecimals && 0.5 / powersOfTen[decimals] > tolerance)
                ++decimals;
            return decimals;
        }

        // Format a single-precision value with the fewest decimals that give the same value back, as the values a map
        // stores are written (COL-4).
        // value: the value.
        public static string FormatShortest(float value)
        {
            const int mostDecimalsOfAFloat = 9;
            for (int decimals = 0; decimals < mostDecimalsOfAFloat; ++decimals) {
                if ((float)(RoundHalfUp(value * powersOfTen[decimals]) / powersOfTen[decimals]) == value)
                    return FormatDecimal(value, decimals);
            }
            return FormatDecimal(value, mostDecimalsOfAFloat);
        }

        // Join numbers as SVG reads them, with a space between two numbers except before a minus sign.
        // text: the text to append the numbers to; it ends in a letter or a parenthesis, which needs no space.
        // numbers: the numbers, already formatted.
        public static void AppendNumbers(System.Text.StringBuilder text, string[] numbers)
        {
            for (int index = 0; index < numbers.Length; ++index) {
                if (index > 0 && numbers[index][0] != '-')
                    text.Append(' ');
                text.Append(numbers[index]);
            }
        }
    }
}

using System;
using System.Text;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // An affine transform in SVG's own form, matrix(a b c d e f), which takes a point (x, y) to
    // (a x + c y + e, b x + d y + f). It is held in doubles, so that composing the transforms of groups loses nothing
    // that writing them keeps.
    public sealed class SvgTransform
    {
        // The share of a step that rounding the translation may move a point by; the rest of the step is the linear
        // part's, divided as below, so that no point moves by a whole step (S-2).
        private const double TranslationError = 0.5;
        private const double MatrixElementsError = 0.45;
        private const double AngleError = 0.2;
        private const double ScaleError = 0.2;
        private const double ShearError = 0.05;

        // The identity, which moves nothing.
        public static readonly SvgTransform Identity = new SvgTransform(1, 0, 0, 1, 0, 0);

        // The elements of matrix(a b c d e f).
        public readonly double A, B, C, D, E, F;

        // Create a transform from its elements.
        public SvgTransform(double a, double b, double c, double d, double e, double f)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
        }

        // Create the transform of a Graphics2D matrix, scaling its translation, so that a transform between
        // millimetres becomes one between steps.
        // matrix: the matrix.
        // translationScale: what the translation is multiplied by.
        public static SvgTransform FromMatrix(Matrix matrix, double translationScale)
        {
            float[] elements = matrix.Elements;
            return new SvgTransform(elements[0], elements[1], elements[2], elements[3],
                                    elements[4] * translationScale, elements[5] * translationScale);
        }

        // Whether the transform moves nothing.
        public bool IsIdentity
        {
            get { return A == 1 && B == 0 && C == 0 && D == 1 && E == 0 && F == 0; }
        }

        // Whether the transform only moves, without turning, scaling or shearing.
        public bool IsTranslation
        {
            get { return A == 1 && B == 0 && C == 0 && D == 1; }
        }

        // Whether two transforms have the same elements.
        // other: the other transform.
        public bool HasSameElements(SvgTransform other)
        {
            return A == other.A && B == other.B && C == other.C && D == other.D && E == other.E && F == other.F;
        }

        // The transform that applies another transform first and then this one.
        // inner: the transform applied first.
        public SvgTransform After(SvgTransform inner)
        {
            return new SvgTransform(A * inner.A + C * inner.B, B * inner.A + D * inner.B,
                                    A * inner.C + C * inner.D, B * inner.C + D * inner.D,
                                    A * inner.E + C * inner.F + E, B * inner.E + D * inner.F + F);
        }

        // Transform a point.
        // x, y: the point.
        // transformedX, transformedY: receive the transformed point.
        public void TransformPoint(double x, double y, out double transformedX, out double transformedY)
        {
            transformedX = A * x + C * y + E;
            transformedY = B * x + D * y + F;
        }

        // How much the transform scales lengths, on average over directions: the square root of its determinant.
        public double LengthScale
        {
            get { return Math.Sqrt(Math.Abs(A * D - B * C)); }
        }

        // Write the transform in its shortest form, "" for the identity: translate, rotate and scale when it is those,
        // and a matrix otherwise or when that is shorter (S-8), each value with the fewest digits that keep every
        // point within reach of the origin less than a step from where the exact transform takes it (S-2).
        // reach: how far from the origin of the input the transformed points lie, at most.
        // outputUnitsPerStep: how long a step is in the output's units.
        public string Format(double reach, double outputUnitsPerStep)
        {
            if (IsIdentity)
                return "";

            reach = Math.Max(reach, 1);
            int translationDecimals = SvgNumbers.DecimalsWithin(TranslationError * outputUnitsPerStep);
            string matrixText = FormatMatrix(reach, outputUnitsPerStep, translationDecimals);
            string decomposedText = FormatDecomposed(reach, outputUnitsPerStep, translationDecimals);
            if (decomposedText != null && decomposedText.Length <= matrixText.Length)
                return decomposedText;
            return matrixText;
        }

        // Write the transform as matrix(a b c d e f).
        // reach: how far from the origin of the input the transformed points lie.
        // outputUnitsPerStep: how long a step is in the output's units.
        // translationDecimals: the decimals of e and f.
        private string FormatMatrix(double reach, double outputUnitsPerStep, int translationDecimals)
        {
            // A point's error is at most the sum of two elements' errors times the reach.
            int elementDecimals = SvgNumbers.DecimalsWithin(MatrixElementsError * outputUnitsPerStep / (2 * reach));
            StringBuilder text = new StringBuilder("matrix(");
            SvgNumbers.AppendNumbers(text, new string[] {
                SvgNumbers.FormatDecimal(A, elementDecimals), SvgNumbers.FormatDecimal(B, elementDecimals),
                SvgNumbers.FormatDecimal(C, elementDecimals), SvgNumbers.FormatDecimal(D, elementDecimals),
                SvgNumbers.FormatDecimal(E, translationDecimals), SvgNumbers.FormatDecimal(F, translationDecimals)
            });
            return text.Append(')').ToString();
        }

        // Write the transform as translate(e f), rotate(angle) and scale(x y), leaving out those that do nothing, or
        // return null if it shears points beyond what a step allows.
        // reach: how far from the origin of the input the transformed points lie.
        // outputUnitsPerStep: how long a step is in the output's units.
        // translationDecimals: the decimals of the translation.
        private string FormatDecomposed(double reach, double outputUnitsPerStep, int translationDecimals)
        {
            double scaleX = Math.Sqrt(A * A + B * B);
            if (scaleX == 0)
                return null;
            double angle = Math.Atan2(B, A);
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            double scaleY = D * cos - C * sin;
            double shear = C * cos + D * sin;
            if (Math.Abs(shear) * reach > ShearError * outputUnitsPerStep)
                return null;

            double largestScale = Math.Max(scaleX, Math.Abs(scaleY));
            int angleDecimals = SvgNumbers.DecimalsWithin(AngleError * outputUnitsPerStep / (largestScale * reach) * 180 / Math.PI);
            int scaleDecimals = SvgNumbers.DecimalsWithin(ScaleError * outputUnitsPerStep / reach);

            StringBuilder text = new StringBuilder();
            string translateX = SvgNumbers.FormatDecimal(E, translationDecimals);
            string translateY = SvgNumbers.FormatDecimal(F, translationDecimals);
            if (translateX != "0" || translateY != "0")
                AppendFunction(text, "translate", translateY == "0" ? new string[] { translateX } : new string[] { translateX, translateY });

            string degrees = SvgNumbers.FormatDecimal(angle * 180 / Math.PI, angleDecimals);
            if (degrees != "0")
                AppendFunction(text, "rotate", new string[] { degrees });

            string scaleXText = SvgNumbers.FormatDecimal(scaleX, scaleDecimals);
            string scaleYText = SvgNumbers.FormatDecimal(scaleY, scaleDecimals);
            if (scaleXText != "1" || scaleYText != "1")
                AppendFunction(text, "scale", scaleXText == scaleYText ? new string[] { scaleXText } : new string[] { scaleXText, scaleYText });

            return text.ToString();
        }

        // Append a transform function, such as rotate(30), with no space before it.
        // text: the text to append to.
        // name: the name of the function.
        // arguments: its arguments, already formatted.
        private static void AppendFunction(StringBuilder text, string name, string[] arguments)
        {
            text.Append(name).Append('(');
            SvgNumbers.AppendNumbers(text, arguments);
            text.Append(')');
        }
    }
}

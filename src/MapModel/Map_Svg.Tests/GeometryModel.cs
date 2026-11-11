using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PurplePen.Graphics2D;

namespace Map_Svg.Tests
{
    // An affine transform of the plane, which takes (x, y) to (a x + c y + e, b x + d y + f), held in doubles: the
    // transforms of drawing calls and those that a document writes are composed alike (V-10).
    public readonly struct Affine
    {
        // The elements of matrix(a b c d e f).
        public readonly double A, B, C, D, E, F;

        // The transform that moves nothing.
        public static readonly Affine Identity = new Affine(1, 0, 0, 1, 0, 0);

        // Create a transform from its elements.
        public Affine(double a, double b, double c, double d, double e, double f)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
        }

        // The transform of a Graphics2D matrix, in the single precision a graphics target is given it in.
        // matrix: the matrix.
        public static Affine FromMatrix(Matrix matrix)
        {
            float[] elements = matrix.Elements;
            return new Affine(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

        // A turn by an angle in degrees, from the x axis towards the y axis.
        // degrees: the angle.
        public static Affine Rotation(double degrees)
        {
            double radians = degrees * Math.PI / 180, cos = Math.Cos(radians), sin = Math.Sin(radians);
            return new Affine(cos, sin, -sin, cos, 0, 0);
        }

        // The transform that applies another first and then this one.
        // inner: the transform applied first.
        public Affine After(Affine inner)
        {
            return new Affine(A * inner.A + C * inner.B, B * inner.A + D * inner.B,
                              A * inner.C + C * inner.D, B * inner.C + D * inner.D,
                              A * inner.E + C * inner.F + E, B * inner.E + D * inner.F + F);
        }

        // A point transformed.
        // x, y: the point.
        public GeometryPoint Apply(double x, double y)
        {
            return new GeometryPoint(A * x + C * y + E, B * x + D * y + F);
        }

        // The most the transform stretches a length: the larger singular value of its linear part.
        public double MaxStretch
        {
            get {
                double squares = A * A + B * B + C * C + D * D, determinant = A * D - B * C;
                return Math.Sqrt((squares + Math.Sqrt(Math.Max(0, squares * squares - 4 * determinant * determinant))) / 2);
            }
        }

        // How the transform scales lengths on average over directions, and a circle's radius when it keeps circles
        // circles: the square root of its determinant's magnitude.
        public double LengthScale
        {
            get { return Math.Sqrt(Math.Abs(A * D - B * C)); }
        }

        // Whether the transform mirrors, so that what turns towards positive angles turns the other way after it.
        public bool Mirrors
        {
            get { return A * D - B * C < 0; }
        }
    }

    // A point of the plane, in doubles.
    public readonly struct GeometryPoint
    {
        // The coordinates.
        public readonly double X, Y;

        // Create a point.
        public GeometryPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        // The distance to another point.
        // other: the other point.
        public double DistanceTo(GeometryPoint other)
        {
            double dx = X - other.X, dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // The point written for a message.
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:0.####}, {1:0.####})", X, Y);
        }
    }

    // The geometry of one drawing, as a document draws it or as a drawing call describes it, in the coordinates of a
    // page or of a pattern's tile (V-10): a signature of what it is, which both sides must give alike (its kind, its
    // paint and the commands of its path, with an arc's flags), its points, its lengths (an arc's radius, a stroke's
    // width), the clips around it and the tile of the pattern it paints with. A drawing read back from a document
    // carries for each point and length how far it may lie from the call's, which the rounding of the document's
    // numbers allows; a call's allows nothing.
    public sealed class DrawnGeometry
    {
        // What the drawing is, for messages: the call, or the element and its place in the document.
        public readonly string What;

        // The signature: the kind and paint, and the commands of a path.
        public readonly StringBuilder Signature = new StringBuilder();

        // The points, and how far each may lie from the other side's.
        public readonly List<GeometryPoint> Points = new List<GeometryPoint>();
        public readonly List<double> PointAllowances = new List<double>();

        // The lengths, and how far each may lie from the other side's.
        public readonly List<double> Lengths = new List<double>();
        public readonly List<double> LengthAllowances = new List<double>();

        // The clips around the drawing, outermost first, each a geometry of its own.
        public IReadOnlyList<DrawnGeometry> Clips = Array.Empty<DrawnGeometry>();

        // The tile of the pattern the drawing paints with, or null for a color.
        public GeometryTile Pattern;

        // Create a geometry.
        // what: what the drawing is, for messages.
        public DrawnGeometry(string what)
        {
            What = what;
        }

        // Add a point.
        // point: the point.
        // allowance: how far it may lie from the other side's.
        public void AddPoint(GeometryPoint point, double allowance)
        {
            Points.Add(point);
            PointAllowances.Add(allowance);
        }

        // Add a length.
        // length: the length.
        // allowance: how far it may lie from the other side's.
        public void AddLength(double length, double allowance)
        {
            Lengths.Add(length);
            LengthAllowances.Add(allowance);
        }
    }

    // The tile of a pattern that drawings paint with: what it draws, in steps of the tile, whose middle is the origin of
    // its coordinates, as the SVG target draws a pattern brush's tile (D-7). A tile read back from a document reads its
    // drawings when they are first compared.
    public sealed class GeometryTile
    {
        // What the tile is, for messages.
        public readonly string What;

        // Gives the tile's drawings, in the order they are drawn.
        private readonly Func<IReadOnlyList<DrawnGeometry>> readDrawings;

        // The drawings, once read.
        private IReadOnlyList<DrawnGeometry> drawings;

        // Create a tile.
        // what: what the tile is, for messages.
        // readDrawings: gives its drawings, in steps of the tile.
        public GeometryTile(string what, Func<IReadOnlyList<DrawnGeometry>> readDrawings)
        {
            What = what;
            this.readDrawings = readDrawings;
        }

        // The tile's drawings, in the order they are drawn.
        public IReadOnlyList<DrawnGeometry> Drawings
        {
            get { return drawings ?? (drawings = readDrawings()); }
        }
    }

    // Builds the path of a geometry, as both sides describe a path: each command in the signature, as M, L, C, an arc
    // as A with its flags of size and turn, or Z, and its points; an arc's radius among the lengths. A segment that
    // follows a close starts a new subpath at the closed one's start, as SVG and Skia both have it, and a close that
    // follows a close, or comes before any subpath, closes nothing.
    public sealed class PathGeometryBuilder
    {
        // The geometry built.
        private readonly DrawnGeometry geometry;

        // The start of the current subpath, and how far it may lie from the other side's.
        private GeometryPoint subpathStart;
        private double subpathStartAllowance;

        // Whether a subpath has begun, and whether the last command closed it.
        private bool begun, closed;

        // Create a builder of a geometry's path.
        // geometry: the geometry.
        public PathGeometryBuilder(DrawnGeometry geometry)
        {
            this.geometry = geometry;
        }

        // Whether a subpath has begun.
        public bool HasBegun
        {
            get { return begun; }
        }

        // Begin a subpath at a point.
        // point: the point.
        // allowance: how far it may lie from the other side's.
        public void MoveTo(GeometryPoint point, double allowance)
        {
            geometry.Signature.Append(" M");
            geometry.AddPoint(point, allowance);
            subpathStart = point;
            subpathStartAllowance = allowance;
            begun = true;
            closed = false;
        }

        // Draw a straight line to a point.
        // point: the point.
        // allowance: how far it may lie from the other side's.
        public void LineTo(GeometryPoint point, double allowance)
        {
            BeginSegment();
            geometry.Signature.Append(" L");
            geometry.AddPoint(point, allowance);
        }

        // Draw a cubic Bezier curve.
        // control1, control2: its control points.
        // point: its end.
        // allowance: how far each may lie from the other side's.
        public void CubicTo(GeometryPoint control1, GeometryPoint control2, GeometryPoint point, double allowance)
        {
            BeginSegment();
            geometry.Signature.Append(" C");
            geometry.AddPoint(control1, allowance);
            geometry.AddPoint(control2, allowance);
            geometry.AddPoint(point, allowance);
        }

        // Draw an arc of a circle to a point.
        // radius: its radius.
        // radiusAllowance: how far the radius may lie from the other side's.
        // large: whether it is the larger of the two arcs between its ends.
        // turnsPositively: whether it turns from the x axis of the page towards its y axis.
        // point: its end.
        // allowance: how far the end may lie from the other side's.
        public void ArcTo(double radius, double radiusAllowance, bool large, bool turnsPositively, GeometryPoint point, double allowance)
        {
            BeginSegment();
            geometry.Signature.Append(" A").Append(large ? '1' : '0').Append(turnsPositively ? '1' : '0');
            geometry.AddPoint(point, allowance);
            geometry.AddLength(radius, radiusAllowance);
        }

        // Close the current subpath.
        public void Close()
        {
            if (!begun || closed)
                return;
            geometry.Signature.Append(" Z");
            closed = true;
        }

        // Start a new subpath at the closed one's start, when a segment follows a close.
        private void BeginSegment()
        {
            if (closed)
                MoveTo(subpathStart, subpathStartAllowance);
        }
    }

    // The characters of a text as a document holds them (V-10): without the characters XML cannot hold, controls other
    // than tab, line feed and carriage return, U+FFFE, U+FFFF and unpaired surrogates; ends of lines as an XML reader
    // gives them; composed to the precomposed form, as the application composes text before shaping it; and the emoji
    // presentation selector as the text presentation selector, as the document writes it (T-8).
    public static class DocumentCharacters
    {
        // The characters of a text as a document holds them.
        // text: the text.
        public static string Of(string text)
        {
            StringBuilder kept = new StringBuilder(text.Length);
            for (int index = 0; index < text.Length; ++index) {
                char character = text[index];
                if (char.IsSurrogatePair(text, index)) {
                    kept.Append(character).Append(text[++index]);
                }
                else if (!char.IsSurrogate(character) && character != '\uFFFE' && character != '\uFFFF' &&
                         (character >= ' ' || character == '\t' || character == '\n' || character == '\r')) {
                    kept.Append(character);
                }
            }
            string composed = kept.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
            if (!composed.IsNormalized(NormalizationForm.FormC))
                composed = composed.Normalize(NormalizationForm.FormC);
            return composed.Replace('\uFE0F', '\uFE0E');
        }
    }
}

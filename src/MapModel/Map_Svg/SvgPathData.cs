using System.Text;

namespace PurplePen.MapModel
{
    // The data of an SVG path, built from commands whose coordinates are whole steps. Each command is written in its
    // shortest form (S-3): absolute or relative, whichever is shorter; a horizontal or vertical line, or a smooth
    // curve, where it is one; the command letter left out where it repeats the one before; and no separator before a
    // minus sign. The arc flags keep their separators, which readers of SVG 1.1 need.
    public sealed class SvgPathData
    {
        // The text written so far.
        private readonly StringBuilder text = new StringBuilder();

        // The bounds of every point given, control points included.
        public readonly SvgBounds Bounds = new SvgBounds();

        // The command that the next one may leave its letter out for, or none at the start and after a close.
        private char repeatableCommand;

        // The current point, and the start of the current subpath, to which a close returns.
        private long currentX, currentY, subpathStartX, subpathStartY;

        // The first control point that a smooth curve from the current point implies.
        private long smoothControlX, smoothControlY;

        // Whether no command has been given.
        public bool IsEmpty
        {
            get { return text.Length == 0; }
        }

        // The data as written.
        public override string ToString()
        {
            return text.ToString();
        }

        // Begin a subpath at a point.
        // x, y: the point.
        public void MoveTo(long x, long y)
        {
            WriteShortest(new char[] { 'M', 'm' },
                          new long[][] { new long[] { x, y }, new long[] { x - currentX, y - currentY } });
            SetCurrentPoint(x, y);
            subpathStartX = x;
            subpathStartY = y;
        }

        // Draw a straight line to a point.
        // x, y: the point.
        public void LineTo(long x, long y)
        {
            if (y == currentY && x == currentX)
                WriteShortest(new char[] { 'h' }, new long[][] { new long[] { 0 } });
            else if (y == currentY)
                WriteShortest(new char[] { 'h', 'H' }, new long[][] { new long[] { x - currentX }, new long[] { x } });
            else if (x == currentX)
                WriteShortest(new char[] { 'v', 'V' }, new long[][] { new long[] { y - currentY }, new long[] { y } });
            else
                WriteShortest(new char[] { 'l', 'L' }, new long[][] { new long[] { x - currentX, y - currentY }, new long[] { x, y } });
            SetCurrentPoint(x, y);
        }

        // Draw a cubic Bezier curve to a point.
        // x1, y1: the first control point.
        // x2, y2: the second control point.
        // x, y: the end point.
        public void CubicTo(long x1, long y1, long x2, long y2, long x, long y)
        {
            long[] relative = { x1 - currentX, y1 - currentY, x2 - currentX, y2 - currentY, x - currentX, y - currentY };
            long[] absolute = { x1, y1, x2, y2, x, y };
            if (x1 == smoothControlX && y1 == smoothControlY) {
                WriteShortest(new char[] { 's', 'S', 'c', 'C' },
                              new long[][] { new long[] { x2 - currentX, y2 - currentY, x - currentX, y - currentY },
                                             new long[] { x2, y2, x, y }, relative, absolute });
            }
            else {
                WriteShortest(new char[] { 'c', 'C' }, new long[][] { relative, absolute });
            }
            Bounds.Include(x1, y1);
            Bounds.Include(x2, y2);
            currentX = x;
            currentY = y;
            Bounds.Include(x, y);
            smoothControlX = 2 * x - x2;
            smoothControlY = 2 * y - y2;
        }

        // Draw an elliptical arc, without rotation, to a point.
        // radiusX, radiusY: the radii of the ellipse.
        // largeArc: whether the arc is the longer of the two between its ends.
        // sweep: whether the arc turns in the direction of positive angles.
        // x, y: the end point.
        public void ArcTo(long radiusX, long radiusY, bool largeArc, bool sweep, long x, long y)
        {
            long large = largeArc ? 1 : 0, positive = sweep ? 1 : 0;
            WriteShortest(new char[] { 'a', 'A' },
                          new long[][] { new long[] { radiusX, radiusY, 0, large, positive, x - currentX, y - currentY },
                                         new long[] { radiusX, radiusY, 0, large, positive, x, y } });
            SetCurrentPoint(x, y);
        }

        // Close the current subpath, returning to its start.
        public void Close()
        {
            text.Append('z');
            repeatableCommand = '\0';
            SetCurrentPoint(subpathStartX, subpathStartY);
        }

        // Make a point the current one, which a smooth curve from it starts with as its first control point.
        // x, y: the point.
        private void SetCurrentPoint(long x, long y)
        {
            currentX = x;
            currentY = y;
            smoothControlX = x;
            smoothControlY = y;
            Bounds.Include(x, y);
        }

        // Write the shortest of several forms of one command; of forms equally short, the first.
        // letters: the command letter of each form.
        // arguments: the arguments of each form.
        private void WriteShortest(char[] letters, long[][] arguments)
        {
            int shortest = 0;
            int shortestLength = LengthOf(letters[0], arguments[0]);
            for (int form = 1; form < letters.Length; ++form) {
                int length = LengthOf(letters[form], arguments[form]);
                if (length < shortestLength) {
                    shortest = form;
                    shortestLength = length;
                }
            }
            Write(letters[shortest], arguments[shortest]);
        }

        // The length that writing a command would add.
        // letter: the command letter, which is left out when it repeats the command before.
        // arguments: its arguments.
        private int LengthOf(char letter, long[] arguments)
        {
            bool letterLeftOut = letter == repeatableCommand;
            int length = letterLeftOut ? 0 : 1;
            for (int index = 0; index < arguments.Length; ++index) {
                string number = SvgNumbers.FormatInteger(arguments[index]);
                if ((index > 0 || letterLeftOut) && number[0] != '-')
                    ++length;
                length += number.Length;
            }
            return length;
        }

        // Write a command, leaving its letter out when it repeats the command before.
        // letter: the command letter.
        // arguments: its arguments.
        private void Write(char letter, long[] arguments)
        {
            bool letterLeftOut = letter == repeatableCommand;
            if (!letterLeftOut)
                text.Append(letter);
            for (int index = 0; index < arguments.Length; ++index) {
                string number = SvgNumbers.FormatInteger(arguments[index]);
                if ((index > 0 || letterLeftOut) && number[0] != '-')
                    text.Append(' ');
                text.Append(number);
            }

            // Coordinates that follow a move without a letter of their own are lines.
            if (letter == 'M')
                repeatableCommand = 'L';
            else if (letter == 'm')
                repeatableCommand = 'l';
            else
                repeatableCommand = letter;
        }
    }
}

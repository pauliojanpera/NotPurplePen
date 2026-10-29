using System.Collections.Generic;
using PointF = System.Drawing.PointF;

namespace PurplePen.Graphics2D
{
    // A course that a map of a document draws, as an export describes it to a target that keeps the structure of what
    // it draws (CD-1). Plain data, as the graphics layer knows nothing of courses.
    public sealed class CourseRecord
    {
        // The course's name.
        public string Name;

        // The part of the course the map draws, as its number from 1, or 0 for the whole course.
        public int Part;

        // The variation the map draws, as its code string, or null for a course without variations.
        public string Variation;

        // The scale the course prints at, as 10000 for 1:10,000.
        public float PrintScale;

        // The course's controls in the order the course visits them.
        public readonly List<CourseControlRecord> Controls = new List<CourseControlRecord>();
    }

    // A control of a course, in the order the course visits it (CD-1).
    public sealed class CourseControlRecord
    {
        // The control's code, or null for a control without one, such as a start.
        public string Code;

        // What kind of control it is: start, control, finish, crossing or map-exchange.
        public string Kind;

        // Where the control lies, in map coordinates.
        public PointF Position;
    }

    // What an object that a course adds to its map draws (CD-2): its kind, and the controls of its course it belongs
    // to, by their places in the course's list of controls, from 1.
    public sealed class CourseObjectRecord
    {
        // What the object draws: control, start, finish, control-number, leg, description, and so on.
        public string Kind;

        // The places of the controls it belongs to: one for a control circle, start, finish or control number, two for a
        // leg, none for an object that belongs to no control.
        public int[] Controls;
    }
}

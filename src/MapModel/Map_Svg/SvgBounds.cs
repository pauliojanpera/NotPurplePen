using System;

namespace PurplePen.MapModel
{
    // The bounds of what a node draws, in the coordinates of a frame: how far its points reach from the frame's origin
    // decides how many digits a transform of them needs (S-2).
    public sealed class SvgBounds
    {
        // The smallest and the largest coordinates included, or infinities while the bounds are empty.
        public double MinX = double.PositiveInfinity, MinY = double.PositiveInfinity;
        public double MaxX = double.NegativeInfinity, MaxY = double.NegativeInfinity;

        // Whether no point has been included.
        public bool IsEmpty
        {
            get { return MinX > MaxX; }
        }

        // Include a point.
        // x, y: the point.
        public void Include(double x, double y)
        {
            MinX = Math.Min(MinX, x);
            MinY = Math.Min(MinY, y);
            MaxX = Math.Max(MaxX, x);
            MaxY = Math.Max(MaxY, y);
        }

        // Include other bounds of the same frame.
        // other: the bounds to include.
        public void Include(SvgBounds other)
        {
            if (other.IsEmpty)
                return;
            Include(other.MinX, other.MinY);
            Include(other.MaxX, other.MaxY);
        }

        // Include bounds of an inner frame, through the transform that takes it into this one: the corners of the
        // bounds, transformed, which bound everything inside them.
        // other: the bounds in the inner frame.
        // transform: the transform from the inner frame to this one.
        public void IncludeTransformed(SvgBounds other, SvgTransform transform)
        {
            if (other.IsEmpty)
                return;
            double x, y;
            transform.TransformPoint(other.MinX, other.MinY, out x, out y);
            Include(x, y);
            transform.TransformPoint(other.MaxX, other.MinY, out x, out y);
            Include(x, y);
            transform.TransformPoint(other.MinX, other.MaxY, out x, out y);
            Include(x, y);
            transform.TransformPoint(other.MaxX, other.MaxY, out x, out y);
            Include(x, y);
        }

        // How far the farthest point within the bounds lies from the frame's origin; zero while they are empty.
        public double ReachFromOrigin
        {
            get {
                if (IsEmpty)
                    return 0;
                double farthestX = Math.Max(Math.Abs(MinX), Math.Abs(MaxX));
                double farthestY = Math.Max(Math.Abs(MinY), Math.Abs(MaxY));
                return Math.Sqrt(farthestX * farthestX + farthestY * farthestY);
            }
        }
    }
}

using System.Collections.Generic;

namespace PurplePen.Graphics2D
{
    // Compares keys by identity, as graphics targets key their paths, brushes, pens and fonts: two keys are equal
    // only when they are the same object, whatever their own Equals says.
    public class IdentityComparer<T> : IEqualityComparer<T>
    {
        // Whether two keys are the same object.
        public bool Equals(T x, T y) {
            return ((object)x == (object)y);
        }

        // The hash code of the object itself, not of its value.
        public int GetHashCode(T obj) {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}

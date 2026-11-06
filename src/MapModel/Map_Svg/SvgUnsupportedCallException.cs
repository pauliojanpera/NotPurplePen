using System;

namespace PurplePen.MapModel
{
    // Raised by Svg_GraphicsTarget for a call it cannot honor, naming the call.
    public class Svg_UnsupportedCallException : NotSupportedException
    {
        // The name of the call that the target cannot honor: an IGraphicsTarget member, or one of the target's own.
        public string CallName { get; }

        // Create the exception for a call.
        // callName: the name of the call.
        public Svg_UnsupportedCallException(string callName)
            : base("The SVG target cannot honor " + callName + ".")
        {
            CallName = callName;
        }
    }
}

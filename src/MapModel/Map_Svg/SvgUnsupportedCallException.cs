using System;

namespace PurplePen.MapModel
{
    // Raised by Svg_GraphicsTarget for a call it cannot honor, naming the call, and, for a call it will never honor as
    // made, such as a blending other than darken (D-8), saying why.
    public class Svg_UnsupportedCallException : NotSupportedException
    {
        // The name of the call that the target cannot honor: an IGraphicsTarget member, or one of the target's own.
        public string CallName { get; }

        // Why the target will never honor the call as made, beginning with the requirement that says so; or null when
        // it does not honor the call yet.
        public string WhyNeverHonored { get; }

        // Create the exception for a call that the target does not honor yet.
        // callName: the name of the call.
        public Svg_UnsupportedCallException(string callName)
            : this(callName, null)
        {
        }

        // Create the exception for a call that the target will never honor as made, or does not honor yet.
        // callName: the name of the call.
        // whyNeverHonored: why the target will never honor it, beginning with the requirement that says so, or null
        //                  when it does not honor it yet.
        public Svg_UnsupportedCallException(string callName, string whyNeverHonored)
            : base("The SVG target cannot honor " + callName + (whyNeverHonored == null ? "." : ", and never will: " + whyNeverHonored + "."))
        {
            CallName = callName;
            WhyNeverHonored = whyNeverHonored;
        }
    }
}

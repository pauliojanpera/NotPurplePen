using System;

namespace PurplePen.Graphics2D
{
    // A graphics target that can defer a drawing: it keeps the drawing, and makes it later on the target on which it is
    // played back, as a recording graphics target does (RecordingGraphicsTarget). A deferred drawing reads its source when
    // it is made, at the playback, and not when it is deferred.
    public interface IDeferredDrawingTarget
    {
        // Defer a drawing. The playback makes it on its target, at its place among the other calls of this target.
        // draw: the drawing, which gets the target on which it draws.
        void DrawDeferred(Action<IGraphicsTarget> draw);
    }
}

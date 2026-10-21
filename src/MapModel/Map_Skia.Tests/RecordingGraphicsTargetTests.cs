using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Map_Skia.Tests
{
    using System.Drawing;
    using NUnit.Framework;
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;
    using RenderingTestCases;
    using SkiaSharp;
    using TestingUtils;

    // Tests for RecordingGraphicsTarget. Rendering tests record drawing, play it back, and compare
    // against the same baselines used by SimpleDrawingTests and RenderingTests.
    [TestFixture]
    public class RecordingGraphicsTargetTests
    {
        // Record each drawing case, play the recording back twice, and compare both images with the case's
        // baseline.
        [TestCaseSource(typeof(RenderingUtil), nameof(RenderingUtil.DrawingTestCases))]
        public void RecordAndPlayBack(DrawingCase drawingCase)
        {
            RenderingUtil.RecordingRenderingTest(drawingCase);
        }

        [Test]
        public void RecordingMapRender()
        {
            // Area symbols use pattern brushes; also tests the lightened (intensity) variant.
            // Renders both directly and through a RecordingGraphicsTarget.
            string fullname = TestUtil.GetTestFile(Path.Combine("skia_render", "isomarea.txt"));
            bool ok = RenderingUtil.VerifyTestFile(fullname, new RenderOptions(), true, false, true, false, false, 6, 12, 0, true);
            Assert.IsTrue(ok);
        }

        [Test]
        public void PlaybackBeforeEndRecordingThrows()
        {
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget())
            using (SKBitmap bitmap = new SKBitmap(10, 10))
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (Skia_GraphicsTarget target = new Skia_GraphicsTarget(canvas)) {
                Assert.IsTrue(recorder.IsRecording);
                Assert.Throws<InvalidOperationException>(() => recorder.Playback(target));
            }
        }

        [Test]
        public void DrawingAfterEndRecordingThrows()
        {
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget()) {
                object penKey = new object();
                recorder.CreatePen(penKey, CmykColor.FromCmyk(0, 0, 0, 1), 1, LineCapMode.Flat, LineJoinMode.Miter, 1);
                recorder.EndRecording();

                Assert.IsFalse(recorder.IsRecording);
                Assert.Throws<InvalidOperationException>(() => recorder.DrawLine(penKey, new PointF(0, 0), new PointF(1, 1)));
                Assert.Throws<InvalidOperationException>(() => recorder.CreateSolidBrush(new object(), CmykColor.FromCmyk(0, 0, 0, 1)));
                Assert.Throws<InvalidOperationException>(() => recorder.EndRecording());
            }
        }

        [Test]
        public void PlaybackAfterDisposeThrows()
        {
            RecordingGraphicsTarget recorder = new RecordingGraphicsTarget();
            recorder.EndRecording();
            recorder.Dispose();

            using (SKBitmap bitmap = new SKBitmap(10, 10))
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (Skia_GraphicsTarget target = new Skia_GraphicsTarget(canvas)) {
                Assert.Throws<ObjectDisposedException>(() => recorder.Playback(target));
            }
        }

        [Test]
        public void HasObjects()
        {
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget()) {
                object pathKey = new object(), brushKey = new object(), penKey = new object(), fontKey = new object();

                Assert.IsFalse(recorder.HasPath(pathKey));
                Assert.IsFalse(recorder.HasBrush(brushKey));
                Assert.IsFalse(recorder.HasPen(penKey));
                Assert.IsFalse(recorder.HasFont(fontKey));

                recorder.CreatePath(pathKey, new List<GraphicsPathPart> {
                    new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { new PointF(0, 0) }),
                    new GraphicsPathPart(GraphicsPathPartKind.Lines, new PointF[] { new PointF(1, 1) }) }, AreaFillMode.Winding);
                recorder.CreateSolidBrush(brushKey, CmykColor.FromCmyk(1, 0, 0, 0));
                recorder.CreatePen(penKey, brushKey, 1, LineCapMode.Flat, LineJoinMode.Miter, 1);
                recorder.CreateFont(fontKey, "Arial", 10, TextEffects.Regular);

                Assert.IsTrue(recorder.HasPath(pathKey));
                Assert.IsTrue(recorder.HasBrush(brushKey));
                Assert.IsTrue(recorder.HasPen(penKey));
                Assert.IsTrue(recorder.HasFont(fontKey));

                Assert.Throws<InvalidOperationException>(() => recorder.CreateSolidBrush(brushKey, CmykColor.FromCmyk(1, 0, 0, 0)));

                // Setting intensity destroys pens and brushes, but not paths and fonts.
                recorder.Intensity = 0.5F;
                Assert.AreEqual(0.5F, recorder.Intensity);
                Assert.IsTrue(recorder.HasPath(pathKey));
                Assert.IsFalse(recorder.HasBrush(brushKey));
                Assert.IsFalse(recorder.HasPen(penKey));
                Assert.IsTrue(recorder.HasFont(fontKey));
            }
        }

        [Test]
        public void ChangingInputsDoesNotAffectPlayback()
        {
            // Draw directly as the reference.
            SKBitmap expected = DrawToBitmap(grTarget => DrawMutableInputs(grTarget, false));

            // Record, then mutate all the inputs before playing back.
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget()) {
                DrawMutableInputs(recorder, true);
                recorder.EndRecording();

                SKBitmap actual = DrawToBitmap(grTarget => recorder.Playback(grTarget));
                Assert.AreEqual(expected.Bytes, actual.Bytes);
                actual.Dispose();
            }

            expected.Dispose();
        }

        [Test]
        public void KeysNotReferencedAfterEndRecording()
        {
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget()) {
                WeakReference[] keys = RecordWithKeys(recorder);
                recorder.EndRecording();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                foreach (WeakReference key in keys)
                    Assert.IsFalse(key.IsAlive);

                // Playback still works without the keys.
                DrawToBitmap(grTarget => recorder.Playback(grTarget)).Dispose();
            }
        }

        [Test]
        public void RecordedCallNamesAreTheCallsPlaybackMakes()
        {
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget())
            using (Skia_Bitmap bitmap = new Skia_Bitmap(new SKBitmap(4, 4))) {
                recorder.Intensity = 0.5F;

                IBrushTarget brushTarget = recorder.CreatePatternBrush(new SizeF(10, 10), 0, 10, 10);
                object penKey = new object(), patternKey = new object();
                brushTarget.CreatePen(penKey, CmykColor.FromCmyk(0, 1, 0, 0), 1, LineCapMode.Flat, LineJoinMode.Miter, 1);
                brushTarget.DrawLine(penKey, new PointF(0, 0), new PointF(5, 5));
                brushTarget.FinishBrush(patternKey);

                recorder.FillRectangle(patternKey, new RectangleF(0, 0, 50, 50));
                recorder.DrawBitmapPart(bitmap, 1, 1, 2, 2, new RectangleF(0, 0, 10, 10), BitmapScaling.NearestNeighbor);

                // The pattern brush is played back through CreatePatternBrush, and the bitmap part, recorded
                // cropped, through DrawBitmap.
                CollectionAssert.AreEqual(
                    new[] { "CreatePatternBrush", "CreatePen", "DrawBitmap", "DrawLine", "FillRectangle", "Intensity" },
                    recorder.RecordedCallNames);
            }
        }

        // Create objects with keys that are only referenced by the recorder. Returns weak references to the keys.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] RecordWithKeys(RecordingGraphicsTarget recorder)
        {
            object pathKey = new object(), brushKey = new object(), penKey = new object(), fontKey = new object(), patternKey = new object();

            recorder.CreatePath(pathKey, new List<GraphicsPathPart> {
                new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { new PointF(10, 10) }),
                new GraphicsPathPart(GraphicsPathPartKind.Lines, new PointF[] { new PointF(50, 50), new PointF(10, 50) }),
                new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]) }, AreaFillMode.Winding);
            recorder.CreateSolidBrush(brushKey, CmykColor.FromCmyk(1, 0, 0, 0));
            recorder.CreatePen(penKey, brushKey, 2, LineCapMode.Flat, LineJoinMode.Miter, 1);
            recorder.CreateFont(fontKey, "Arial", 10, TextEffects.Regular);

            IBrushTarget brushTarget = recorder.CreatePatternBrush(new SizeF(10, 10), 0, 10, 10);
            object innerBrushKey = new object();
            brushTarget.CreateSolidBrush(innerBrushKey, CmykColor.FromCmyk(0, 1, 0, 0));
            brushTarget.FillRectangle(innerBrushKey, new RectangleF(-2, -2, 4, 4));
            brushTarget.FinishBrush(patternKey);

            recorder.FillPath(patternKey, pathKey);
            recorder.DrawPath(penKey, pathKey);
            recorder.DrawText("A", fontKey, brushKey, new PointF(60, 60));

            return new WeakReference[] {
                new WeakReference(pathKey), new WeakReference(brushKey), new WeakReference(penKey),
                new WeakReference(fontKey), new WeakReference(patternKey), new WeakReference(innerBrushKey) };
        }

        // Draw using inputs that can be mutated after the drawing calls. If mutate is true, all inputs are changed after use.
        private static void DrawMutableInputs(IGraphicsTarget grTarget, bool mutate)
        {
            object penKey = new object(), brushKey = new object();
            grTarget.CreateSolidBrush(brushKey, CmykColor.FromCmyk(0, 1, 1, 0));
            grTarget.CreatePen(penKey, CmykColor.FromCmyk(1, 0, 0, 0), 3, LineCapMode.Flat, LineJoinMode.Miter, 1);

            Matrix matrix = new Matrix();
            matrix.Translate(5, 5);
            grTarget.PushTransform(matrix);

            PointF[] pts = { new PointF(10, 10), new PointF(80, 20), new PointF(40, 80) };
            grTarget.DrawPolyline(penKey, pts);

            List<GraphicsPathPart> parts = new List<GraphicsPathPart> {
                new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { new PointF(50, 50) }),
                new GraphicsPathPart(GraphicsPathPartKind.Lines, new PointF[] { new PointF(90, 50), new PointF(90, 90) }),
                new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]) };
            grTarget.FillPath(brushKey, parts, AreaFillMode.Winding);

            RectangleF[] clipRects = { new RectangleF(0, 0, 40, 100) };
            grTarget.PushClip(clipRects);
            grTarget.FillRectangle(brushKey, new RectangleF(0, 0, 100, 100));
            grTarget.PopClip();

            grTarget.PopTransform();

            if (mutate) {
                matrix.Translate(30, 30);
                pts[1] = new PointF(0, 99);
                parts[1].Points[0] = new PointF(0, 0);
                parts.Clear();
                clipRects[0] = new RectangleF(0, 0, 100, 100);
            }
        }

        // Create a bitmap and draw on it with a Skia_GraphicsTarget.
        private static SKBitmap DrawToBitmap(Action<IGraphicsTarget> draw)
        {
            SKBitmap bitmap = new SKBitmap(100, 100);
            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                canvas.Clear(SKColors.White);
                using (Skia_GraphicsTarget grTarget = new Skia_GraphicsTarget(canvas)) {
                    draw(grTarget);
                }
            }
            return bitmap;
        }
    }
}

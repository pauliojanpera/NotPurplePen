using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using RenderingTestCases;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // A diagnosis, not part of the loop's tests: draws each drawing case and each recorded map check with Skia
    // directly and through a RecordingGraphicsTarget, and compares the two images on this machine.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class RecordingFidelityTests
    {
        // Make a test case of each drawing case.
        private static IEnumerable<DrawingCase> Drawings()
        {
            return DrawingCases.All;
        }

        // Make a test case of each map check that the Skia tests also draw through a recording.
        private static IEnumerable<TestCaseData> RecordedMapChecks()
        {
            int number = 0;
            foreach (MapRenderingCheck check in MapRenderingTestCases.All.SelectMany(test => test.Checks).Where(check => check.AlsoTestRecording))
                yield return new TestCaseData(check).SetName("MapCheck(" + (++number) + " " + check.DescriptorFileName + ")");
        }

        [TestCaseSource(nameof(Drawings))]
        public void DrawingPlaysBackAsDrawn(DrawingCase drawingCase)
        {
            Size pixelSize = new Size(drawingCase.PixelWidth, drawingCase.PixelHeight);
            using (SKBitmap direct = DrawDrawing(drawingCase, pixelSize, target => drawingCase.Draw(target)))
            using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget()) {
                drawingCase.Draw(recorder);
                recorder.EndRecording();
                using (SKBitmap played = DrawDrawing(drawingCase, pixelSize, target => recorder.Playback(target))) {
                    ImageComparison.AssertImagesMatch("fidelity-" + drawingCase.Name, direct, played, 0);
                }
            }
        }

        [TestCaseSource(nameof(RecordedMapChecks))]
        public void MapPlaysBackAsDrawn(MapRenderingCheck check)
        {
            RenderingDescriptor descriptor = RenderingDescriptor.Read(check.DescriptorPath);
            using (Map map = descriptor.LoadMap()) {
                foreach (float intensity in check.TestLightenedColor ? new[] { 1.0F, 0.4F } : new[] { 1.0F }) {
                    using (SKBitmap direct = DrawMap(descriptor, intensity, target => MapDrawing.DrawMapArea(target, map, descriptor.MapArea, descriptor.PixelSize,
                                                     check.CreateRenderOptions(), check.UsePatternBitmaps, check.BlendOverprintedColors, check.AntiAlias)))
                    using (RecordingGraphicsTarget recorder = new RecordingGraphicsTarget(intensity)) {
                        MapDrawing.DrawMapArea(recorder, map, descriptor.MapArea, descriptor.PixelSize,
                                               check.CreateRenderOptions(), check.UsePatternBitmaps, check.BlendOverprintedColors, check.AntiAlias);
                        recorder.EndRecording();
                        using (SKBitmap played = DrawMap(descriptor, intensity, target => recorder.Playback(target))) {
                            ImageComparison.AssertImagesMatch("fidelity-" + System.IO.Path.GetFileNameWithoutExtension(check.DescriptorFileName) + "-" + intensity, direct, played, 0);
                        }
                    }
                }
            }
        }

        // Draw a drawing case on a white bitmap with Skia.
        private static SKBitmap DrawDrawing(DrawingCase drawingCase, Size pixelSize, Action<IGraphicsTarget> draw)
        {
            SKBitmap bitmap = new SKBitmap(pixelSize.Width, pixelSize.Height);
            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                canvas.Clear(SKColors.White);
                using (Skia_GraphicsTarget target = new Skia_GraphicsTarget(canvas))
                using (Matrix transform = AreaInImage.CreateTransform(drawingCase.Area, pixelSize, false)) {
                    target.PushTransform(transform);
                    draw(target);
                }
            }
            return bitmap;
        }

        // Draw a map area on a white bitmap with Skia, as the map rendering tests do.
        private static SKBitmap DrawMap(RenderingDescriptor descriptor, float intensity, Action<IGraphicsTarget> draw)
        {
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(descriptor.PixelSize.Width, descriptor.PixelSize.Height, false,
                                                                                    CmykColor.FromCmyk(0, 0, 0, 0), descriptor.MapArea, true, null, intensity)) {
                draw(target);
                Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap();
                return bitmap.Bitmap;
            }
        }
    }
}

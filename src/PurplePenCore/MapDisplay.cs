/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using PurplePen.MapModel;
using ColorMatrix = PurplePen.MapModel.ColorMatrix;
using PurplePen.Graphics2D;
using System.IO;
using System.Linq;


namespace PurplePen
{
    // This interface encapsulates something that can be cached for drawing in the ViewCache. It
    // needs to be able to draw itself, and also notify when it has changed. The graphics is always
    // set up in world coordinates, as it the visible rectangle and the changedRegion.
    public interface IMapDisplay
    {
        RectangleF Bounds { get; }
        void Draw(IGraphicsBitmap bitmap, Matrix transform, RectangleF? clipRect);
        void Draw(IGraphicsTarget grTarget, RectangleF visRect, float minResolution, Action throwOnCancel);

        event MapDisplayChanged Changed;
    }

    public delegate void MapDisplayChanged();

    public enum ColorModel { RGB, CMYK };

    // The layers a MapDisplay draws a page in, from the bottom up: the map up to the color that the course's lower layer
    // goes above, the course's lower layer, the rest of the map, and the rest of the course. Without a lower purple
    // layer, the first is the whole map and the second the whole course, and the other two draw nothing.
    public enum MapDisplayLayer { MapBelowCourse, CourseLower, MapAboveCourse, CourseUpper };


    // The map display represents everything that is cached in the ViewCache and normally shown
    // on the map. It includes the map proper, as well as the courses and so forth drawn on top.
    // The IMapDisplay interface is the communication channel with the ViewCache and the MapViewer
    // controls -- it simply has to be able to draw itself, and notify when parts change.
    public class MapDisplay : IMapDisplay, IDisposable
    {
        MapType mapType;    // Map type. Note that OpenMapper and OCAD files both called MapType.OCAD. See MapVersion property to distinguish.
        string filename;

        // Used for MapType.Bitmap or MapType.PDF
        IGraphicsBitmap bitmap;     // the bitmap
        IGraphicsBitmap dimmedBitmap;  // the dimmed bitmap.
        float bitmapDpi;     // dpi for bitmap

        MapFileFormat mapVersion;       // OCAD version. (OCAD maps only)
        Map map;                // The map to draw. (OCAD maps only)

        PdfMapFile pdfMapFile;  // pdfMapFile (PDF maps only)

        CourseLayout course;    // The course to display.
        Map courseMap;              // The courses, rendered into a map.

        float mapIntensity = 1.0F;   // Intensity to display the map at.
        ColorModel colorModel = ColorModel.CMYK; // color model to use (cannot be OCADCompatible)
        bool antialiased = false;        // anti-alias (high quality) the map display?
        bool ocadOverprintEffect = false; // overprint effect for colors in the OCAD map (not the purple of the course on top).
        bool showBounds = false;       // show symbols bounds (for testing)

        int? lowerPurpleMapLayer;      // If non-null, place the lower purple of the map just above this OCAD ID color.

        RectangleF? printArea;          // print area to display, or null for none.
        private bool disposed = false;

        // Clones this map display.
        public MapDisplay Clone()
        {
            MapDisplay newMapDisplay = (MapDisplay) MemberwiseClone();

            newMapDisplay.dimmedBitmap = null;         // clones should not share dimmed bitmaps.
            newMapDisplay.UpdateDimmedBitmap();

            return newMapDisplay;
        }

        // Dispose managed resources used by the MapDisplay.
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) {
                try {
                    if (bitmap != null) {
                        bitmap.Dispose();
                        bitmap = null;
                    }

                    if (dimmedBitmap != null) {
                        dimmedBitmap.Dispose();
                        dimmedBitmap = null;
                    }

                    if (map != null) {
                        map.Dispose();
                        map = null;
                    }

                    pdfMapFile = null;
                    mapType = MapType.None;     // the map data is gone, so don't claim to have a map.

                    if (courseMap != null) {
                        courseMap.Dispose();
                        courseMap = null;
                    }
                }
                catch {
                    // Swallow exceptions during dispose to avoid throwing from Dispose.
                }
            }

            disposed = true;
        }

        // Clones this map display, and set the clone to full intensity
        public MapDisplay CloneToFullIntensity()
        {
            MapDisplay newMapDisplay = (MapDisplay)MemberwiseClone();

            newMapDisplay.dimmedBitmap = null;         // clones should not share dimmed bitmaps.
            newMapDisplay.mapIntensity = 1;

            return newMapDisplay;
        }

        // Map type we're drawing.
        public MapType MapType
        {
            get
            {
                return mapType;
            }
        }

        // File name of the map.
        public string FileName
        {
            get
            {
                return filename;
            }
        }

        // OCAD version of the map.
        public MapFileFormat MapVersion
        {
            get
            {
                return mapVersion;
            }
        }

        // Scale of the map
        public float MapScale
        {
            get
            {
                if (map != null) {
                    using (map.Read())
                        return map.MapScale;
                }
                else
                    return 0;
            }
        }

        // Dpi of the bitmap
        public float Dpi
        {
            get
            {
                if (mapType == MapType.Bitmap)
                    return bitmapDpi;
                else
                    return 0;
            }

            set
            {
                bitmapDpi = value;
                RaiseChanged();        // redraw everything.
            }
        }

        // Real world coordinates
        public RealWorldCoords RealWorldCoords
        {
            get
            {
                if (map != null) {
                    using (map.Read())
                        return map.RealWorldCoords;
                }
                else {
                    return new RealWorldCoords();
                }
            }
        }

        // Bounds of the map, or empty if no map.
        public RectangleF MapBounds
        {
            get
            {
                switch (mapType) {
                case MapType.Bitmap:
                case MapType.PDF:
                    if (bitmap == null)
                        return new RectangleF();
                    int pixelWidth = bitmap.PixelWidth;
                    int pixelHeight = bitmap.PixelHeight;
                    if (pixelWidth == 0 || pixelHeight == 0) {
                        return new RectangleF();
                    }
                    else {
                        return Geometry.TransformRectangle(BitmapTransform(bitmap), new RectangleF(0, 0, pixelWidth, pixelHeight));
                    }

                    case MapType.OCAD:
                    if (map != null) {
                        using (map.Read())
                            return map.GetBoundsIncludingTemplates();
                    }
                    else
                        return new RectangleF();

                case MapType.None:
                    return new RectangleF();

                default:
                    Debug.Fail("bad maptype");
                    return new RectangleF();
                }
            }
        }

        // Bounds of both the map and the course.
        public RectangleF Bounds
        {
            get
            {
                RectangleF mapBounds = MapBounds;
                RectangleF courseBounds = new RectangleF();
                
                if (courseMap != null) {
                    using (courseMap.Read())
                        courseBounds = courseMap.Bounds;
                }

                if (mapBounds.IsEmpty)
                    return courseBounds;
                else if (courseBounds.IsEmpty)
                    return mapBounds;
                else
                    return RectangleF.Union(mapBounds, courseBounds);
            }
        }


        public List<string> GetReferencedFiles()
        {
             if (mapType == MapType.OCAD && map != null) {
                using (map.Read()) {
                    return map.GetReferencedFiles(1);
                }
            }
            else {
                return new List<string>();
            }
        }

        // Colors in the map, or empty list if no map or bitmap map.
        public List<SymColor> GetMapColors()
        {
            if (mapType == MapType.OCAD && map != null) {
                using (map.Read())
                    return new List<SymColor>(map.AllColors);
            }
            else {
                return new List<SymColor>();
            }
        }

        // Templates in the map, or empty list in no map or bitmap map.
        public IList<TemplateInfo> GetMapTemplates()
        {
            if (mapType == MapType.OCAD && map != null) {
                using (map.Read())
                    return map.Templates;
            }
            else {
                return new List<TemplateInfo>();
            }
        }

        // Get a coordinate mapper for the map, or null if mapping coordinates isn't possible.
        public CoordinateMapper CoordinateMapper
        {
            get {
                if (mapType == MapType.OCAD && map != null)
                    return new CoordinateMapper(map);
                else
                    return null;
            }
        }

        // Intensity to draw the map at.
        public float MapIntensity
        {
            get
            {
                return mapIntensity;
            }

            set
            {
                if (MapIntensity != value) {
                    mapIntensity = value;
                    UpdateDimmedBitmap();
                    RaiseChanged();
                }
            }
        }

        // Color model to use
        public ColorModel ColorModel
        {
            get { return colorModel; }
            set
            {
                if (colorModel != value) {
                    colorModel = value;
                    RaiseChanged();
                }
            }
        }

        public GraphicsBitmapFormat BitmapImageFormat
        {
            get
            {
                if (MapType == MapType.Bitmap && bitmap != null)
                    return bitmap.GetOriginalFormat();
                else
                    return GraphicsBitmapFormat.None;
            }
        }


        // Missing fonts?
        public string[] MissingFonts()
        {
            if (mapType == MapType.OCAD)
                return map.MissingFonts;
            else
                return null;
        }

        // Nonrenderable objects?
        public string[] NonRenderableObjects()
        {
            if (mapType == MapType.OCAD)
                return map.NotRenderableObjects;
            else
                return null;
        }

        // Anti-alias the map display?
        public bool AntiAlias
        {
            get
            {
                return antialiased;
            }
            set
            {
                if (antialiased != value) {
                    antialiased = value;
                    RaiseChanged();
                }
            }
        }

        // User Overprint effect for OCAD map
        public bool OcadOverprintEffect
        {
            get
            {
                return ocadOverprintEffect;
            }
            set
            {
                if (ocadOverprintEffect != value) {
                    ocadOverprintEffect = value;
                    RaiseChanged();
                }
            }
        }

        // If non-null, place the lower purple of the map just above this OCAD ID color.
        public int? LowerPurpleMapLayer {
            get {
                return lowerPurpleMapLayer;
            }
            set {
                if (lowerPurpleMapLayer != value) {
                    lowerPurpleMapLayer = value;
                    RaiseChanged();
                }
            }
        }


        // Are we printing?
        public bool Printing { get; set; }

        // Whether drawing for printing keeps the anti-aliasing of drawing for the screen: the map anti-aliased as
        // AntiAlias says, and the course always. Off by default, so that printing turns anti-aliasing off. Printing
        // still draws without pattern bitmaps, unless the target's pattern brushes are vectors.
        public bool AntiAliasWhenPrinting { get; set; }

        // Whether area patterns are drawn element by element on every target, as printing draws them on a target whose
        // pattern brushes are bitmaps: each pattern glyph by glyph and each hatching line by line. Off by default, so
        // that a target whose pattern brushes are vectors draws the regular patterns in clip mode, and the hatchings,
        // with pattern brushes.
        public bool AreaPatternsElementByElement { get; set; }

        // Whether drawing turns anti-aliasing off because it is for printing: Printing is on and AntiAliasWhenPrinting
        // is off.
        private bool PrintingWithoutAntiAliasing
        {
            get { return Printing && !AntiAliasWhenPrinting; }
        }

        // Anti-alias the map display?
        public bool ShowSymbolBounds
        {
            get
            {
                return showBounds;
            }
            set
            {
                if (showBounds != value) {
                    showBounds = value;
                    RaiseChanged();
                }
            }
        }

        // Get a color matrix from the current map intensity value. Can return null if intensity is 1.0.
        ColorMatrix ComputeColorMatrix()
        {
            if (mapIntensity < 0.99F) {
                float[][] colorMatrixElements = { 
                           new float[] {(float)mapIntensity,  0,  0,  0, 0},
                           new float[] {0,  (float)mapIntensity,  0,  0, 0},
                           new float[] {0,  0,  (float)mapIntensity,  0, 0},
                           new float[] {0,  0,  0,  1, 0},
                           new float[] {(float) (1-mapIntensity), (float) (1-mapIntensity), (float) (1-mapIntensity), 0, 1}};
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                return colorMatrix;
            }
            else
                return null;            // full intensity.
        }

        // Computer the transform from coordinates of the bitmap to world coordinates
        // bitmap: the bitmap map being drawn.
        Matrix BitmapTransform(IGraphicsBitmap bitmap)
        {
            // (worldcoord in mm) / 25.4F * dpi = pixels
            float scaleFactor = bitmapDpi / 25.4F;

            Matrix matrix = new Matrix();
            matrix.Translate(0, bitmap.PixelHeight);
            matrix.Scale(scaleFactor, -scaleFactor);
            matrix.Invert();
            return matrix;
        }

        // True while SetMapFile is loading a map file. Loading a PDF map pumps the UI thread (to show the
        // "Reading PDF" dialog), so idle-time UI code can run in the middle of a load; it should check this
        // and wait for the load to finish rather than act on the map display.
        public bool IsLoadingMap { get; private set; }

        // Set the map file used to draw.
        // The new map is loaded into locals and only stored into the fields once loading has fully succeeded,
        // so the fields are always mutually consistent (a Bitmap/PDF map type always has a bitmap, an OCAD map
        // type always has a map), both while the load is in progress and if it throws.
        // newMapType: the kind of map file.
        // newFilename: the map file to load, or null if newMapType is None.
        public void SetMapFile(MapType newMapType, string newFilename)
        {
            // Show no map while loading. The file name is set immediately, because Controller.MapDisplay
            // starts a reload whenever the file name doesn't match the event; setting it now prevents a
            // nested reload if UI code runs while the load is pumping messages.
            // Old maps/bitmaps are not disposed, because a background draw may still be using them.
            this.mapType = MapType.None;
            this.filename = newFilename;
            this.map = null;
            this.bitmap = null;
            this.pdfMapFile = null;
            this.mapVersion = new MapFileFormat(MapFileFormatKind.None, 0);
            UpdateDimmedBitmap();

            MapType loadedMapType = newMapType;
            Map loadedMap = null;
            MapFileFormat loadedMapVersion = new MapFileFormat(MapFileFormatKind.None, 0);
            IGraphicsBitmap loadedBitmap = null;
            float loadedBitmapDpi = bitmapDpi;
            PdfMapFile loadedPdfMapFile = null;

            IsLoadingMap = true;
            try {
                if (newMapType == MapType.None) {
                    // Nothing to load.
                }
                else if (newMapType == MapType.OCAD) {
                    loadedMap = new Map(MapUtil.TextMetricsProvider, Services.FileLoaderProvider.GetFileLoaderForDirectory(Path.GetDirectoryName(newFilename)));
                    loadedMapVersion = InputOutput.ReadFile(newFilename, loadedMap);
                }
                else if (newMapType == MapType.Bitmap) {
                    using (Stream stream = new FileStream(newFilename, FileMode.Open, FileAccess.Read)) {
                        loadedBitmap = Services.BitmapLoader.ReadBitmapFromStream(stream);
                    }

                    loadedBitmapDpi = (float) loadedBitmap.HorizontalResolution;
                }
                else if (newMapType == MapType.PDF) {
                    string errorText;
                    Size bitmapSize;

                    loadedPdfMapFile = CoreMapUtil.ValidatePdf(newFilename, out loadedBitmapDpi, out bitmapSize, out errorText);

                    if (loadedPdfMapFile == null) {
                        loadedMapType = MapType.None;
                    }
                    else {
                        using (Stream stream = new FileStream(loadedPdfMapFile.PngFileName, FileMode.Open, FileAccess.Read)) {
                            loadedBitmap = Services.BitmapLoader.ReadBitmapFromStream(stream);
                        }
                    }
                }
                else {
                    Debug.Fail("bad maptype");
                    loadedMapType = MapType.None;
                }
            }
            finally {
                IsLoadingMap = false;
            }

            // Loading succeeded; publish the new map. Set the bitmap and map before the map type, so a
            // background draw that snapshots the fields never sees a map type without its data.
            this.bitmapDpi = loadedBitmapDpi;
            this.bitmap = loadedBitmap;
            this.map = loadedMap;
            this.pdfMapFile = loadedPdfMapFile;
            this.mapVersion = loadedMapVersion;
            this.mapType = loadedMapType;

            UpdateDimmedBitmap();
            RaiseChanged();        // redraw everything.
        }


        // Whether setting a course records which course object added each symbol of the course's map, as an export that
        // says what each object it draws of the course is asks (CD-3). Off by default, as the display has no use for it.
        public bool RecordsCourseObjects { get; set; }

        // For each symbol of the course's map, the course object that added it, recorded when the course was set if
        // RecordsCourseObjects was on; else empty.
        public IReadOnlyDictionary<PurplePen.MapModel.Symbol, CourseObj> CourseObjectsOfSymbols
        {
            get { return courseObjectOfSymbol; }
        }

        // The course object that added each symbol of the course's map, when recorded.
        private Dictionary<PurplePen.MapModel.Symbol, CourseObj> courseObjectOfSymbol = new Dictionary<PurplePen.MapModel.Symbol, CourseObj>();

        // Set the courses being displayed.
        public void SetCourse(CourseLayout newCourse)
        {
            if (! object.Equals(course, newCourse)) {
                // The ordering of layers depends on whether we have a lower purple map layer.
                CourseLayout.MapRenderOptions renderOptions = new CourseLayout.MapRenderOptions();
                renderOptions.LowerColorsBelowWhiteOut = (LowerPurpleMapLayer != null);

                course = newCourse;
                courseObjectOfSymbol = new Dictionary<PurplePen.MapModel.Symbol, CourseObj>();
                if (course == null)
                    courseMap = null;
                else
                    courseMap = course.RenderToMap(renderOptions, RecordsCourseObjects ? courseObjectOfSymbol : null);

                RaiseChanged();
            }
        }

        // Set the print area to display, or null to not display print area.
        public void SetPrintArea(RectangleF? printArea)
        {
            if (!this.printArea.Equals(printArea)) {
                this.printArea = printArea;
                RaiseChanged();
            }
        }

        // Update the dimmed bitmap. If the intensity is < 1 and we're using a bitmap, dim it.
        public void UpdateDimmedBitmap()
        {
            if (dimmedBitmap != null)
                dimmedBitmap.Dispose();
            dimmedBitmap = null;

            // Only dim bitmap if size isn't too large. Otherwise takes too much memory.
            if ((mapType == MapType.Bitmap || mapType == MapType.PDF) && bitmap != null && mapIntensity < 0.99F && (bitmap.PixelWidth * bitmap.PixelHeight) < 36000000) {
                // ColorConverter doesn't actually matter since we are just drawing a bitmap.
                using (IBitmapGraphicsTarget grTarget = Services.BitmapGraphicsTargetProvider.CreateBitmapGraphicsTarget(bitmap.PixelWidth, bitmap.PixelHeight, CmykColor.FromCmyk(0, 0, 0, 0), DefaultColorConverter.Instance)) {
                    grTarget.Intensity = mapIntensity;
                    grTarget.DrawBitmap(bitmap, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), BitmapScaling.NearestNeighbor);
                    dimmedBitmap = grTarget.FinishBitmap();
                }
            }
            else {
                dimmedBitmap = null;
            }
        }


        // Draw the ocad map part.
        // map: the OCAD map to draw. Passed in (rather than read from the field) so that the map
        //      we lock is guaranteed to be the map we draw, even if SetMapFile runs on another thread.
        void DrawOcadMap(Map map, IGraphicsTarget grTarget, RectangleF visRect, RenderOptions renderOptions, Action throwOnCancel)
        {
            using (map.Write()) {
                map.Draw(grTarget, visRect, renderOptions, throwOnCancel);
            }
        }

        // Draw the bitmap map part.
        // bitmap, dimmedBitmap: the bitmap map and its dimmed version (may be null), snapshotted by the caller.
        void DrawBitmapMap(IGraphicsBitmap bitmap, IGraphicsBitmap dimmedBitmap, IGraphicsTarget grTarget, RectangleF visRect, float minResolution)
        {
            // Setup transform.
            grTarget.PushTransform(BitmapTransform(bitmap));

            // Setup drawing map and intensity.
            BitmapScaling scalingMode = antialiased ? BitmapScaling.MediumQuality : BitmapScaling.NearestNeighbor;
            if (bitmap.PixelHeight * bitmap.PixelWidth > 50000000)
                scalingMode = BitmapScaling.NearestNeighbor;            // Turn off high quality scaling for very large bitmaps.

            // Get source bitmap. Use the dimmed bitmap if there is one.
            IGraphicsBitmap sourceBitmap;
            if (dimmedBitmap != null)
                sourceBitmap = dimmedBitmap;
            else
                sourceBitmap = bitmap;

            // Draw it.
            grTarget.DrawBitmap(sourceBitmap, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), scalingMode);

            if (mapIntensity < 0.99 && sourceBitmap == bitmap) {
                // Dimming desired, but we don't have a dimmed bitmap. Use an alpha mask instead.
                CmykColor dimmedWhite = CmykColor.FromCmyka(0, 0, 0, 0, 1-mapIntensity);
                object brush = new object();
                grTarget.CreateSolidBrush(brush, dimmedWhite);
                grTarget.FillRectangle(brush, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }

            // Pop transform
            grTarget.PopTransform();
        }

        // For bitmap or PDF background, write the bitmap to the given file with the given format.
        public void WriteBitmapMap(string fileName, GraphicsBitmapFormat format, out float dpi)
        {
            dpi = this.bitmapDpi;

            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                bitmap.WriteToStream(format, stream, 100);
            }
        }

        float GetMinResolution(Matrix transform)
        {
            Matrix m = transform.Clone();
            m.Invert();
            return Geometry.TransformDistance(1, m);
        }



        private short? ColorIdBelow(Map map, short colorId)
        {
            using (map.Read()) {
                List<SymColor> allColors = map.AllColors.ToList();
                int index = allColors.FindIndex(sc => sc.OcadId == colorId);
                return (index > 0) ? allColors[index - 1].OcadId : (short?)null;
            }
        }

        // Draw the map and course onto a graphics target. The color model is ignored, because the color model
        // in the IGraphicsTarget is used. The graphics target should support blending to get the right purple
        // blending to be performed.
        public void Draw(IGraphicsTarget grTarget, RectangleF visRect, float minResolution, Action throwOnCancel)
        {
            DrawHelper(grTarget, visRect, minResolution, throwOnCancel);
        }



        // Draw the map and course onto a bitmap of the given size. The given rectangle is mapped onto the whole bitmap, then
        // the given clip region is applied.
        // Draw the map and course onto a bitmap of the given size. The given rectangle is mapped onto the whole bitmap, then
        // the given clip region is applied.
        public void Draw(IGraphicsBitmap bitmap, Matrix transform, RectangleF? clipRect = null)
        {
            Debug.Assert(colorModel == ColorModel.CMYK || colorModel == ColorModel.RGB);
            IColorConverter colorConverter = (colorModel == ColorModel.CMYK) ? (IColorConverter) SwopColorConverter.Instance : (IColorConverter) DefaultColorConverter.Instance;

            if (bitmap.PixelHeight == 0 || bitmap.PixelWidth == 0)
                return;

            Debug.Assert(!bitmap.MustCopyBitsForGraphicsTarget, "Bitmap must allow non-copying graphics target");

            using (IBitmapGraphicsTarget grTarget = bitmap.GetGraphicsTarget(false, colorConverter)) {
                // Clear the bitmap with white.
                RectangleF entireBitmapRect = RectangleF.FromLTRB(-10, -10, bitmap.PixelWidth + 10, bitmap.PixelHeight + 10);
                object whiteBrush = new object();
                grTarget.CreateSolidBrush(whiteBrush, CmykColor.FromCmyk(0, 0, 0, 0));
                grTarget.FillRectangle(whiteBrush, entireBitmapRect);

                grTarget.PushTransform(transform);
                float minResolution = GetMinResolution(transform);
                Matrix transformInverse = transform.Clone();
                transformInverse.Invert();
                RectangleF clipBounds = Geometry.TransformRectangle(transformInverse, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));

                DrawHelper(grTarget, clipBounds, minResolution, null);

                grTarget.FinishBitmap();
            }
        }

        // Draw one layer of the map and course onto a graphics target, as Draw draws it among the others, so that a target
        // that keeps the structure of what it draws can hold the layers apart.
        // grTarget: the target to draw onto.
        // layer: the layer to draw.
        // visRect: the area of the map to draw, in map coordinates.
        // minResolution: the smallest distance worth drawing, in map coordinates.
        // throwOnCancel: called now and then to throw if drawing is to stop, or null.
        public void Draw(IGraphicsTarget grTarget, MapDisplayLayer layer, RectangleF visRect, float minResolution, Action throwOnCancel)
        {
            DrawSnapshot snapshot = TakeDrawSnapshot();
            DrawLayer(grTarget, layer, snapshot, visRect, CreateRenderOptions(grTarget, minResolution), ColorIdBelowWhiteOut(snapshot), throwOnCancel);
        }

        // Draw the map and course onto a graphics. A helper for the other two draw methods.

        private void DrawHelper(IGraphicsTarget grTarget, RectangleF visRect, float minResolution, Action throwOnCancel)
        {
            if (throwOnCancel != null) { throwOnCancel(); }

            // Read the fields that the UI thread can change while we draw on a background thread once, so that every
            // layer draws the same maps (see DrawSnapshot).
            DrawSnapshot snapshot = TakeDrawSnapshot();
            RenderOptions renderOptions = CreateRenderOptions(grTarget, minResolution);
            short? colorIdBelowWhiteOut = ColorIdBelowWhiteOut(snapshot);

            // First draw the real map, then the course map on top, and when the course has a lower layer, the rest of the
            // map and then the rest of the course.
            DrawLayer(grTarget, MapDisplayLayer.MapBelowCourse, snapshot, visRect, renderOptions, colorIdBelowWhiteOut, throwOnCancel);
            DrawLayer(grTarget, MapDisplayLayer.CourseLower, snapshot, visRect, renderOptions, colorIdBelowWhiteOut, throwOnCancel);
            if (HasLayersAboveCourseIn(snapshot)) {
                DrawLayer(grTarget, MapDisplayLayer.MapAboveCourse, snapshot, visRect, renderOptions, colorIdBelowWhiteOut, throwOnCancel);
                if (throwOnCancel != null) { throwOnCancel(); }
                DrawLayer(grTarget, MapDisplayLayer.CourseUpper, snapshot, visRect, renderOptions, colorIdBelowWhiteOut, throwOnCancel);
            }

            if (throwOnCancel != null) { throwOnCancel(); }
            DrawPrintAreaOutline(grTarget, visRect);
        }

        // The fields that SetMapFile, SetCourse and LowerPurpleMapLayer can change on the UI thread while a background
        // thread draws, read once for a drawing. Each Map must be locked and drawn through the same reference; re-reading
        // the field after taking the lock can draw a different Map whose lock this thread does not hold (MapUsageException).
        // Old maps and bitmaps are not disposed when replaced, so a snapshot stays usable.
        private struct DrawSnapshot
        {
            public MapType mapType;
            public Map map;
            public Map courseMap;
            public IGraphicsBitmap bitmap;
            public IGraphicsBitmap dimmedBitmap;
            public int? lowerPurpleMapLayer;
        }

        // Read the fields a drawing draws from, for all of its layers.
        private DrawSnapshot TakeDrawSnapshot()
        {
            DrawSnapshot snapshot = new DrawSnapshot();
            snapshot.mapType = this.mapType;
            snapshot.map = this.map;
            snapshot.courseMap = this.courseMap;
            snapshot.bitmap = this.bitmap;
            snapshot.dimmedBitmap = this.dimmedBitmap;
            snapshot.lowerPurpleMapLayer = this.lowerPurpleMapLayer;
            return snapshot;
        }

        // Whether the map and the course are drawn in four layers rather than two: the map is an OCAD map, and the course
        // has a lower layer, which goes between two parts of the map. Without, MapAboveCourse and CourseUpper draw nothing.
        public bool HasLayersAboveCourse
        {
            get { return HasLayersAboveCourseIn(TakeDrawSnapshot()); }
        }

        // Whether a drawing from a snapshot draws four layers rather than two, as HasLayersAboveCourse says. The map can
        // be null briefly while SetMapFile is switching maps, and then draws nothing.
        // snapshot: the fields the drawing draws from.
        private static bool HasLayersAboveCourseIn(DrawSnapshot snapshot)
        {
            return snapshot.mapType == MapType.OCAD && snapshot.map != null && snapshot.lowerPurpleMapLayer != null;
        }

        // Create the options that every layer of the map and course is drawn with. Area patterns are drawn element by
        // element, each area drawing its own, when AreaPatternsElementByElement says so; otherwise with pattern brushes
        // for a target whose brushes are vectors, which draw them, hatchings among them, as well as drawing each element
        // does, and in less; and for any other target with pattern bitmaps, unless printing or zoomed in far with
        // anti-aliasing.
        // grTarget: the target drawn onto.
        // minResolution: the smallest distance worth drawing, in map coordinates.
        private RenderOptions CreateRenderOptions(IGraphicsTarget grTarget, float minResolution)
        {
            RenderOptions renderOptions = new RenderOptions();
            renderOptions.minResolution = minResolution;

            if (AreaPatternsElementByElement)
                renderOptions.usePatternBitmaps = false;
            else if (GraphicsUtil.PatternBrushesAreVectors(grTarget))
                renderOptions.usePatternBitmaps = true;
            else if (Printing)
                renderOptions.usePatternBitmaps = false;   // don't use pattern bitmaps when printing, they cause some problems in some printer drivers and we want best quality.
            else if (antialiased && minResolution < 0.007F)  // use pattern bitmaps unless high quality and zoomed in very far
                renderOptions.usePatternBitmaps = false;
            else
                renderOptions.usePatternBitmaps = true;

            // Drawn as separate shapes, each area draws its own patterns, as a reader that cannot fill an area with a
            // pattern is then given a clip of one outline for each (S-7).
            renderOptions.patternsWithEachArea = AreaPatternsElementByElement;

            renderOptions.showSymbolBounds = showBounds;
            renderOptions.renderTemplates = RenderTemplateOption.MapAndTemplates;
            renderOptions.blendOverprintedColors = ocadOverprintEffect;
            return renderOptions;
        }

        // Get the color ID of the color just below the white-out color, which is where we divide between lower purple
        // and upper purple; or null when there is no lower purple layer.
        // snapshot: the fields the drawing draws from.
        private short? ColorIdBelowWhiteOut(DrawSnapshot snapshot)
        {
            if (snapshot.lowerPurpleMapLayer != null && snapshot.courseMap != null)
                return ColorIdBelow(snapshot.courseMap, CourseLayout.WHITEOUT_COLOR_OCADID);
            return null;
        }

        // Draw one layer of the map and course, with the options of the drawing it belongs to.
        // grTarget: the target to draw onto.
        // layer: the layer to draw.
        // snapshot: the fields the drawing draws from, read once for all of its layers.
        // visRect: the area of the map to draw, in map coordinates.
        // renderOptions: the options the drawing draws its layers with; the range of colors drawn is set here.
        // colorIdBelowWhiteOut: the color ID just below the white-out color, or null when there is no lower purple layer.
        // throwOnCancel: called now and then to throw if drawing is to stop, or null.
        private void DrawLayer(IGraphicsTarget grTarget, MapDisplayLayer layer, DrawSnapshot snapshot, RectangleF visRect, RenderOptions renderOptions, short? colorIdBelowWhiteOut, Action throwOnCancel)
        {
            float saveIntensity;

            switch (layer) {
            case MapDisplayLayer.MapBelowCourse:
                switch (snapshot.mapType) {
                case MapType.OCAD:
                    // map can be null briefly while SetMapFile is switching maps.
                    if (snapshot.map != null) {
                        saveIntensity = grTarget.Intensity;
                        grTarget.Intensity = mapIntensity;
                        grTarget.PushAntiAliasing(PrintingWithoutAntiAliasing ? false : antialiased);       // don't anti-alias on printer

                        if (snapshot.lowerPurpleMapLayer != null) {
                            // Only draw the part below lower purple.
                            renderOptions.colorBeginDrawExclusive = null;
                            renderOptions.colorEndDrawInclusive = snapshot.lowerPurpleMapLayer;
                        }

                        DrawOcadMap(snapshot.map, grTarget, visRect, renderOptions, throwOnCancel);
                        grTarget.PopAntiAliasing();
                        grTarget.Intensity = saveIntensity;
                    }
                    break;

                case MapType.Bitmap:
                case MapType.PDF:
                    // bitmap can be null briefly while SetMapFile is switching maps.
                    if (snapshot.bitmap != null) {
                        grTarget.PushAntiAliasing(PrintingWithoutAntiAliasing ? false : antialiased);       // don't anti-alias on printer
                        DrawBitmapMap(snapshot.bitmap, snapshot.dimmedBitmap, grTarget, visRect, renderOptions.minResolution);
                        grTarget.PopAntiAliasing();
                    }
                    break;

                case MapType.None:
                    break;
                }
                break;

            case MapDisplayLayer.CourseLower:
                if (snapshot.lowerPurpleMapLayer != null) {
                    // Only draw the part below the white-out layer, which is all of the lower purple.
                    renderOptions.colorBeginDrawExclusive = null;
                    renderOptions.colorEndDrawInclusive = colorIdBelowWhiteOut;
                }

                DrawCourseMap(snapshot.courseMap, grTarget, visRect, renderOptions);
                break;

            case MapDisplayLayer.MapAboveCourse:
                if (HasLayersAboveCourseIn(snapshot)) {
                    // Draw the part of the map above the lower purple.
                    saveIntensity = grTarget.Intensity;
                    grTarget.Intensity = mapIntensity;

                    grTarget.PushAntiAliasing(PrintingWithoutAntiAliasing ? false : antialiased);       // don't anti-alias on printer

                    renderOptions.colorBeginDrawExclusive = snapshot.lowerPurpleMapLayer;
                    renderOptions.colorEndDrawInclusive = null;

                    DrawOcadMap(snapshot.map, grTarget, visRect, renderOptions, throwOnCancel);
                    grTarget.PopAntiAliasing();
                    grTarget.Intensity = saveIntensity;
                }
                break;

            case MapDisplayLayer.CourseUpper:
                if (HasLayersAboveCourseIn(snapshot)) {
                    // Draw the rest of the course map on top.
                    renderOptions.colorBeginDrawExclusive = colorIdBelowWhiteOut;
                    renderOptions.colorEndDrawInclusive = null;
                    DrawCourseMap(snapshot.courseMap, grTarget, visRect, renderOptions);
                }
                break;
            }
        }

        // Draw the course map (or part of it, per renderOptions).
        // courseMap: the course map to draw (may be null), snapshotted by the caller so the map we lock is the map we draw.
        private void DrawCourseMap(Map courseMap, IGraphicsTarget grTargetCourses, RectangleF visRect, RenderOptions renderOptions)
        {
            if (PrintingWithoutAntiAliasing)
                grTargetCourses.PushAntiAliasing(false);
            else
                grTargetCourses.PushAntiAliasing(true);   // always anti-alias the course unless printing without anti-aliasing

            // Always turn blending on.
            bool saveBlendOverprintedColors = renderOptions.blendOverprintedColors;
            renderOptions.blendOverprintedColors = true;

            if (courseMap != null) {
                using (courseMap.Read())
                    courseMap.Draw(grTargetCourses, visRect, renderOptions, null);
            }

            // Restore old blending setting.
            renderOptions.blendOverprintedColors = saveBlendOverprintedColors;

            grTargetCourses.PopAntiAliasing();
        }

        // Draw a gray outline to show the print area.
        private void DrawPrintAreaOutline(IGraphicsTarget grTargetCourses, RectangleF visRect)
        {
            RectangleF? printArea = this.printArea; // Snapshot in case it changes on another thread while we are drawing.

            grTargetCourses.PushAntiAliasing(false);

            if (printArea.HasValue && !printArea.Value.Contains(visRect)) {
                object printAreaOutline = new object();

                grTargetCourses.CreateSolidBrush(printAreaOutline, CmykColor.FromCmyka(0, 0, 0, 1, 0.12F));
                if (printArea.Value.Top > visRect.Top) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, visRect.Top, visRect.Right, printArea.Value.Top);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Bottom < visRect.Bottom) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, printArea.Value.Bottom, visRect.Right, visRect.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Left > visRect.Left) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, printArea.Value.Top, printArea.Value.Left, printArea.Value.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Right < visRect.Right) {
                    RectangleF draw = RectangleF.FromLTRB(printArea.Value.Right, printArea.Value.Top, visRect.Right, printArea.Value.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
            }

            grTargetCourses.PopAntiAliasing();
        }

        public event MapDisplayChanged Changed;

        // Raise the changed event.
        private void RaiseChanged()
        {
            if (Changed != null)
                Changed();
        }
    }
}

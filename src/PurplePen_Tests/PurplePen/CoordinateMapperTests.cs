#if TEST
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurplePen.Graphics2D;
using TestingUtils;

namespace PurplePen.Tests
{
    // Tests of the georeference that CoordinateMapper gives a map, which the SVG export records with it (G-1, G-2).
    [TestClass]
    public class CoordinateMapperTests : TestFixtureBase
    {
        // The coordinate mapper of an OCAD map among the test files.
        // fileName: the map file's name within the test files.
        private static CoordinateMapper MapperOf(string fileName)
        {
            MapDisplay mapDisplay = new MapDisplay();
            mapDisplay.SetMapFile(MapType.OCAD, TestUtil.GetTestFile(fileName));
            return mapDisplay.CoordinateMapper;
        }

        // GRC-Jan2017.ocd is at 1:4000 in OCAD grid 2010, UTM zone 10N on WGS 84, which is EPSG:32610, and turned by
        // 17.3 degrees about its offset (561483, 5239175): a millimetre of map is 4 m of the real world, and every point
        // lies where GetRealWorld, which world files are written with, puts it.
        [TestMethod]
        public void AGeoreferenceIsTheTransformThatGetRealWorldApplies()
        {
            CoordinateMapper mapper = MapperOf(@"routegadget\GRC-Jan2017.ocd");
            MapGeoreference georeference = mapper.GetMapGeoreference();

            double angle = -17.3 * Math.PI / 180;
            Assert.AreEqual(4 * Math.Cos(angle), georeference.A, 1e-6);
            Assert.AreEqual(4 * Math.Sin(angle), georeference.B, 1e-6);
            Assert.AreEqual(-4 * Math.Sin(angle), georeference.C, 1e-6);
            Assert.AreEqual(4 * Math.Cos(angle), georeference.D, 1e-6);
            Assert.AreEqual(561483, georeference.E, 1e-6);
            Assert.AreEqual(5239175, georeference.F, 1e-6);
            foreach (PointF point in new PointF[] { new PointF(-43.78F, 201.04F), new PointF(168.46F, 418.32F), new PointF(1000, -1000) }) {
                double realX, realY;
                Assert.IsTrue(mapper.GetRealWorld(point, out realX, out realY));
                Assert.AreEqual(realX, georeference.A * point.X + georeference.C * point.Y + georeference.E, 1e-6);
                Assert.AreEqual(realY, georeference.B * point.X + georeference.D * point.Y + georeference.F, 1e-6);
            }

            Assert.AreEqual(32610, georeference.EpsgCode);
            Assert.AreEqual("+proj=utm +zone=10 +datum=WGS84 +units=m +no_defs", georeference.Proj4String);
            Assert.AreEqual(2010, georeference.OcadGridAndZone);
        }

        // Composed with the transform from a bitmap's pixels to the map, the georeference gives the world file that
        // image export writes for the bitmap, as TestExportWorldFile records it.
        [TestMethod]
        public void AGeoreferenceGivesTheWorldFileOfABitmapOfTheMap()
        {
            MapGeoreference georeference = MapperOf(@"routegadget\GRC-Jan2017.ocd").GetMapGeoreference();
            RectangleF rect = RectangleF.FromLTRB(-43.78F, 201.04F, 168.46F, 418.32F);
            float bitmapWidth = (rect.Width / 25.4F) * 200F, bitmapHeight = (rect.Height / 25.4F) * 200F;
            Matrix pixelsToMap = Geometry.CreateInvertedRectangleTransform(new RectangleF(0, 0, bitmapWidth, bitmapHeight), rect);
            PointF[] pixels = Geometry.TransformPoints(new PointF[] { new PointF(0, 0), new PointF(1, 0), new PointF(0, 1) }, pixelsToMap);
            double[] realX = new double[pixels.Length], realY = new double[pixels.Length];
            for (int i = 0; i < pixels.Length; ++i) {
                realX[i] = georeference.A * pixels[i].X + georeference.C * pixels[i].Y + georeference.E;
                realY[i] = georeference.B * pixels[i].X + georeference.D * pixels[i].Y + georeference.F;
            }

            string[] worldFile = File.ReadAllLines(TestUtil.GetTestFile(@"routegadget\exportgif2.gfw.expected"));
            double[] actual = { realX[1] - realX[0], realY[1] - realY[0], realX[2] - realX[0], realY[2] - realY[0], realX[0], realY[0] };
            Assert.AreEqual(actual.Length, worldFile.Length);
            for (int i = 0; i < actual.Length; ++i)
                Assert.AreEqual(double.Parse(worldFile[i], CultureInfo.InvariantCulture), actual[i], 1e-5);
        }

        // A map in another grid names that grid's EPSG code too: testproj2.ocd is in OCAD grid 5000, the British
        // National Grid, which is EPSG:27700.
        [TestMethod]
        public void AGeoreferenceNamesTheEpsgCodeOfTheMapsGrid()
        {
            MapGeoreference georeference = MapperOf(@"gpx\testproj2.ocd").GetMapGeoreference();
            Assert.AreEqual(27700, georeference.EpsgCode);
            Assert.AreEqual(5000, georeference.OcadGridAndZone);
            StringAssert.StartsWith(georeference.Proj4String, "+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 ");
        }

        // A map with real world coordinates but no coordinate system, OCAD's grid 1000, has a georeference that
        // identifies no coordinate system; a map in a grid that Purple Pen does not support names that grid alone; and
        // a map without real world coordinates has no georeference.
        [TestMethod]
        public void AGeoreferenceNamesOnlyTheIdentificationsThatTheMapHas()
        {
            MapGeoreference noCoordinateSystem = MapperOf(@"gpx\NoCoordSystem.ocd").GetMapGeoreference();
            Assert.AreEqual(15, noCoordinateSystem.A, 1e-9);
            Assert.AreEqual(0, noCoordinateSystem.B, 1e-9);
            Assert.AreEqual(230034, noCoordinateSystem.E, 1e-6);
            Assert.AreEqual(345234, noCoordinateSystem.F, 1e-6);
            Assert.AreEqual(0, noCoordinateSystem.EpsgCode);
            Assert.IsTrue(string.IsNullOrEmpty(noCoordinateSystem.Proj4String));
            Assert.AreEqual(0, noCoordinateSystem.OcadGridAndZone);

            MapGeoreference unsupported = MapperOf(@"gpx\LickCr2014AprUnsupCoordSystem.ocd").GetMapGeoreference();
            Assert.AreEqual(0, unsupported.EpsgCode);
            Assert.IsTrue(string.IsNullOrEmpty(unsupported.Proj4String));
            Assert.AreEqual(55000, unsupported.OcadGridAndZone);

            Assert.IsNull(MapperOf(@"gpx\LickCr2014AprNoRealWorld.ocd").GetMapGeoreference());
        }
    }
}
#endif

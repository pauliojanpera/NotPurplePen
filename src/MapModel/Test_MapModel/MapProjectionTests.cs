#if TEST
using System;
using NUnit.Framework;

namespace PurplePen.MapModel.Tests
{
    // Tests of the table that maps OCAD projection ids to the Proj4 strings and EPSG codes of their coordinate systems.
    [TestFixture]
    public class MapProjectionTests
    {
        // A grid names the EPSG code of its coordinate system: ETRS-TM35FIN is EPSG:3067, and WGS 84 / UTM zone 60S
        // is EPSG:32760.
        [Test]
        public void AGridNamesTheEpsgCodeOfItsCoordinateSystem()
        {
            Assert.AreEqual(3067, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(6005));
            Assert.AreEqual(32760, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(-2060));
        }

        // A grid whose own code EPSG has deprecated names the code EPSG replaced it with, where that one describes the
        // same coordinates: NTF (Paris) / Lambert Nord France is 27561 for the deprecated 27591, and the
        // pseudo-Mercator of web maps is 3857 for the deprecated 3785.
        [Test]
        public void AGridWhoseCodeIsDeprecatedNamesItsReplacement()
        {
            Assert.AreEqual(27561, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(7001));
            Assert.AreEqual(3857, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(64000));
        }

        // A grid that has no EPSG code, a grid whose code describes coordinates other than its Proj4 string does, and
        // an id that no grid has, name none.
        [Test]
        public void AGridWithoutAConfirmedCodeOrAnUnknownIdNamesNone()
        {
            Assert.AreEqual(0, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(16001));
            Assert.AreEqual(0, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(50050));
            Assert.AreEqual(0, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(17008));
            Assert.AreEqual(0, OcadToProj4Projection.EpsgCodeFromOcadProjectionId(123456));
        }

        // A grid that names its EPSG code still gives the Proj4 string it gave before.
        [Test]
        public void AGridThatNamesItsEpsgCodeKeepsItsProj4String()
        {
            Assert.AreEqual("+proj=utm +zone=35 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs",
                            OcadToProj4Projection.Proj4StringFromOcadProjectionId(6005));
            Assert.IsNull(OcadToProj4Projection.Proj4StringFromOcadProjectionId(123456));
        }
    }
}
#endif

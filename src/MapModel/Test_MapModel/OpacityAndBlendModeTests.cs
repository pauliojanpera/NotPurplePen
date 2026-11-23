#if TEST
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using PurplePen.Graphics2D;
using static PurplePen.MapModel.Tests.MapFileTestUtil;

namespace PurplePen.MapModel.Tests
{
    // Tests of the opacity and the blend mode of palette colors: the map side, and the import and export of OCAD and
    // Mapper files. No test draws.
    [TestFixture]
    public class OpacityAndBlendModeTests
    {
        const string Ocad9Map = "rendering\\LordHill.ocd";
        const string Ocad11Map = "rendering\\LordHill OCAD11.ocd";
        const string Ocad12Map = "rendering\\wholestructure.ocd";
        const string Ocad2018Map = "ocadimport\\ssl_issprom2019.ocd";
        const string MapperMap = "openmapper\\LordHill.omap";      // LordHill.ocd, as Mapper opened and saved it.

        // The base name of the files that the tests write of the map that MakeMap makes.
        const string MadeMap = "ocadimport\\opacity and blend mode";

        // A palette color: its name, its opacity and its blend mode.
        static string DescribeCompositing(SymColor color)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1} {2}", color.Name, color.Opacity, color.BlendMode);
        }

        // Make a black palette color with an opacity and a blend mode.
        static SymColor MakeColor(string name, short ocadId, bool overprint, float opacity, ColorBlendMode blendMode)
        {
            SymColor color = new SymColor(name, ocadId, 0, 0, 0, 1, overprint);
            color.Opacity = opacity;
            color.BlendMode = blendMode;
            return color;
        }

        // Make a map of four palette colors, the top one first: opaque, transparent, overprinted and darkened, and
        // transparent and multiplied.
        static Map MakeMap()
        {
            Map map = new Map(new GDIPlus_TextMetrics(), null);
            using (map.Write()) {
                map.AddColor(MakeColor("Multiplied", 4, false, 0.25F, ColorBlendMode.Multiply));
                map.AddColor(MakeColor("Darkened", 3, true, 1, ColorBlendMode.Darken));
                map.AddColor(MakeColor("Transparent", 2, false, 0.6F, ColorBlendMode.Normal));
                map.AddColor(MakeColor("Opaque", 1, false, 1, ColorBlendMode.Normal));
            }
            return map;
        }

        // The color strings of an OCAD 9 or later file, in the order of the file: the name of each color, and each field
        // as its code and its value, such as "Opaque n1 c0 m0 y0 k100 o0 t100".
        static List<string> ReadColorStrings(string path)
        {
            FastBinaryReader reader = new FastBinaryReader(path, Util.GetDefaultOcadEncoding());
            OcadFileHeader header = new OcadFileHeader();
            header.Read(reader);
            reader.UseOcadEncoding(header.Version);
            OcadStIndexBlocks indexBlocks = new OcadStIndexBlocks();
            indexBlocks.Read(reader, header.FirstStIndexBlk);

            List<string> colorStrings = new List<string>();
            foreach (OcadStIndex index in indexBlocks.indexes.Where(index => index.StType == (int) OcadStringParam.Color)) {
                OcadParamString paramString = new OcadParamString();
                paramString.Read(reader, index);
                colorStrings.Add(string.Join(" ", paramString.codes.Select((code, i) => code + paramString.values[i]).Prepend(paramString.firstField)));
            }
            return colorStrings;
        }

        static void Undo(Map map)
        {
            using (map.Write())
                map.UndoMgr.Undo();
        }

        static void Redo(Map map)
        {
            using (map.Write())
                map.UndoMgr.Redo();
        }

        [Test]
        public void PaletteEntryIsOpaqueAndNormalByDefault()
        {
            Assert.AreEqual("Brown: 1 Normal", DescribeCompositing(new SymColor("Brown", 3, 0F, 0.56F, 1F, 0.18F, true)));

            Map map = new Map(new GDIPlus_TextMetrics(), null);
            using (map.Write()) {
                Assert.AreEqual("Purple: 1 Normal", DescribeCompositing(map.AddColor("Purple", 11, 0.15F, 0.9F, 0, 0.25F, false)));
                Assert.AreEqual(1F, map.LayoutColor.Opacity);
                Assert.AreEqual(ColorBlendMode.Normal, map.ImageColor.BlendMode);
            }
        }

        [Test]
        public void OpacityAndBlendModeAreApartFromCmykAndOverprint()
        {
            SymColor color = MakeColor("Purple", 11, true, 0.8F, ColorBlendMode.Multiply);
            color.SetCMYK(0.15F, 0.9F, 0, 0.25F);

            // The CMYK value keeps its alpha of 1, and the inks are the process inks at the CMYK values.
            Assert.AreEqual(CmykColor.FromCmyk(0.15F, 0.9F, 0, 0.25F), color.CmykColor);
            Assert.AreEqual(1F, color.CmykColor.Alpha);
            CollectionAssert.AreEqual(new float[] { 0.15F, 0.9F, 0, 0.25F }, color.InkComponents.Select(component => component.Factor));
            Assert.IsTrue(color.OverPrint);

            color.OverPrint = false;
            Assert.AreEqual("Purple: 0.8 Multiply", DescribeCompositing(color));

            Assert.Throws<ArgumentOutOfRangeException>(() => color.Opacity = 1.01F);
            Assert.Throws<ArgumentOutOfRangeException>(() => color.Opacity = -0.01F);
            Assert.Throws<ArgumentOutOfRangeException>(() => color.Opacity = float.NaN);
            Assert.AreEqual(0.8F, color.Opacity);

            Assert.AreEqual(1F, SymColor.BoundOpacity(float.NaN));
            Assert.AreEqual(1F, SymColor.BoundOpacity(1.5F));
            Assert.AreEqual(0F, SymColor.BoundOpacity(-0.5F));
            Assert.AreEqual(0.25F, SymColor.BoundOpacity(0.25F));
        }

        [Test]
        public void CopiesKeepOpacityAndBlendMode()
        {
            SymColor color = MakeColor("Purple", 11, true, 0.8F, ColorBlendMode.Darken);
            Assert.AreEqual("Purple: 0.8 Darken", DescribeCompositing(new SymColor(color.Layer).CopyFrom(color)));

            Map map = new Map(new GDIPlus_TextMetrics(), null);
            map.UndoMgr = new UndoMgr(10);
            using (map.UndoableWrite("Add color"))
                map.AddColor(color);
            Assert.AreEqual("Purple: 0.8 Darken", DescribeCompositing(ColorsTopFirst(ColorsOnly(map)).Single()));

            // Undo and redo the addition, the removal and a change of the color.
            Undo(map);
            Assert.AreEqual(0, ColorsTopFirst(map).Count);
            Redo(map);
            Assert.AreEqual("Purple: 0.8 Darken", DescribeCompositing(ColorsTopFirst(map).Single()));

            using (map.UndoableWrite("Remove color"))
                map.RemoveColorAtIndex(0);
            Undo(map);
            Assert.AreEqual("Purple: 0.8 Darken", DescribeCompositing(ColorsTopFirst(map).Single()));

            using (map.UndoableWrite("Change color"))
                map.ChangeColorAtIndex(0, "Violet", 11, 0.3F, 1, 0, 0, false);
            Assert.AreEqual("Violet: 0.8 Darken", DescribeCompositing(ColorsTopFirst(map).Single()));
            Undo(map);
            Assert.AreEqual("Purple: 0.8 Darken", DescribeCompositing(ColorsTopFirst(map).Single()));
        }

        [TestCase(Ocad9Map)]
        [TestCase(Ocad11Map)]
        public void OcadImportReadsTransparencyAsOpacity(string fileName)
        {
            Map map = LoadMap(fileName);
            List<SymColor> colors = ColorsTopFirst(map);

            // Purple is t80. Its CMYK value, with the alpha of 1, and its overprint flag are as before.
            SymColor purple = ColorNamed(map, "Purple");
            Assert.AreEqual(0.8F, purple.Opacity);
            Assert.AreEqual(CmykColor.FromCmyk(0.3F, 1, 0, 0), purple.CmykColor);
            Assert.IsTrue(purple.OverPrint);

            // Each other color is opaque: t100, or no transparency, as Green above white has. No color has a blend mode.
            CollectionAssert.AreEqual(new string[] { "Purple" }, colors.Where(color => color.Opacity != 1).Select(color => color.Name));
            Assert.AreEqual(1F, ColorNamed(map, "Green above white").Opacity);
            Assert.IsTrue(colors.All(color => color.BlendMode == ColorBlendMode.Normal));
        }

        [TestCase(Ocad12Map, new string[] { "Upper Purple for Course Overprint", "Lower Purple for Course Overprint", "Blue 100% - North Line", "Green 100%  - Point Feature" })]
        [TestCase(Ocad2018Map, new string[] { "Ylempi violetti ratapainatukselle", "Alempi violetti ratapainatukselle", "Sininen - Pohjoisviiva", "Vihreä - Pistekohde" })]
        public void OcadImportReadsBlendModes(string fileName, string[] darkenedColorNames)
        {
            List<SymColor> colors = ColorsTopFirst(LoadMap(fileName));
            List<SymColor> darkened = colors.Where(color => color.BlendMode == ColorBlendMode.Darken).ToList();

            // The other colors are bNormal, or have no blend mode, as some colors of the OCAD 2018 map have. They are
            // opaque.
            CollectionAssert.AreEquivalent(darkenedColorNames, darkened.Select(color => color.Name));
            Assert.IsTrue(colors.Except(darkened).All(color => color.BlendMode == ColorBlendMode.Normal));
            Assert.IsTrue(colors.All(color => color.Opacity == 1));

            // The blend mode is apart from the overprint flag.
            Assert.IsTrue(darkened.Any(color => color.OverPrint));
            Assert.IsTrue(darkened.Any(color => !color.OverPrint));
        }

        [Test]
        public void UnknownBlendModeIsNormal()
        {
            Assert.AreEqual(ColorBlendMode.Multiply, OcadBlendMode.FromName("Multiply"));
            Assert.AreEqual(ColorBlendMode.Normal, OcadBlendMode.FromName("Screen"));
            Assert.AreEqual(ColorBlendMode.Normal, OcadBlendMode.FromName(null));

            // A file whose color has a blend mode that OCAD does not know: Screen, written over Darken.
            string path = WriteMap(MakeMap(), MadeMap, Format(MapFileFormatKind.OCAD, 12));
            byte[] bytes = File.ReadAllBytes(path);
            int position = bytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("\tbDarken"));
            Assert.GreaterOrEqual(position, 0);
            Encoding.ASCII.GetBytes("\tbScreen").CopyTo(bytes, position);
            File.WriteAllBytes(path, bytes);

            Assert.AreEqual("Darkened: 1 Normal", DescribeCompositing(ColorNamed(ReadMap(path), "Darkened")));
        }

        [TestCase("io\\scales6.ocd")]
        [TestCase("io\\isomarea.ocd")]
        [TestCase("ocadimport\\Hakola malli OCAD8.ocd")]
        public void Ocad6To8ImportGivesDefaults(string fileName)
        {
            List<SymColor> colors = ColorsTopFirst(LoadMap(fileName));

            Assert.Greater(colors.Count, 0);
            Assert.IsTrue(colors.All(color => color.Opacity == 1 && color.BlendMode == ColorBlendMode.Normal));
        }

        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(2018)]
        public void OcadExportWritesTransparencyAndBlendModeAsOcad(int version)
        {
            // Each color has the transparency after the overprint flag, and from OCAD 12 on the blend mode after it.
            string[] colorStrings = { "Opaque n1 c0 m0 y0 k100 o0 t100", "Transparent n2 c0 m0 y0 k100 o0 t60",
                                      "Darkened n3 c0 m0 y0 k100 o1 t100", "Multiplied n4 c0 m0 y0 k100 o0 t25" };
            string[] blendModes = { " bNormal", " bNormal", " bDarken", " bMultiply" };
            IEnumerable<string> expected = colorStrings.Select((colorString, i) => colorString + (version >= 12 ? blendModes[i] : ""));

            CollectionAssert.AreEqual(expected, ReadColorStrings(WriteMap(MakeMap(), MadeMap, Format(MapFileFormatKind.OCAD, version))));
        }

        [Test]
        public void MapperFileHasOpacityAndNoBlendMode()
        {
            string path = WriteMap(MakeMap(), MadeMap, Format(MapFileFormatKind.OpenMapper, 9));
            XmlDocument document = new XmlDocument();
            document.Load(path);
            XmlNamespaceManager namespaces = new XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("m", document.DocumentElement.NamespaceURI);
            List<XmlElement> colorElements = document.SelectNodes("/m:map/m:colors/m:color", namespaces).Cast<XmlElement>().ToList();

            // The export writes the opacity of each color, and no blend mode.
            CollectionAssert.AreEqual(new string[] { "Opaque 1", "Transparent 0.6", "Darkened 1", "Multiplied 0.25" },
                                      colorElements.Select(element => element.GetAttribute("name") + " " + element.GetAttribute("opacity")));
            foreach (XmlElement element in colorElements)
                Assert.AreEqual("priority name c m y k opacity", string.Join(" ", element.Attributes.Cast<XmlAttribute>().Select(attribute => attribute.Name)));

            // The import reads a missing opacity as 1.
            colorElements[1].RemoveAttribute("opacity");
            document.Save(path);
            Assert.AreEqual("Transparent: 1 Normal", DescribeCompositing(ColorNamed(ReadMap(path), "Transparent")));
        }

        // Each format keeps what it can hold: OCAD 9 and later the opacity, OCAD 12 and later the blend mode, and
        // Mapper the opacity. OCAD 6 to 8 hold neither.
        [TestCase(MapFileFormatKind.OCAD, 6, false, false)]
        [TestCase(MapFileFormatKind.OCAD, 7, false, false)]
        [TestCase(MapFileFormatKind.OCAD, 8, false, false)]
        [TestCase(MapFileFormatKind.OCAD, 9, true, false)]
        [TestCase(MapFileFormatKind.OCAD, 10, true, false)]
        [TestCase(MapFileFormatKind.OCAD, 11, true, false)]
        [TestCase(MapFileFormatKind.OCAD, 12, true, true)]
        [TestCase(MapFileFormatKind.OCAD, 2018, true, true)]
        [TestCase(MapFileFormatKind.OpenMapper, 9, true, false)]
        public void RoundTripKeepsWhatFormatHolds(MapFileFormatKind kind, int version, bool holdsOpacity, bool holdsBlendMode)
        {
            Map map = MakeMap();
            Map newMap = WriteAndRead(map, MadeMap, Format(kind, version));

            foreach (SymColor color in ColorsTopFirst(map)) {
                SymColor newColor = ColorNamed(newMap, color.Name);
                Assert.AreEqual(holdsOpacity ? color.Opacity : 1F, newColor.Opacity, color.Name);
                Assert.AreEqual(holdsBlendMode ? color.BlendMode : ColorBlendMode.Normal, newColor.BlendMode, color.Name);
            }
        }

        [TestCase(Ocad9Map, MapFileFormatKind.OCAD, 9)]
        [TestCase(Ocad11Map, MapFileFormatKind.OCAD, 11)]
        [TestCase(Ocad12Map, MapFileFormatKind.OCAD, 12)]
        [TestCase(Ocad2018Map, MapFileFormatKind.OCAD, 2018)]
        [TestCase(MapperMap, MapFileFormatKind.OpenMapper, 9)]
        public void RoundTripKeepsOpacityAndBlendModeOfTestMap(string fileName, MapFileFormatKind kind, int version)
        {
            Map map = LoadMap(fileName);
            Map newMap = WriteAndRead(map, fileName, Format(kind, version));

            CollectionAssert.AreEqual(ColorsTopFirst(map).Select(DescribeCompositing), ColorsTopFirst(newMap).Select(DescribeCompositing));
        }

        [Test]
        public void MapperReadsTransparencyAsOcadImportDoes()
        {
            // Each color of LordHill.ocd has in LordHill.omap, which Mapper saved of it, the opacity that the OCAD
            // import reads, and no blend mode. Mapper makes "All color separations" its registration black, which is
            // not a color of its file. It adds the spot color separations as spot colors below the other colors, with
            // their names, so the color of a name is the top one.
            List<SymColor> mapperColors = ColorsTopFirst(LoadMap(MapperMap));
            List<SymColor> ocadColors = ColorsTopFirst(LoadMap(Ocad9Map)).Where(color => color.Name != "All color separations").ToList();

            CollectionAssert.AreEqual(ocadColors.Select(DescribeCompositing),
                                      ocadColors.Select(color => DescribeCompositing(mapperColors.First(mapperColor => mapperColor.Name == color.Name))));
            Assert.AreEqual(0.8F, mapperColors.First(color => color.Name == "Purple").Opacity);
        }
    }
}
#endif

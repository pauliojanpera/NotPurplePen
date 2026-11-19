#if TEST
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using PurplePen.Graphics2D;
using TestingUtils;
using static PurplePen.MapModel.Tests.MapFileTestUtil;

namespace PurplePen.MapModel.Tests
{
    // Tests of the inks of palette entries: the model, and the import and export of OCAD and Mapper files.
    // The Hakola test maps are one map in three formats, with eight spot inks.
    [TestFixture]
    public class InkTests
    {
        const string MapperMap = "openmapper\\Hakola malli.omap";
        const string Ocad12Map = "ocadimport\\Hakola malli OCAD12.ocd";
        const string Ocad8Map = "ocadimport\\Hakola malli OCAD8.ocd";

        static readonly string[] SpotInkNames = {
            "Process Musta", "PMS299_Sininen", "PMS471_Ruskea", "PMS361_Vihrea", "PMS136_Keltainen", "Violetti", "PMS428_Harmaa", "Oranssi"
        };

        static List<Ink> SpotInks(Map map)
        {
            using (map.Read())
                return map.AllInks.Where(ink => !ink.IsProcessInk).ToList();
        }

        static string Number(float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        static string DescribeCmyk(CmykColor cmyk)
        {
            return string.Join(",", new float[] { cmyk.Cyan, cmyk.Magenta, cmyk.Yellow, cmyk.Black }.Select(Number));
        }

        static string DescribeInk(Ink ink)
        {
            string screen = ink.Screen.HasValue ? Number(ink.Screen.Value.Frequency) + "@" + Number(ink.Screen.Value.Angle) : "none";
            return string.Format("ink {0} {1} screen {2}{3}", ink.Name, DescribeCmyk(ink.Appearance), screen, ink.IsProcessInk ? " process" : "");
        }

        static string DescribeComponents(SymColor color)
        {
            return string.Join(", ", color.InkComponents.Select(component => component.Ink.Name + " " + Number(component.Factor)));
        }

        // A color: its name, its CMYK value, its overprint flag if asked, whether it is a spot ink as such, and its ink
        // components.
        static string DescribeColor(SymColor color, bool withOverprint)
        {
            string definition = (color.SpotInk != null) ? "ink " + color.SpotInk.Name : (color.HasSpotDefinition ? "spot" : "process");
            return string.Format("color {0} {1}{2} {3}: {4}", color.Name, DescribeCmyk(color.CmykColor),
                                 withOverprint ? (color.OverPrint ? " overprint" : " knockout") : "", definition, DescribeComponents(color));
        }

        // The inks of a map and every color, the top one first, as lines of text.
        static List<string> DescribeInks(Map map)
        {
            using (map.Read()) {
                return map.AllInks.Select(DescribeInk)
                    .Concat(map.AllColors.Reverse().Select(color => DescribeColor(color, true)))
                    .ToList();
            }
        }

        static void AssertComponents(SymColor color, params object[] inkNamesAndFactors)
        {
            List<string> expected = new List<string>();
            for (int i = 0; i < inkNamesAndFactors.Length; i += 2)
                expected.Add(inkNamesAndFactors[i] + " " + Number(Convert.ToSingle(inkNamesAndFactors[i + 1], CultureInfo.InvariantCulture)));
            Assert.AreEqual(string.Join(", ", expected), DescribeComponents(color), color.Name);
        }

        static void AssertCmyk(SymColor color, float cyan, float magenta, float yellow, float black)
        {
            Assert.AreEqual(cyan, color.CmykColor.Cyan, 1E-6, color.Name);
            Assert.AreEqual(magenta, color.CmykColor.Magenta, 1E-6, color.Name);
            Assert.AreEqual(yellow, color.CmykColor.Yellow, 1E-6, color.Name);
            Assert.AreEqual(black, color.CmykColor.Black, 1E-6, color.Name);
        }

        // Two colors whose ink components share an ink, with at least one color between them, have the same ink
        // object, which is an ink of the map.
        static void AssertSameInkInColorsApart(Map map, string colorName1, string colorName2, string inkName)
        {
            List<SymColor> colors = ColorsTopFirst(map);
            SymColor color1 = colors.First(color => color.Name == colorName1);
            SymColor color2 = colors.Last(color => color.Name == colorName2);
            Assert.Greater(Math.Abs(colors.IndexOf(color1) - colors.IndexOf(color2)), 1, "The colors are not apart.");

            Ink ink1 = color1.InkComponents.Single(component => component.Ink.Name == inkName).Ink;
            Ink ink2 = color2.InkComponents.Single(component => component.Ink.Name == inkName).Ink;
            Assert.AreSame(ink1, ink2);
            Assert.AreSame(SpotInks(map).Single(ink => ink.Name == inkName), ink1);
        }

        [Test]
        public void ProcessInksAlwaysExist()
        {
            Map map = new Map(new GDIPlus_TextMetrics(), null);
            using (map.Read())
                CollectionAssert.AreEqual(new Ink[] { Ink.Cyan, Ink.Magenta, Ink.Yellow, Ink.Black }, map.AllInks);

            foreach (Map loaded in new Map[] { LoadMap(MapperMap), LoadMap(Ocad12Map), LoadMap(Ocad8Map) }) {
                using (loaded.Read())
                    CollectionAssert.AreEqual(Ink.ProcessInks, loaded.AllInks.Take(4));
            }

            Assert.AreEqual("ink Cyan 1,0,0,0 screen none process", DescribeInk(Ink.Cyan));
            Assert.AreEqual("ink Black 0,0,0,1 screen none process", DescribeInk(Ink.Black));
        }

        [Test]
        public void ColorWithoutSpotDefinitionIsMadeOfProcessInks()
        {
            SymColor color = new SymColor("Brown", 3, 0F, 0.56F, 1F, 0.18F, true);

            Assert.IsFalse(color.HasSpotDefinition);
            Assert.IsNull(color.SpotInk);
            AssertComponents(color, "Cyan", 0, "Magenta", 0.56, "Yellow", 1, "Black", 0.18);
            Assert.AreSame(Ink.Cyan, color.InkComponents[0].Ink);
            Assert.AreSame(Ink.Black, color.InkComponents[3].Ink);
        }

        [Test]
        public void MissingComponentIsNotFactorZero()
        {
            Ink blue = new Ink("Blue", CmykColor.FromCmyk(0.87F, 0.185F, 0, 0), null);
            Ink green = new Ink("Green", CmykColor.FromCmyk(0.76F, 0, 0.91F, 0), new InkScreen(150, 45));
            SymColor white = new SymColor("White for blue", 1, 0, 0, 0, 0, false);
            white.SetSpotComposition(new InkComponent[] { new InkComponent(blue, 0), new InkComponent(blue, 1) });

            Assert.IsTrue(white.HasSpotDefinition);
            AssertComponents(white, "Blue", 0);
            Assert.IsFalse(white.InkComponents.Any(component => component.Ink == green));
            AssertCmyk(white, 0, 0, 0, 0);

            white.SetSpotComposition(new InkComponent[0]);
            Assert.IsFalse(white.HasSpotDefinition);

            Assert.Throws<ArgumentOutOfRangeException>(() => new InkComponent(blue, 1.5F));
            Assert.Throws<ArgumentOutOfRangeException>(() => new InkComponent(blue, float.NaN));
            Assert.Throws<ArgumentException>(() => white.SetSpotComposition(new InkComponent[] { new InkComponent(Ink.Cyan, 1) }));
            Assert.Throws<ArgumentException>(() => white.SetSpotInk(Ink.Black));
            Assert.AreEqual(0F, InkComponent.BoundFactor(float.NaN));
            Assert.AreEqual(1F, InkComponent.BoundFactor(1.01F));
        }

        [Test]
        public void SpotInkIsPaletteEntryAtFullTone()
        {
            Ink purple = new Ink("Purple", CmykColor.FromCmyk(0.3F, 1, 0, 0), new InkScreen(150, 45));
            SymColor color = new SymColor("Purple", 41, 0.3F, 1, 0, 0, true);
            color.SetSpotInk(purple);

            Assert.IsTrue(color.HasSpotDefinition);
            Assert.AreSame(purple, color.SpotInk);
            AssertComponents(color, "Purple", 1);
        }

        [Test]
        public void ColorUsesOnlyInksOfItsMap()
        {
            Map map = new Map(new GDIPlus_TextMetrics(), null);
            Ink gray = new Ink("Gray", CmykColor.FromCmyk(0, 0, 0, 0.3F), null);
            SymColor color = new SymColor("Gray 50%", 31, 0, 0, 0, 0.15F, false);
            color.SetSpotComposition(new InkComponent[] { new InkComponent(gray, 0.5F) });

            using (map.Write()) {
                Assert.Throws<MapUsageException>(() => map.AddColor(new SymColor(color.Layer).CopyFrom(color)));

                map.AddSpotInk(gray);
                SymColor added = map.AddColor(color);
                Assert.Throws<MapUsageException>(() => map.AddSpotInk(gray));
                Assert.Throws<MapUsageException>(() => map.AddSpotInk(Ink.Yellow));
                Assert.Throws<MapUsageException>(() => map.RemoveSpotInk(gray));

                map.RemoveColorAtIndex(map.AllColors.ToList().IndexOf(added));
                map.RemoveSpotInk(gray);
                CollectionAssert.AreEqual(Ink.ProcessInks, map.AllInks);
            }
        }

        [Test]
        public void CloneKeepsInks()
        {
            Map map = LoadMap(MapperMap);
            Map clone;
            using (map.Read())
                clone = map.Clone(new Map.CloneOptions() { CloneColors = true });

            CollectionAssert.AreEqual(DescribeInks(map), DescribeInks(clone));
            CollectionAssert.AreEqual(SpotInks(map), SpotInks(clone));
        }

        [Test]
        public void MapperImportReadsSpotColorsAsInks()
        {
            Map map = LoadMap(MapperMap);
            List<Ink> inks = SpotInks(map);
            List<SymColor> colors = ColorsTopFirst(map);

            CollectionAssert.AreEqual(SpotInkNames, inks.Select(ink => ink.Name));
            Assert.AreEqual("ink PMS299_Sininen 0.87,0.185,0,0 screen none", DescribeInk(inks[1]));
            Assert.AreEqual(74 + 1, colors.Count);  // one extra for registration black.

            // The spot colors as such are the inks, at the bottom of the palette.
            for (int i = 0; i < inks.Count; ++i) {
                SymColor color = colors[colors.Count - inks.Count + i];
                Assert.AreSame(inks[i], color.SpotInk);
                Assert.IsTrue(color.OverPrint, color.Name);
            }

            AssertComponents(ColorNamed(map, "#ffffff"), "Process Musta", 0);
            AssertComponents(ColorNamed(map, "Musta 100%"), "Process Musta", 1);
            AssertComponents(colors.First(color => color.Name == "Ruskea 50% - Aluekohde"),
                             "Process Musta", 0, "PMS471_Ruskea", 0.5, "PMS361_Vihrea", 0, "PMS136_Keltainen", 0);
            AssertComponents(ColorNamed(map, "Vihreä 100% + 50% Musta Viivakohde"), "Process Musta", 0.5, "PMS361_Vihrea", 1);
            AssertCmyk(colors.First(color => color.Name == "Sininen 70% - Aluekohde"), 0.7F, 0, 0, 0);

            // Registration black is all the spot inks at full tone.
            Assert.AreEqual("Registration black", colors[0].Name);
            AssertComponents(colors[0], SpotInkNames.SelectMany(name => new object[] { name, 1 }).ToArray());

            AssertSameInkInColorsApart(map, "Sininen 100% - Aluekohde", "Sininen 100% - Aluekohde", "PMS299_Sininen");
            AssertSameInkInColorsApart(map, "Musta 100%", "Musta Viljelty maa", "Process Musta");
        }

        [Test]
        public void MapperColorOverprintsOnlyWithSpotDefinitionAndNoKnockout()
        {
            Map map = LoadMap(MapperMap);

            // A spot definition without knockout.
            Assert.IsTrue(ColorNamed(map, "Musta 100%").OverPrint);
            Assert.IsTrue(ColorNamed(map, "Sininen 100% - Pohjoisviiva").OverPrint);

            // A spot definition with knockout.
            Assert.IsFalse(ColorNamed(map, "Sininen 100% - Pistekohde").OverPrint);
            Assert.IsFalse(ColorNamed(map, "#000000").OverPrint);

            // No spot definition: Mapper knocks it out.
            SymColor brown = ColorNamed(map, "Ruskea 100%  - Pistekohde");
            Assert.IsFalse(brown.HasSpotDefinition);
            Assert.IsFalse(brown.OverPrint);
            AssertCmyk(brown, 0, 0.56F, 1, 0.18F);
            AssertComponents(brown, "Cyan", 0, "Magenta", 0.56, "Yellow", 1, "Black", 0.18);
            Assert.IsFalse(ColorNamed(map, "SININEN").OverPrint);
        }

        [Test]
        public void Ocad12ImportReadsSeparationsAsInks()
        {
            Map map = LoadMap(Ocad12Map);
            List<Ink> inks = SpotInks(map);
            List<SymColor> colors = ColorsTopFirst(map);

            CollectionAssert.AreEqual(SpotInkNames, inks.Select(ink => ink.Name));
            Assert.AreEqual("ink PMS299_Sininen 0.87,0.185,0,0 screen none", DescribeInk(inks[1]));
            Assert.AreEqual(67, colors.Count);

            // Registration black is in all the separations.
            Assert.AreEqual("Registration black (all printed colors)", colors[0].Name);
            Assert.IsTrue(colors[0].OverPrint);
            AssertComponents(colors[0], SpotInkNames.SelectMany(name => new object[] { name, 1 }).ToArray());

            AssertComponents(ColorNamed(map, "#ffffff"), "Process Musta", 0);
            AssertComponents(ColorNamed(map, "Musta 100%"), "Process Musta", 1);
            Assert.IsTrue(ColorNamed(map, "Musta 100%").OverPrint);
            SymColor brown50 = colors.First(color => color.Name == "Ruskea 50% - Aluekohde");
            AssertComponents(brown50, "Process Musta", 0, "PMS471_Ruskea", 0.5, "PMS361_Vihrea", 0, "PMS136_Keltainen", 0);
            Assert.IsFalse(brown50.OverPrint);
            AssertCmyk(brown50, 0, 0.28F, 0.5F, 0.09F);

            // An overprinted color without separations.
            SymColor brown = ColorNamed(map, "Ruskea 100%  - Pistekohde");
            Assert.IsFalse(brown.HasSpotDefinition);
            Assert.IsTrue(brown.OverPrint);
            AssertComponents(brown, "Cyan", 0, "Magenta", 0.56, "Yellow", 1, "Black", 0.18);

            AssertSameInkInColorsApart(map, "Sininen 100% - Aluekohde", "Sininen 100% - Aluekohde", "PMS299_Sininen");
        }

        [Test]
        public void Ocad8ImportReadsSeparationsAsInks()
        {
            Map map = LoadMap(Ocad8Map);
            List<Ink> inks = SpotInks(map);
            List<SymColor> colors = ColorsTopFirst(map);

            // OCAD 8 keeps 15 characters of the name of a separation.
            CollectionAssert.AreEqual(SpotInkNames.Select(name => name.Substring(0, Math.Min(15, name.Length))), inks.Select(ink => ink.Name));
            Assert.AreEqual("ink PMS299_Sininen 0.87,0.185,0,0 screen none", DescribeInk(inks[1]));
            Assert.AreEqual(67, colors.Count);

            AssertComponents(colors[0], inks.SelectMany(ink => new object[] { ink.Name, 1 }).ToArray());

            // The shares are in the order of the separations. 255 is no share, and 0 is a share of 0.
            SymColor brown50 = colors.First(color => color.Name == "Ruskea 50% - Aluekohde");
            AssertComponents(brown50, "Process Musta", 0, "PMS471_Ruskea", 0.5, "PMS361_Vihrea", 0, "PMS136_Keltaine", 0);
            AssertComponents(ColorNamed(map, "#ffffff"), "Process Musta", 0);
            AssertComponents(colors.First(color => color.Name == "Sininen 70% - Aluekohde"),
                             "PMS299_Sininen", 0.7, "PMS471_Ruskea", 0, "PMS361_Vihrea", 0, "PMS136_Keltaine", 0);
            Assert.IsFalse(ColorNamed(map, "Ruskea 100%  - Pistekohde").HasSpotDefinition);

            AssertSameInkInColorsApart(map, "Sininen 100% - Aluekohde", "Sininen 100% - Aluekohde", "PMS299_Sininen");
        }

        [Test]
        public void OcadImportReadsScreens()
        {
            // An OCAD 7 map whose separations have screens: 150 lines per inch at 45 degrees, yellow at 30 degrees.
            List<Ink> inks = SpotInks(LoadMap("io\\isomarea.ocd"));
            Assert.AreEqual("ink Black 0,0,0,1 screen 150@45", DescribeInk(inks[0]));
            Assert.AreEqual("ink Yellow 0,0.275,0.79,0 screen 150@30", DescribeInk(inks[4]));

            // The Mapper map of the same symbol set.
            List<Ink> mapperInks = SpotInks(LoadMap("openmapper\\overprinting.omap"));
            Assert.AreEqual("ink Yellow 0,0.275,0.79,0 screen 150@30", DescribeInk(mapperInks[4]));
        }

        [TestCase(MapperMap, MapFileFormatKind.OpenMapper, 9)]
        [TestCase(Ocad12Map, MapFileFormatKind.OCAD, 12)]
        [TestCase(Ocad12Map, MapFileFormatKind.OCAD, 2018)]
        [TestCase(Ocad8Map, MapFileFormatKind.OCAD, 8)]
        [TestCase("io\\isomarea.ocd", MapFileFormatKind.OCAD, 7)]
        [TestCase("openmapper\\overprinting.omap", MapFileFormatKind.OpenMapper, 9)]
        public void RoundTripKeepsInks(string fileName, MapFileFormatKind kind, int version)
        {
            Map map = LoadMap(fileName);
            Map newMap = WriteAndRead(map, fileName, Format(kind, version));

            CollectionAssert.AreEqual(DescribeInks(map), DescribeInks(newMap));
        }

        [Test]
        public void MapperToOcadKeepsInks()
        {
            Map map = LoadMap(MapperMap);
            Map ocadMap = WriteAndRead(ColorsOnly(map), MapperMap, Format(MapFileFormatKind.OCAD, 12));

            CollectionAssert.AreEqual(SpotInks(map).Select(DescribeInk), SpotInks(ocadMap).Select(DescribeInk));

            // The spot colors as such below the other colors, which no symbol uses, are separations only. Each other
            // color keeps its ink components.
            List<SymColor> colors = ColorsTopFirst(map);
            List<SymColor> ocadColors = ColorsTopFirst(ocadMap);
            Assert.AreEqual(colors.Count - SpotInkNames.Length, ocadColors.Count);
            CollectionAssert.AreEqual(colors.Take(ocadColors.Count).Select(DescribeComponents), ocadColors.Select(DescribeComponents));
            CollectionAssert.AreEqual(colors.Take(ocadColors.Count).Select(color => color.OverPrint), ocadColors.Select(color => color.OverPrint));
        }

        [Test]
        public void OcadToMapperKeepsInks()
        {
            Map map = LoadMap(Ocad12Map);
            Map mapperMap = WriteAndRead(map, Ocad12Map, Format(MapFileFormatKind.OpenMapper, 9));

            CollectionAssert.AreEqual(SpotInks(map).Select(DescribeInk), SpotInks(mapperMap).Select(DescribeInk));

            // Mapper keeps each separation as a spot color as such, below the other colors. Registration black is
            // Mapper's own, and the other colors keep their ink components.
            List<SymColor> colors = ColorsTopFirst(map);
            List<SymColor> mapperColors = ColorsTopFirst(mapperMap);
            int inkCount = SpotInkNames.Length;
            Assert.AreEqual(colors.Count + inkCount, mapperColors.Count);
            CollectionAssert.AreEqual(SpotInks(mapperMap), mapperColors.Skip(mapperColors.Count - inkCount).Select(color => color.SpotInk));
            Assert.AreEqual(DescribeComponents(colors[0]), DescribeComponents(mapperColors[0]));
            CollectionAssert.AreEqual(colors.Skip(1).Select(DescribeComponents), mapperColors.Skip(1).Take(colors.Count - 1).Select(DescribeComponents));

            // And back: the spot colors as such are separations again.
            Map ocadMap = WriteAndRead(ColorsOnly(mapperMap), Ocad12Map, Format(MapFileFormatKind.OCAD, 2018));
            CollectionAssert.AreEqual(SpotInks(map).Select(DescribeInk), SpotInks(ocadMap).Select(DescribeInk));
            CollectionAssert.AreEqual(colors.Skip(1).Select(DescribeComponents), ColorsTopFirst(ocadMap).Skip(1).Select(DescribeComponents));
        }

        [Test]
        public void MapperExportNumbersColorsByPlace()
        {
            // Mapper finds the color of an id, and the spot color of a component, at that place in the color list.
            Map map = LoadMap(MapperMap);
            string fileName = Path.ChangeExtension(TestUtil.GetTestFile(MapperMap), null) + " places_temp.omap";
            InputOutput.WriteFile(fileName, map, Format(MapFileFormatKind.OpenMapper, 9));

            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            XmlNamespaceManager namespaces = new XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("m", document.DocumentElement.NamespaceURI);
            List<XmlElement> colorElements = document.SelectNodes("/m:map/m:colors/m:color", namespaces).Cast<XmlElement>().ToList();

            Assert.AreEqual(74, colorElements.Count);
            for (int place = 0; place < colorElements.Count; ++place)
                Assert.AreEqual(place.ToString(CultureInfo.InvariantCulture), colorElements[place].GetAttribute("priority"));

            foreach (XmlElement component in document.SelectNodes("//m:component", namespaces)) {
                int spotColor = int.Parse(component.GetAttribute("spotcolor"), CultureInfo.InvariantCulture);
                Assert.IsNotNull(colorElements[spotColor].SelectSingleNode("m:spotcolors/m:namedcolor", namespaces));
            }

            XmlElement brown50 = colorElements.First(element => element.GetAttribute("name") == "Ruskea 50% - Aluekohde");
            Assert.AreEqual("true", ((XmlElement) brown50.SelectSingleNode("m:spotcolors", namespaces)).GetAttribute("knockout"));
            XmlElement brown = colorElements.Single(element => element.GetAttribute("name") == "Ruskea 100%  - Pistekohde");
            Assert.IsNull(brown.SelectSingleNode("m:spotcolors", namespaces));
        }
    }
}
#endif

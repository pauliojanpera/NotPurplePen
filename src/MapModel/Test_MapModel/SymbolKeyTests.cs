#if TEST
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using static PurplePen.MapModel.Tests.MapFileTestUtil;

namespace PurplePen.MapModel.Tests
{
    // The symbol keys of the symbols of a map (Symbol.Key): the keys that the counter of the map gives, the keys that
    // the maker of a symbol gives, the keys of a prefix (Map.PrefixSymbolKeys), their uniqueness, the replacement of a
    // symbol, the undo and the redo, the copies, the holes, and the keys of the maps read from files.
    [TestFixture]
    public class SymbolKeyTests
    {
        [Test]
        public void TextFormOfACounterKeyIsTheValueInBase62()
        {
            CollectionAssert.AreEqual(new string[] { "0", "9", "A", "Z", "a", "z", "10", "zz", "100", "4C92" },
                                      new long[] { 0, 9, 10, 35, 36, 61, 62, 3843, 3844, 1000000 }.Select(value => SymbolKey.FromCounter(value).Text));
        }

        [Test]
        public void KeysAreTheSameWhenTheirTextFormsAreTheSame()
        {
            SymbolKey key = new SymbolKey("Ab1");
            Assert.AreEqual(new SymbolKey("Ab1"), key);
            Assert.IsTrue(new SymbolKey("Ab1") == key);
            Assert.AreEqual(new SymbolKey("Ab1").GetHashCode(), key.GetHashCode());
            Assert.AreNotEqual(new SymbolKey("ab1"), key);
            Assert.IsTrue(new SymbolKey("ab1") != key);
            Assert.AreEqual("Ab1", key.ToString());
            Assert.Throws<ArgumentException>(() => new SymbolKey(""));
        }

        [Test]
        public void MapGivesTheSymbolsWithoutAKeyKeysFromItsCounter()
        {
            Map map = CreateMap();
            LineSymDef symdef;
            LineSymbol first = null, second = null, third = null;
            using (map.Write()) {
                symdef = AddLineSymdef(map);
                first = CreateLine(symdef, 0);
                second = CreateLine(symdef, 1);
                third = CreateLine(symdef, 2);
                Assert.IsNull(first.Key);
                map.AddSymbol(first);
                map.AddSymbol(second);
                map.AddSymbol(third);
            }

            // The symbols of one symbol definition have keys of their own, which are not the number of the definition.
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, new Symbol[] { first, second, third }.Select(symbol => symbol.Key.Text));
            Assert.AreEqual("1", symdef.SymbolId);
        }

        [Test]
        public void CounterDoesNotGiveAValueAgain()
        {
            Map map = CreateMap();
            LineSymDef symdef;
            LineSymbol last, afterRemoval, afterClear;
            using (map.Write()) {
                symdef = AddLineSymdef(map);
                map.AddSymbol(CreateLine(symdef, 0));
                last = CreateLine(symdef, 1);
                map.AddSymbol(last);
                map.RemoveSymbol(last);
                afterRemoval = CreateLine(symdef, 2);
                map.AddSymbol(afterRemoval);
            }
            Assert.AreEqual("3", afterRemoval.Key.Text);

            using (map.Write()) {
                map.Clear();
                symdef = AddLineSymdef(map);
                afterClear = CreateLine(symdef, 0);
                map.AddSymbol(afterClear);
            }
            Assert.AreEqual("4", afterClear.Key.Text);
        }

        [Test]
        public void MakerOfASymbolCanGiveItsKeyBeforeTheMapGetsIt()
        {
            Map map = CreateMap();
            LineSymbol given, counted;
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                given = CreateLine(symdef, 0);
                given.Key = new SymbolKey("2");
                map.AddSymbol(given);
                counted = CreateLine(symdef, 1);
                map.AddSymbol(counted);
                LineSymbol next = CreateLine(symdef, 2);
                map.AddSymbol(next);

                // The counter skips the value whose text form a symbol of the map has as its key.
                Assert.AreEqual("1", counted.Key.Text);
                Assert.AreEqual("3", next.Key.Text);
            }

            Assert.AreEqual("2", given.Key.Text);
            Assert.Throws<MapUsageException>(() => given.Key = new SymbolKey("5"));
        }

        [Test]
        public void KeyIsUniqueInItsMap()
        {
            Map map = CreateMap();
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                LineSymbol first = CreateLine(symdef, 0);
                first.Key = new SymbolKey("a");
                map.AddSymbol(first);

                LineSymbol second = CreateLine(symdef, 1);
                second.Key = new SymbolKey("a");
                Assert.Throws<MapUsageException>(() => map.AddSymbol(second));
                Assert.AreEqual(1, map.AllSymbols.Count);
                Assert.IsNull(second.ContainingMap);

                // A symbol of the counter key that another symbol of the map already has is a usage error too.
                map.AddSymbol(CreateLine(symdef, 2));
                LineSymbol third = CreateLine(symdef, 3);
                third.Key = new SymbolKey("1");
                Assert.Throws<MapUsageException>(() => map.AddSymbol(third));
            }
        }

        [Test]
        public void ReplacingSymbolGetsTheKeyOfTheReplacedSymbol()
        {
            Map map = CreateMap();
            UndoMgr undoMgr = new UndoMgr(10);
            LineSymDef symdef;
            LineSymbol replaced, replacement;
            using (map.Write()) {
                symdef = AddLineSymdef(map);
                map.AddSymbol(CreateLine(symdef, 0));
                replaced = CreateLine(symdef, 1);
                map.AddSymbol(replaced);
                map.UndoMgr = undoMgr;
            }

            replacement = CreateLine(symdef, 5);
            using (map.UndoableWrite("Move line"))
                map.ReplaceSymbol(replaced, replacement);

            using (map.Read()) {
                Assert.AreEqual("2", replacement.Key.Text);
                Assert.AreSame(map, replacement.ContainingMap);
                Assert.IsNull(replaced.ContainingMap);
                Assert.AreEqual(2, map.AllSymbols.Count);
            }

            // The undo restores the replaced symbol with its key, and the redo the replacing symbol with the same key.
            using (map.Write())
                undoMgr.Undo();
            using (map.Read()) {
                Assert.AreSame(map, replaced.ContainingMap);
                Assert.IsNull(replacement.ContainingMap);
                Assert.AreEqual("2", replaced.Key.Text);
            }

            using (map.Write())
                undoMgr.Redo();
            using (map.Read()) {
                Assert.AreSame(map, replacement.ContainingMap);
                Assert.IsNull(replaced.ContainingMap);
                Assert.AreEqual("2", replacement.Key.Text);
            }

            // The map has given no other key.
            LineSymbol added = CreateLine(symdef, 6);
            using (map.UndoableWrite("Add line"))
                map.AddSymbol(added);
            Assert.AreEqual("3", added.Key.Text);
        }

        [Test]
        public void ReplacementIsAUsageErrorWhenTheSymbolsDoNotFit()
        {
            Map map = CreateMap();
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                LineSymbol inMap = CreateLine(symdef, 0);
                map.AddSymbol(inMap);

                LineSymbol keyed = CreateLine(symdef, 1);
                keyed.Key = new SymbolKey("other");
                Assert.Throws<MapUsageException>(() => map.ReplaceSymbol(inMap, keyed));
                Assert.Throws<MapUsageException>(() => map.ReplaceSymbol(CreateLine(symdef, 2), CreateLine(symdef, 3)));
                Assert.AreSame(map, inMap.ContainingMap);
            }
        }

        [Test]
        public void UndoAndRedoOfAnAdditionKeepTheKey()
        {
            Map map = CreateMap();
            UndoMgr undoMgr = new UndoMgr(10);
            LineSymDef symdef;
            using (map.Write()) {
                symdef = AddLineSymdef(map);
                map.UndoMgr = undoMgr;
            }

            LineSymbol line = CreateLine(symdef, 0);
            using (map.UndoableWrite("Add line"))
                map.AddSymbol(line);
            Assert.AreEqual("1", line.Key.Text);

            using (map.Write())
                undoMgr.Undo();
            using (map.Read())
                Assert.AreEqual(0, map.AllSymbols.Count);

            using (map.Write())
                undoMgr.Redo();
            using (map.Read())
                Assert.AreSame(line, map.AllSymbols.Single());
            Assert.AreEqual("1", line.Key.Text);
        }

        [Test]
        public void CopyOfASymbolHasNoKey()
        {
            Map map = CreateMap();
            LineSymbol line;
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                line = CreateLine(symdef, 0);
                map.AddSymbol(line);
            }

            using (map.Read()) {
                Assert.IsNotNull(line.Key);
                Assert.IsNull(line.CloneDetached().Key);
            }
        }

        [Test]
        public void HolesOfAnAreaAreSymbolsWithKeys()
        {
            Map map = CreateMap();
            AreaSymbol area;
            HoleSymbol addedHole;
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                AreaSymDef forest = new AreaSymDef("Forest", "2", green, null);
                map.AddSymdef(forest);
                area = new AreaSymbol(forest, new SymPathWithHoles(CreateSquarePath(0, 0, 10), new SymPath[] { CreateSquarePath(1, 1, 2) }), 0, new PointF());
                map.AddSymbol(area);

                addedHole = new HoleSymbol(area, CreateSquarePath(5, 5, 2));
                map.AddSymbol(addedHole);
            }

            // The area gets its key, then its hole, and the hole that the map gets later the next key.
            Assert.AreEqual("1", area.Key.Text);
            Assert.AreEqual("2", area.GetHole(0).Key.Text);
            Assert.AreEqual("3", addedHole.Key.Text);

            // The removal of the area frees the keys of the area and its holes.
            using (map.Write()) {
                map.RemoveSymbol(area);
                LineSymDef symdef = AddLineSymdef(map);
                LineSymbol line = CreateLine(symdef, 0);
                line.Key = new SymbolKey("3");
                map.AddSymbol(line);
            }
        }

        [Test]
        public void SymbolsOfAPeriodOfAPrefixGetTheKeysOfThePrefix()
        {
            Map map = CreateMap();
            LineSymbol before, first, given, second, after;
            AreaSymbol area;
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                AreaSymDef forest = AddAreaSymdef(map);
                before = CreateLine(symdef, 0);
                map.AddSymbol(before);

                using (map.PrefixSymbolKeys("leg.4.5.0")) {
                    first = CreateLine(symdef, 1);
                    map.AddSymbol(first);
                    given = CreateLine(symdef, 2);
                    given.Key = new SymbolKey("given");
                    map.AddSymbol(given);
                    area = new AreaSymbol(forest, new SymPathWithHoles(CreateSquarePath(0, 0, 10), new SymPath[] { CreateSquarePath(1, 1, 2) }), 0, new PointF());
                    map.AddSymbol(area);
                    second = CreateLine(symdef, 3);
                    map.AddSymbol(second);
                }

                after = CreateLine(symdef, 4);
                map.AddSymbol(after);
            }

            // The index counts the symbols without a key and the holes, in the order in which the map gets them. The
            // counter does not count the symbols of the period, and gives its next key after the period.
            Assert.AreEqual("1", before.Key.Text);
            CollectionAssert.AreEqual(new string[] { "leg.4.5.0/0", "given", "leg.4.5.0/1", "leg.4.5.0/2", "leg.4.5.0/3" },
                                      new Symbol[] { first, given, area, area.GetHole(0), second }.Select(symbol => symbol.Key.Text));
            Assert.AreEqual("2", after.Key.Text);
        }

        [Test]
        public void IndexOfAPrefixSkipsTheKeysThatTheMapHas()
        {
            Map map = CreateMap();
            List<LineSymbol> lines = new List<LineSymbol>();
            using (map.Write()) {
                LineSymDef symdef = AddLineSymdef(map);
                LineSymbol keyed = CreateLine(symdef, 0);
                keyed.Key = new SymbolKey("control.1.1.0/1");
                map.AddSymbol(keyed);

                // A second period of the same prefix, as two course objects of one key give, continues after the keys of
                // the first one.
                for (int period = 0; period < 2; ++period) {
                    using (map.PrefixSymbolKeys("control.1.1.0")) {
                        for (int i = 0; i < 2; ++i) {
                            LineSymbol line = CreateLine(symdef, lines.Count + 1);
                            map.AddSymbol(line);
                            lines.Add(line);
                        }
                    }
                }
            }

            CollectionAssert.AreEqual(new string[] { "control.1.1.0/0", "control.1.1.0/2", "control.1.1.0/3", "control.1.1.0/4" },
                                      lines.Select(line => line.Key.Text));
        }

        [Test]
        public void PeriodsOfAPrefixAreInAWriteAndDoNotNest()
        {
            Map map = CreateMap();
            Assert.Throws<MapUsageException>(() => map.PrefixSymbolKeys("special.1.0"));

            using (map.Write()) {
                Assert.Throws<ArgumentException>(() => map.PrefixSymbolKeys(""));
                Assert.Throws<ArgumentException>(() => map.PrefixSymbolKeys(null));
                using (map.PrefixSymbolKeys("special.1.0"))
                    Assert.Throws<MapUsageException>(() => map.PrefixSymbolKeys("special.2.0"));

                // The period ended at the dispose of its releaser, so a new one can start.
                using (map.PrefixSymbolKeys("special.2.0")) {
                    LineSymbol line = CreateLine(AddLineSymdef(map), 0);
                    map.AddSymbol(line);
                    Assert.AreEqual("special.2.0/0", line.Key.Text);
                }
            }
        }

        [TestCase("LordHill.ocd")]
        [TestCase("LordHill OCAD11.ocd")]
        public void SymbolsOfAnOcadMapGetTheirKeysInTheOrderOfReading(string fileName)
        {
            CheckKeysInTheOrderOfReading(LoadMap(Path.Combine("skia_render", fileName)));
        }

        [TestCase("LordHill.omap")]
        [TestCase("HG_Mar28.omap")]
        public void SymbolsOfAMapperMapGetTheirKeysInTheOrderOfReading(string fileName)
        {
            CheckKeysInTheOrderOfReading(LoadMap(Path.Combine("openmapper", fileName)));
        }

        // Check that the symbols of a map that was read from a file have the keys of the counter, in the order in which
        // the map got them: each symbol, and then the holes of an area.
        // map: the map.
        static void CheckKeysInTheOrderOfReading(Map map)
        {
            using (map.Read()) {
                long value = 0;
                int holes = 0;
                foreach (Symbol symbol in map.AllSymbols) {
                    Assert.AreEqual(SymbolKey.FromCounter(++value), symbol.Key, "the key of the symbol {0}", value);
                    AreaLikeSymbol area = symbol as AreaLikeSymbol;
                    if (area != null) {
                        for (int i = 0; i < area.HoleCount; ++i, ++holes)
                            Assert.AreEqual(SymbolKey.FromCounter(++value), area.GetHole(i).Key, "the key of the hole {0} of the symbol {1}", i, value);
                    }
                }

                TestContext.WriteLine("{0} symbols and {1} holes", map.AllSymbols.Count, holes);
                Assert.Greater(map.AllSymbols.Count, 0);
            }
        }

        // An empty map.
        static Map CreateMap()
        {
            return new Map(new GDIPlus_TextMetrics(), null);
        }

        // Add a color and a solid line symbol definition of it to a map.
        static LineSymDef AddLineSymdef(Map map)
        {
            SymColor black = map.AddColor("Black", (short)(map.AllColors.Count + 1), 0, 0, 0, 1, false);
            LineSymDef symdef = new LineSymDef("Track", (map.AllSymdefs.Count + 1).ToString(), black, 0.25F, LineJoinMode.Miter, LineCapMode.Flat);
            map.AddSymdef(symdef);
            return symdef;
        }

        // Add a color and an area symbol definition of it to a map.
        static AreaSymDef AddAreaSymdef(Map map)
        {
            SymColor green = map.AddColor("Green", (short)(map.AllColors.Count + 1), 0.76F, 0, 0.91F, 0, false);
            AreaSymDef symdef = new AreaSymDef("Forest", (map.AllSymdefs.Count + 1).ToString(), green, null);
            map.AddSymdef(symdef);
            return symdef;
        }

        // A line of a symbol definition, at a height of its own.
        static LineSymbol CreateLine(LineSymDef symdef, float y)
        {
            return new LineSymbol(symdef, new SymPath(new PointF[] { new PointF(0, y), new PointF(10, y) }));
        }

        // The closed path of a square.
        static SymPath CreateSquarePath(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }
    }
}
#endif

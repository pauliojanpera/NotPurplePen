using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurplePen.MapModel;
using TestingUtils;

namespace PurplePen.Tests
{
    // The course object keys that the course formatters give (CourseObj.Key), and the symbol keys of the course map that
    // CourseLayout.RenderToMap gives from them: the text form, the uniqueness in the layouts of the test events, the
    // symbol keys, and the keys over the edits that keep the role, the identifiers and the layer of a course object.
    [TestClass]
    public class CourseObjKeyTests : TestFixtureBase
    {
        [TestMethod]
        public void TextFormHasTheRoleTheIdentifiersAndTheLayer()
        {
            Assert.AreEqual("leg.4.5.0", new CourseObjKey(CourseObjRole.Leg, new int[] { 4, 5 }, CourseLayer.MainCourse).Text);
            Assert.AreEqual("legMapIssuePoint.z.10.2", new CourseObjKey(CourseObjRole.LegMapIssuePoint, new int[] { 61, 62 }, CourseLayer.OtherCourse1).Text);
            Assert.AreEqual("dropTarget.1.2.1.E", new CourseObjKey(CourseObjRole.DropTarget, new int[] { 1, 2, 1 }, CourseLayer.InvisibleObjects).Text);
            Assert.AreEqual(new CourseObjKey(CourseObjRole.Label, new int[] { 3, 0 }, CourseLayer.AllControls),
                            new CourseObjKey(CourseObjRole.Label, new int[] { 3, 0 }, CourseLayer.AllControls));
            Assert.AreNotEqual(new CourseObjKey(CourseObjRole.Label, new int[] { 3, 0 }, CourseLayer.AllControls),
                               new CourseObjKey(CourseObjRole.Control, new int[] { 3, 0 }, CourseLayer.AllControls));
        }

        // Each course object of each layout of a test event has a key, and the keys of a layout are unique.
        [TestMethod]
        [DataRow("marymoor.ppen")]
        [DataRow("SpecialLegs.ppen")]
        [DataRow("variations.ppen")]
        [DataRow("courserenderer/Fake Relay.ppen")]
        [DataRow("courserenderer/butterfly.ppen")]
        [DataRow("courserenderer/GappedLegs.coursescribe")]
        [DataRow("courserenderer/SpecialLegs.coursescribe")]
        [DataRow("courseformat/mapexchange1.ppen")]
        [DataRow("courseformat/marymoor1.coursescribe")]
        [DataRow("courserenderer/MapIssueTest.ppen")]
        [DataRow("relay/harper.ppen")]
        public void KeysAreUniqueInEachLayout(string fileName)
        {
            EventDB eventDB = LoadEvent(fileName);
            SymbolDB symbolDB = LoadSymbols();
            int layouts = 0, courseObjects = 0;

            foreach (KeyValuePair<string, CourseLayout> layout in MakeLayouts(eventDB, symbolDB)) {
                List<CourseObj> withoutKey = layout.Value.Where(courseObj => courseObj.Key == null).ToList();
                Assert.AreEqual(0, withoutKey.Count, string.Format("{0}: course objects without a key, the first {1}", layout.Key, withoutKey.FirstOrDefault()));

                List<string> duplicates = layout.Value.GroupBy(courseObj => courseObj.Key.Text).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
                Assert.AreEqual(0, duplicates.Count, string.Format("{0}: keys of more than one course object: {1}", layout.Key, string.Join(", ", duplicates.Take(10))));

                ++layouts;
                courseObjects += layout.Value.Count;
            }

            Console.WriteLine("{0} layouts, {1} course objects", layouts, courseObjects);
            Assert.IsTrue(layouts > 1);
        }

        // The symbols of the course map have the keys <course object key>/<index>, with the index from 0 among the
        // symbols of the course object, and a course map that is made again has the same keys.
        [TestMethod]
        public void SymbolsOfTheCourseMapHaveTheKeysOfTheirCourseObjects()
        {
            EventDB eventDB = LoadEvent("marymoor.ppen");
            SymbolDB symbolDB = LoadSymbols();
            Id<Course> courseId = eventDB.AllCourseIds.First();
            CourseLayout layout = MakeSelectionLayout(eventDB, symbolDB, new CourseDesignator(courseId));
            SetLayerColors(layout);
            HashSet<string> courseObjectKeys = new HashSet<string>(layout.Select(courseObj => courseObj.Key.Text));

            List<string> symbolKeys = GetSymbolKeys(layout.RenderToMap(new CourseLayout.MapRenderOptions()));

            Dictionary<string, List<string>> indexesByCourseObject = new Dictionary<string, List<string>>();
            foreach (string symbolKey in symbolKeys) {
                int slash = symbolKey.LastIndexOf('/');
                Assert.IsTrue(slash > 0, "a symbol key without a course object: " + symbolKey);
                string courseObjectKey = symbolKey.Substring(0, slash);
                Assert.IsTrue(courseObjectKeys.Contains(courseObjectKey), "a symbol key of no course object of the layout: " + symbolKey);
                if (!indexesByCourseObject.ContainsKey(courseObjectKey))
                    indexesByCourseObject.Add(courseObjectKey, new List<string>());
                indexesByCourseObject[courseObjectKey].Add(symbolKey.Substring(slash + 1));
            }
            foreach (KeyValuePair<string, List<string>> indexes in indexesByCourseObject)
                CollectionAssert.AreEqual(Enumerable.Range(0, indexes.Value.Count).Select(index => SymbolKey.ToBase62(index)).ToList(), indexes.Value, "the indexes of " + indexes.Key);
            Assert.IsTrue(indexesByCourseObject.Count > 10);

            CourseLayout layoutAgain = MakeSelectionLayout(eventDB, symbolDB, new CourseDesignator(courseId));
            SetLayerColors(layoutAgain);
            CollectionAssert.AreEqual(symbolKeys, GetSymbolKeys(layoutAgain.RenderToMap(new CourseLayout.MapRenderOptions())));
        }

        // The keys stay the same over the edits that do not change the role, the identifiers or the layer of a course
        // object: a move of a control, a bend of a leg, a move of a control number and the flagging of a whole leg. The
        // class of the course object is not a part of the key, so the flagged leg has the key of the leg, and a control
        // that becomes a map exchange keeps its key.
        [TestMethod]
        public void KeysStayOverTheEditsThatKeepTheirParts()
        {
            UndoMgr undoMgr = new UndoMgr(20);
            EventDB eventDB = new EventDB(undoMgr);
            eventDB.Load(GetTestFile("changeevent/sampleevent1.coursescribe"));
            eventDB.Validate();
            SymbolDB symbolDB = LoadSymbols();

            Id<Course> courseId = eventDB.AllCourseIds.First(id => eventDB.GetCourse(id).kind == CourseKind.Normal && !QueryEvent.HasVariations(eventDB, id) &&
                                                                   QueryEvent.EnumCourseControlIds(eventDB, new CourseDesignator(id)).Count() >= 4);
            CourseDesignator designator = new CourseDesignator(courseId);
            List<Id<CourseControl>> courseControls = QueryEvent.EnumCourseControlIds(eventDB, designator).ToList();
            Id<CourseControl> courseControl1 = courseControls[1], courseControl2 = courseControls[2];
            Id<ControlPoint> control1 = eventDB.GetCourseControl(courseControl1).control, control2 = eventDB.GetCourseControl(courseControl2).control;
            CourseObjKey controlKey = new CourseObjKey(CourseObjRole.Control, new int[] { control1.id, courseControl1.id }, CourseLayer.MainCourse);
            CourseObjKey labelKey = new CourseObjKey(CourseObjRole.Label, new int[] { control1.id, courseControl1.id }, CourseLayer.MainCourse);
            CourseObjKey legKey = new CourseObjKey(CourseObjRole.Leg, new int[] { courseControl1.id, courseControl2.id }, CourseLayer.MainCourse);

            Dictionary<CourseObjKey, CourseObj> before = GetKeyedObjects(MakeSelectionLayout(eventDB, symbolDB, designator));
            Assert.IsTrue(before.ContainsKey(controlKey) && before.ContainsKey(labelKey) && before.ContainsKey(legKey),
                          string.Format("the keys {0}, {1} and {2} of the control, its number and its leg among {3}", controlKey, labelKey, legKey,
                                        string.Join(", ", before.Keys.Select(key => key.Text))));
            Assert.IsInstanceOfType(before[legKey], typeof(LegCourseObj));

            PointF location = eventDB.GetControl(control1).location;
            PointF location2 = eventDB.GetControl(control2).location;
            Edit(undoMgr, "Move control", () => ChangeEvent.ChangeControlLocation(eventDB, control1, new PointF(location.X + 5, location.Y + 3)));
            Dictionary<CourseObjKey, CourseObj> moved = AssertSameKeys(before, MakeSelectionLayout(eventDB, symbolDB, designator), "a move of a control");
            Assert.AreNotEqual(((PointCourseObj)before[controlKey]).location, ((PointCourseObj)moved[controlKey]).location);

            PointF bend = new PointF((location.X + location2.X) / 2 + 4, (location.Y + location2.Y) / 2 - 4);
            Edit(undoMgr, "Add bend", () => ChangeEvent.AddLegBend(eventDB, control1, control2, bend));
            AssertSameKeys(before, MakeSelectionLayout(eventDB, symbolDB, designator), "a bend of a leg");

            Edit(undoMgr, "Move number", () => ChangeEvent.ChangeNumberLocation(eventDB, courseControl1, true, new PointF(location.X - 6, location.Y + 6)));
            AssertSameKeys(before, MakeSelectionLayout(eventDB, symbolDB, designator), "a move of a control number");

            Edit(undoMgr, "Flag leg", () => ChangeEvent.ChangeFlagging(eventDB, control1, control2, FlaggingKind.All));
            Dictionary<CourseObjKey, CourseObj> flagged = AssertSameKeys(before, MakeSelectionLayout(eventDB, symbolDB, designator), "the flagging of a whole leg");
            Assert.IsInstanceOfType(flagged[legKey], typeof(FlaggedLegCourseObj));

            Edit(undoMgr, "Map exchange", () => ChangeEvent.ChangeControlExchange(eventDB, courseControl1, MapExchangeType.Exchange));
            Dictionary<CourseObjKey, CourseObj> exchanged = GetKeyedObjects(MakeSelectionLayout(eventDB, symbolDB, designator));
            Assert.IsTrue(exchanged.ContainsKey(controlKey), "the key of the control that is a map exchange");
            Console.WriteLine("The control is {0}, and after it became a map exchange {1}.", before[controlKey].GetType().Name, exchanged[controlKey].GetType().Name);
        }

        // Load a test event.
        // fileName: the name of the file in TestFiles, with / between the folders.
        static EventDB LoadEvent(string fileName)
        {
            EventDB eventDB = new EventDB(new UndoMgr(5));
            eventDB.Load(GetTestFile(fileName));
            eventDB.Validate();
            return eventDB;
        }

        // The path of a test file, with / between the folders.
        static string GetTestFile(string fileName)
        {
            return TestUtil.GetTestFile(fileName.Replace('/', Path.DirectorySeparatorChar));
        }

        static SymbolDB LoadSymbols()
        {
            return new SymbolDB(Util.GetFileInAppDirectory("symbols.xml"));
        }

        // The layouts of an event that the tests check, by name: for each course, each part of a course that has parts,
        // and each variation of a course that has variations, the layout that the selection makes, with the course, the
        // view of all the controls and the other courses; the view of all the controls alone; and the topology view of
        // each course and of each of its variations.
        static IEnumerable<KeyValuePair<string, CourseLayout>> MakeLayouts(EventDB eventDB, SymbolDB symbolDB)
        {
            yield return new KeyValuePair<string, CourseLayout>("all controls", MakeSelectionLayout(eventDB, symbolDB, CourseDesignator.AllControls));

            foreach (Id<Course> courseId in eventDB.AllCourseIds.ToList()) {
                List<CourseDesignator> designators = new List<CourseDesignator>() { new CourseDesignator(courseId) };
                int parts = QueryEvent.CountCourseParts(eventDB, new CourseDesignator(courseId));
                if (parts > 1)
                    designators.AddRange(Enumerable.Range(0, parts).Select(part => new CourseDesignator(courseId, part)));
                if (QueryEvent.HasVariations(eventDB, courseId))
                    designators.AddRange(QueryEvent.GetAllVariations(eventDB, courseId).Select(variation => new CourseDesignator(courseId, variation)));

                foreach (CourseDesignator designator in designators) {
                    yield return new KeyValuePair<string, CourseLayout>("course " + designator, MakeSelectionLayout(eventDB, symbolDB, designator));

                    CourseLayout topology = MakeTopologyLayout(eventDB, symbolDB, designator);
                    if (topology != null)
                        yield return new KeyValuePair<string, CourseLayout>("topology of course " + designator, topology);
                }
            }
        }

        // The layout that the selection makes of a course (SelectionMgr.UpdateCourse): the course in the main layer,
        // the view of all the controls, and each other course in its layer, without the specials and the numbers.
        static CourseLayout MakeSelectionLayout(EventDB eventDB, SymbolDB symbolDB, CourseDesignator designator)
        {
            CourseAppearance appearance = eventDB.GetEvent().courseAppearance;
            CourseLayout layout = new CourseLayout();
            CourseFormatter.FormatCourseToLayout(symbolDB, CourseView.CreateViewingCourseView(eventDB, designator), appearance, layout, CourseLayer.MainCourse);

            if (!designator.IsAllControls) {
                CourseView allControls = CourseView.CreateFilteredAllControlsView(eventDB, new CourseDesignator[] { designator }, ControlPointKind.None,
                    new CourseViewOptions() { showNonDescriptionSpecials = false, showDescriptionSpecials = false });
                CourseFormatter.FormatCourseToLayout(symbolDB, allControls, appearance, layout, CourseLayer.AllControls);

                List<Id<Course>> otherCourses = eventDB.AllCourseIds.Where(courseId => courseId != designator.CourseId).ToList();
                for (int i = 0; i < otherCourses.Count; ++i) {
                    CourseView courseView = CourseView.CreateCourseView(eventDB, new CourseDesignator(otherCourses[i]),
                        new CourseViewOptions() { showNonDescriptionSpecials = false, showDescriptionSpecials = false, showControlNumbers = false });
                    CourseFormatter.FormatCourseToLayout(symbolDB, courseView, appearance, layout, CourseLayer.OtherCourse1 + (i % CourseLayout.EXTRACOURSECOUNT),
                        new CourseFormatterOptions() { showControlNumbers = false });
                }
            }

            return layout;
        }

        // The layout of the topology view of a course (SelectionMgr.UpdateTopology), or null for a course that has none.
        static CourseLayout MakeTopologyLayout(EventDB eventDB, SymbolDB symbolDB, CourseDesignator designator)
        {
            CourseView courseView = CourseView.CreateViewingCourseView(eventDB, designator);
            CourseView topologyView;
            if (courseView.Kind == CourseView.CourseViewKind.Normal) {
                if (QueryEvent.HasVariations(eventDB, courseView.BaseCourseId) || !designator.AllParts)
                    topologyView = CourseView.CreateViewingCourseView(eventDB, new CourseDesignator(courseView.BaseCourseId));
                else
                    topologyView = courseView;
            }
            else if (courseView.Kind == CourseView.CourseViewKind.AllVariations) {
                topologyView = courseView;
            }
            else {
                return null;
            }

            CourseLayout layout = new CourseLayout();
            new TopologyFormatter().FormatCourseToLayout(symbolDB, topologyView, courseView, layout, Id<CourseControl>.None, Id<CourseControl>.None, CourseLayer.AllVariations, CourseLayer.MainCourse);
            return layout;
        }

        // Give each layer of a layout a color of its own, as the map display does, so that the layout can make a map.
        static void SetLayerColors(CourseLayout layout)
        {
            for (int layer = 0; layer < CourseLayout.LAYERCOUNT; ++layer)
                layout.SetLayerColor((CourseLayer)layer, (short)(11 + layer), "Purple " + layer, 0.2F, 1.0F, 0.0F, 0.07F, false);
        }

        // The keys of the symbols of a map and of their holes, in the order in which the map got them.
        static List<string> GetSymbolKeys(Map map)
        {
            List<string> keys = new List<string>();
            using (map.Read()) {
                foreach (PurplePen.MapModel.Symbol symbol in map.AllSymbols) {
                    keys.Add(symbol.Key.Text);
                    AreaLikeSymbol area = symbol as AreaLikeSymbol;
                    if (area != null && !(symbol is HoleSymbol)) {
                        for (int i = 0; i < area.HoleCount; ++i)
                            keys.Add(area.GetHole(i).Key.Text);
                    }
                }
            }

            return keys;
        }

        // The course objects of a layout by their keys.
        static Dictionary<CourseObjKey, CourseObj> GetKeyedObjects(CourseLayout layout)
        {
            return layout.ToDictionary(courseObj => courseObj.Key);
        }

        // Assert that a layout has the keys of the course objects of an earlier layout, and no others, and return its
        // course objects by their keys.
        static Dictionary<CourseObjKey, CourseObj> AssertSameKeys(Dictionary<CourseObjKey, CourseObj> before, CourseLayout layout, string edit)
        {
            Dictionary<CourseObjKey, CourseObj> after = GetKeyedObjects(layout);
            CollectionAssert.AreEquivalent(before.Keys.Select(key => key.Text).ToList(), after.Keys.Select(key => key.Text).ToList(), "the keys after " + edit);
            return after;
        }

        // Make an edit of the event as one command.
        static void Edit(UndoMgr undoMgr, string name, Action edit)
        {
            undoMgr.BeginCommand(1777, name);
            edit();
            undoMgr.EndCommand(1777);
        }
    }
}

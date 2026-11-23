// Counts the hatchings and the area patterns of the test maps, for the writing of ticket 009. Not committed.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using PurplePen.MapModel;

static class Program
{
    static readonly string TestFiles = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Documents/Projects/npp-worktrees/palette-stack-int/src/MapModel/TestFiles");
    static readonly Dictionary<string, int> totals = new Dictionary<string, int>();

    static void Add(string key, int n = 1)
    {
        totals.TryGetValue(key, out int v);
        totals[key] = v + n;
    }

    static object Field(object o, string name)
    {
        return o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(o);
    }

    static Array Parts(Glyph g) => (Array)Field(g, "parts");
    static bool Simple(Glyph g) => (bool)Field(g, "simple");

    // The area symbol definitions that a definition draws with, itself or as components, with nesting depth.
    static IEnumerable<(AreaSymDef def, int depth)> AreaDefs(SymDef def, int depth = 0)
    {
        if (def is AreaSymDef a)
            yield return (a, depth);
        else if (def is AreaComboSymDef c) {
            foreach (SymDef comp in c.Components)
                foreach (var x in AreaDefs(comp, depth + 1))
                    yield return x;
        }
    }

    static void Main()
    {
        List<string> files = Directory.GetFiles(Path.Combine(TestFiles, "skia_render"))
            .Where(f => f.EndsWith(".ocd", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".omap", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal).ToList();
        files.AddRange(Directory.GetFiles(Path.Combine(TestFiles, "ocadimport"), "dashes-*.ocd").OrderBy(f => f, StringComparer.Ordinal));

        // The descriptions of skia_render, by map file.
        Dictionary<string, List<string>> descriptions = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (string txt in Directory.GetFiles(Path.Combine(TestFiles, "skia_render"), "*.txt")) {
            string[] lines = File.ReadAllLines(txt);
            if (lines.Length < 4) continue;
            string mapFile = lines[0].Trim();
            if (!descriptions.ContainsKey(mapFile)) descriptions[mapFile] = new List<string>();
            descriptions[mapFile].Add(Path.GetFileName(txt));
        }

        int mapsWithAny = 0;
        foreach (string file in files) {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(file)));
            try {
                InputOutput.ReadFile(file, map);
            }
            catch (Exception e) {
                Console.WriteLine("{0}: cannot read: {1}", Path.GetFileName(file), e.Message);
                continue;
            }

            using (map.Read()) {
                List<string> lines = new List<string>();
                int hatchedSymbols = 0, patternedSymbols = 0;
                HashSet<(AreaSymDef, int)> usedHatchings = new HashSet<(AreaSymDef, int)>();
                HashSet<(AreaSymDef, int)> usedPatterns = new HashSet<(AreaSymDef, int)>();

                foreach (Symbol symbol in map.AllSymbols) {
                    AreaSymbol area = symbol as AreaSymbol;
                    if (area == null || !symbol.IsVisible || !map.IsSymdefVisible(symbol.Definition))
                        continue;
                    var defs = AreaDefs(symbol.Definition).ToList();
                    bool anyH = false, anyP = false;
                    foreach (var (def, depth) in defs) {
                        var hs = def.GetHatchings().ToList();
                        var ps = def.GetPatterns().ToList();
                        if (hs.Count == 0 && ps.Count == 0) continue;
                        if (depth > 0) Add(depth > 1 ? "symbols with a hatching or pattern in a nested combo component" : "symbols with a hatching or pattern in a combo component");
                        for (int i = 0; i < hs.Count; ++i) {
                            anyH = true;
                            usedHatchings.Add((def, i));
                            var h = hs[i];
                            Add("hatching x symbol");
                            Add("hatching x symbol, rotate mode " + h.hatchRotateMode);
                            bool turns = h.hatchRotateMode != AreaSymDef.RotateMode.Never;
                            if (turns && area.Angle != 0) Add("hatching x symbol, area angle != 0 and not Never");
                            if (!turns && area.Angle != 0) Add("hatching x symbol, area angle != 0 with Never");
                            if (turns && area.RotationCenter != PointF.Empty) Add("hatching x symbol, rotation center != 0 and not Never");
                            if (area.Path.Holes != null) Add("hatching x symbol, area with holes");
                        }
                        for (int i = 0; i < ps.Count; ++i) {
                            anyP = true;
                            usedPatterns.Add((def, i));
                            var p = ps[i];
                            Add("pattern x symbol");
                            Add("pattern x symbol, fill mode " + p.patternFillMode);
                            Add("pattern x symbol, rotate mode " + p.patternRotateMode);
                            if (p.irregular) Add("pattern x symbol, irregular, fill mode " + p.patternFillMode);
                            bool turns = p.patternRotateMode != AreaSymDef.RotateMode.Never;
                            if (turns && area.Angle != 0) Add("pattern x symbol, area angle != 0 and not Never");
                            if (!turns && area.Angle != 0) Add("pattern x symbol, area angle != 0 with Never");
                            if (turns && area.RotationCenter != PointF.Empty) Add("pattern x symbol, rotation center != 0 and not Never");
                            if (area.Path.Holes != null) Add("pattern x symbol, area with holes");
                            if (p.patternFillMode != AreaSymDef.PatternFillMode.Clip && Parts(p.patternGlyph).Length > 1) Add("pattern x symbol, selection mode with a glyph of several parts");
                        }
                    }
                    if (anyH) ++hatchedSymbols;
                    if (anyP) ++patternedSymbols;
                }

                foreach (var (def, i) in usedHatchings.OrderBy(x => x.Item1.SymbolId).ThenBy(x => x.Item2)) {
                    var h = def.GetHatchings().ToList()[i];
                    Add("hatchings used");
                    Add("hatchings used, rotate mode " + h.hatchRotateMode);
                    if (h.hatchOffset != 0) Add("hatchings used, offset != 0");
                    if (h.hatchAngle != 0) Add("hatchings used, angle != 0");
                    bool sharesColor = h.hatchColor == def.FillColor || (def.BorderSymdef != null && def.BorderSymdef.HasColor(h.hatchColor));
                    if (sharesColor) Add("hatchings used, color also full coverage of the definition");
                    lines.Add(string.Format("  hatching {0} [{1}] {2}: color {3}, width {4}, spacing {5}, offset {6}, angle {7}, {8}{9}",
                        def.SymbolId, i, def.Name, h.hatchColor.Name, h.hatchWidth, h.hatchSpacing, h.hatchOffset, h.hatchAngle, h.hatchRotateMode, sharesColor ? ", color also full coverage" : ""));
                }
                foreach (AreaSymDef def in usedHatchings.Select(x => x.Item1).Distinct()) {
                    var hs = def.GetHatchings().ToList();
                    if (hs.GroupBy(h => h.hatchColor).Any(g => g.Count() > 1)) Add("definitions with a cross hatching (two hatchings of one color)");
                }

                foreach (var (def, i) in usedPatterns.OrderBy(x => x.Item1.SymbolId).ThenBy(x => x.Item2)) {
                    var p = def.GetPatterns().ToList()[i];
                    Glyph g = p.patternGlyph;
                    Array parts = Parts(g);
                    HashSet<SymColor> colors = new HashSet<SymColor>();
                    List<string> kinds = new List<string>();
                    foreach (object part in parts) {
                        colors.Add((SymColor)Field(part, "color"));
                        kinds.Add(Field(part, "kind").ToString());
                    }
                    RectangleF bounds = g.BoundingBox;
                    bool exceeds = bounds.Left < -p.patternWidth / 2 || bounds.Right > p.patternWidth / 2 || bounds.Top < -p.patternHeight / 2 || bounds.Bottom > p.patternHeight / 2;
                    Add("patterns used");
                    Add("patterns used, fill mode " + p.patternFillMode);
                    Add("patterns used, rotate mode " + p.patternRotateMode);
                    if (p.irregular) Add("patterns used, irregular");
                    if (p.offsetRows) Add("patterns used, offset rows");
                    if (p.patternOffsetX != 0 || p.patternOffsetY != 0) Add("patterns used, offset != 0");
                    if (p.patternAngle != 0) Add("patterns used, angle != 0");
                    if (Simple(g)) Add("patterns used, simple glyph (one circle at the origin)");
                    if (parts.Length > 1) Add("patterns used, glyph of several parts");
                    if (colors.Count > 1) Add("patterns used, glyph of several colors");
                    if (exceeds) Add("patterns used, glyph bounds beyond the cell");
                    foreach (string k in kinds.Distinct()) Add("patterns used, glyph has a part of kind " + k);
                    bool sharesColor = colors.Any(c => c == def.FillColor || (def.BorderSymdef != null && def.BorderSymdef.HasColor(c)));
                    if (sharesColor) Add("patterns used, a glyph color also full coverage of the definition");
                    if (def.GetHatchings().Any(h => colors.Contains(h.hatchColor))) Add("patterns used, a glyph color also a hatching of the definition");
                    lines.Add(string.Format("  pattern {0} [{1}] {2}: {3} x {4}{5}, angle {6}, offset ({7}, {8}), {9}, {10}{11}, glyph {12} parts [{13}] in {14} colors{15}{16}, bounds {17}{18}",
                        def.SymbolId, i, def.Name, p.patternWidth, p.patternHeight, p.offsetRows ? " offset rows" : "", p.patternAngle, p.patternOffsetX, p.patternOffsetY,
                        p.patternRotateMode, p.patternFillMode, p.irregular ? string.Format(", irregular ({0}, {1}, {2})", p.irregularVarX, p.irregularVarY, p.irregularMinDist) : "",
                        parts.Length, string.Join(",", kinds), colors.Count, Simple(g) ? ", simple" : "", sharesColor ? ", color also full coverage" : "", bounds, exceeds ? " beyond the cell" : ""));
                }

                if (usedHatchings.Count > 0 || usedPatterns.Count > 0) {
                    ++mapsWithAny;
                    Add("maps with a hatching or a pattern");
                    if (usedHatchings.Count > 0) Add("maps with a hatching");
                    if (usedPatterns.Count > 0) Add("maps with a pattern");
                    string desc = descriptions.TryGetValue(Path.GetFileName(file), out var d) ? string.Join(" ", d) : "(no description)";
                    Console.WriteLine("{0}: {1} hatched symbols, {2} patterned symbols; descriptions: {3}", Path.GetFileName(file), hatchedSymbols, patternedSymbols, desc);
                    foreach (string line in lines) Console.WriteLine(line);
                }
                Add("maps");
                Add("area symbols", map.AllSymbols.Count(s => s is AreaSymbol));
            }
        }

        Console.WriteLine();
        foreach (var kv in totals.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            Console.WriteLine("{0}: {1}", kv.Key, kv.Value);
    }
}

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Map_SkiaStd
{
    // EXPERIMENT ONLY, not for merging: records how font families resolve on a CI machine, to find out why the families
    // of the font store came out in another face in some runs. Enabled by the environment variable FONT_PROBE_DIR, where
    // each process writes font-probe-<process>-<pid>.log: a first line with what the process's own font manager knows of
    // the watched families, then one line per resolution through SkiaFontManager.CreateTypeface, with the steps it took
    // and the face it ended with. A resolution that ends in another family than asked for is followed, once per family,
    // by what DirectWrite and GDI know of that family at that moment. Nothing runs in the background, and nothing asks
    // DirectWrite anything until a resolution has gone wrong, so that the probe does not change when or how the process
    // first takes the system's fonts.
    public static class FontProbe
    {
        // The families the first line asks about: those of the store the failing cases draw with, others of the store,
        // and two of the system's.
        private static readonly string[] watched = {
            "Arial Narrow", "Lucida Bright", "Baskerville Old Face", "Book Antiqua", "Forte", "Cooper Black", "Bodoni MT",
            "Arial", "Times New Roman", "Cambria"
        };

        private static readonly object writeLock = new object();
        private static readonly StreamWriter writer;
        private static readonly Dictionary<string, string> hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly HashSet<string> examined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        [ThreadStatic] private static int depth;
        [ThreadStatic] private static StringBuilder trace;

        // Open the log, and write what the process's font manager knows of the watched families.
        static FontProbe()
        {
            string folder = Environment.GetEnvironmentVariable("FONT_PROBE_DIR");
            if (string.IsNullOrEmpty(folder))
                return;

            try {
                Directory.CreateDirectory(folder);
                Process process = Process.GetCurrentProcess();
                string path = Path.Combine(folder, "font-probe-" + process.ProcessName + "-" + Environment.ProcessId + ".log");
                writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false));
                writer.AutoFlush = true;
                Write("start process=" + process.ProcessName + " pid=" + Environment.ProcessId + " started=" + process.StartTime.ToUniversalTime().ToString("HH:mm:ss.fff") +
                      " args=" + Environment.CommandLine);
                StringBuilder known = new StringBuilder();
                foreach (string family in watched)
                    known.Append(" '").Append(family).Append("'=").Append(Count(SKFontManager.Default, family));
                Write("first use: default manager families=" + SKFontManager.Default.FontFamilyCount + " skiaDefault=" + Describe(SKTypeface.Default) + known);
                WriteFamily("Arial");
                WriteFamily("Book Antiqua");
                StringBuilder families = new StringBuilder();
                foreach (string family in SKFontManager.Default.FontFamilies)
                    families.Append(family).Append('=').Append(Count(SKFontManager.Default, family)).Append("; ");
                Write("first use: families " + families);
                foreach (SKFontStyleWeight weight in new SKFontStyleWeight[] { SKFontStyleWeight.Normal, SKFontStyleWeight.Bold }) {
                    foreach (SKFontStyleSlant slant in new SKFontStyleSlant[] { SKFontStyleSlant.Upright, SKFontStyleSlant.Italic }) {
                        using (SKTypeface match = SKTypeface.FromFamilyName("Arial", weight, SKFontStyleWidth.Condensed, slant))
                            Write("first use: 'Arial' " + (int)weight + "/3/" + slant + " gives " + Describe(match));
                    }
                }
            }
            catch (Exception e) {
                writer = null;
                Console.Error.WriteLine("FontProbe could not start: " + e);
            }
        }

        // Write each face of a family as the process's font manager lists it, in its order, with the face it creates.
        private static void WriteFamily(string family)
        {
            using (SKFontStyleSet set = SKFontManager.Default.GetFontStyles(family)) {
                for (int i = 0; i < set.Count; i++) {
                    SKFontStyle style = set[i];
                    using (SKTypeface face = set.CreateTypeface(i))
                        Write("first use: '" + family + "' face " + i + " " + style.Weight + "/" + style.Width + "/" + style.Slant + " = " + Describe(face));
                }
            }
        }

        // Begin a resolution; resolutions nest when one falls back to the default family.
        public static void Begin()
        {
            if (writer == null)
                return;
            if (depth++ == 0)
                trace = new StringBuilder();
        }

        // Add a step to the resolution in progress on this thread, if any.
        public static void Step(string step)
        {
            if (writer == null || depth == 0)
                return;
            trace.Append(step).Append(" | ");
        }

        // End a resolution, and log it when it is the outermost one.
        // state: the font manager's own state, as SkiaFontManager describes it.
        public static void End(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, SKTypeface result, string state)
        {
            if (writer == null)
                return;
            if (--depth > 0)
                return;

            try {
                StringBuilder line = new StringBuilder();
                line.Append("resolve '").Append(familyName).Append("' ").Append((int)weight).Append('/').Append((int)width).Append('/').Append(slant)
                    .Append(" -> ").Append(Describe(result)).Append(" :: ").Append(trace).Append(" state: ").Append(state);

                if (result == null || !string.Equals(result.FamilyName, familyName, StringComparison.OrdinalIgnoreCase)) {
                    line.Append(" MISMATCH");
                    bool first;
                    lock (writeLock) {
                        first = examined.Add(familyName);
                    }
                    if (first)
                        line.Append(" defaultStyles=").Append(Count(SKFontManager.Default, familyName)).Append(' ').Append(DirectWriteAndGdi(familyName));
                }
                Write(line.ToString());
            }
            catch (Exception e) {
                Write("resolve '" + familyName + "' probe failed: " + e.GetType().Name + ": " + e.Message);
            }
            finally {
                trace = null;
            }
        }

        // What DirectWrite and GDI know of a family now: the process's shared factory's system collection as it stands
        // (what Skia's font manager took, unless the system's fonts changed since), an isolated factory's, the shared
        // factory's after checking for updates, and the face GDI selects for the name.
        private static string DirectWriteAndGdi(string familyName)
        {
            if (!OperatingSystem.IsWindows())
                return "";
            StringBuilder text = new StringBuilder();
            text.Append("dwShared=").Append(FindFamily(0, false, familyName));
            text.Append(" dwIsolated=").Append(FindFamily(1, true, familyName));
            text.Append(" dwSharedUpdated=").Append(FindFamily(0, true, familyName));
            text.Append(" gdi=").Append(GdiFace(familyName));
            return text.ToString();
        }

        // Whether a DirectWrite system font collection has a family, as "yes", "no" or the error, with the count of the
        // collection's families.
        // factoryType: 0 for the shared factory, 1 for an isolated one.
        // checkForUpdates: whether the collection is checked for changes to the system's fonts.
        private static string FindFamily(int factoryType, bool checkForUpdates, string familyName)
        {
            try {
                Guid iid = typeof(IDWriteFactory).GUID;
                object factoryObject;
                int hr = DWriteCreateFactory(factoryType, ref iid, out factoryObject);
                if (hr < 0)
                    return "factory 0x" + hr.ToString("X8");
                IDWriteFactory factory = (IDWriteFactory)factoryObject;
                IDWriteFontCollection collection;
                hr = factory.GetSystemFontCollection(out collection, checkForUpdates);
                if (hr < 0)
                    return "collection 0x" + hr.ToString("X8");
                uint index;
                bool exists;
                hr = collection.FindFamilyName(familyName, out index, out exists);
                if (hr < 0)
                    return "find 0x" + hr.ToString("X8");
                return (exists ? "yes" : "no") + "/" + collection.GetFontFamilyCount();
            }
            catch (Exception e) {
                return e.GetType().Name + ": " + e.Message;
            }
        }

        // The face GDI selects for a family name, which is the name itself when GDI has the family.
        private static string GdiFace(string familyName)
        {
            try {
                IntPtr dc = CreateCompatibleDC(IntPtr.Zero);
                IntPtr font = CreateFontW(-20, 0, 0, 0, 400, 0, 0, 0, 1, 0, 0, 0, 0, familyName);
                IntPtr old = SelectObject(dc, font);
                StringBuilder face = new StringBuilder(64);
                GetTextFaceW(dc, face.Capacity, face);
                SelectObject(dc, old);
                DeleteObject(font);
                DeleteDC(dc);
                return "'" + face + "'";
            }
            catch (Exception e) {
                return e.GetType().Name + ": " + e.Message;
            }
        }

        // The number of faces a font manager has of a family, or -1 if it does not know it.
        private static int Count(SKFontManager manager, string family)
        {
            using (SKFontStyleSet set = manager.GetFontStyles(family))
                return set == null ? -1 : set.Count;
        }

        // A typeface as its family, style and PostScript name, with the length and hash of its data.
        private static string Describe(SKTypeface typeface)
        {
            if (typeface == null)
                return "null";
            StringBuilder text = new StringBuilder();
            text.Append('\'').Append(typeface.FamilyName).Append("' ").Append(typeface.FontWeight).Append('/').Append(typeface.FontWidth).Append('/')
                .Append(typeface.FontSlant).Append(" ps=").Append(typeface.PostScriptName);
            try {
                int ttcIndex;
                using (SKStreamAsset stream = typeface.OpenStream(out ttcIndex)) {
                    if (stream == null) {
                        text.Append(" data=none");
                    }
                    else {
                        int length = stream.Length;
                        string key = typeface.PostScriptName + "|" + length;
                        string hash;
                        lock (writeLock) {
                            hashes.TryGetValue(key, out hash);
                        }
                        if (hash == null) {
                            byte[] data = new byte[length];
                            int read = stream.Read(data, length);
                            hash = Convert.ToHexString(SHA256.HashData(data)).Substring(0, 16) + (read == length ? "" : " short " + read);
                            lock (writeLock) {
                                hashes[key] = hash;
                            }
                        }
                        text.Append(" data=").Append(length).Append(" sha256=").Append(hash);
                    }
                }
            }
            catch (Exception e) {
                text.Append(" data failed: ").Append(e.GetType().Name);
            }
            return text.ToString();
        }

        // Write a line to the log, with the time and the thread.
        private static void Write(string text)
        {
            if (writer == null)
                return;
            lock (writeLock) {
                writer.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss.fff") + " t" + Environment.CurrentManagedThreadId + " " + text);
            }
        }

        [DllImport("dwrite.dll")]
        private static extern int DWriteCreateFactory(int factoryType, ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object factory);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr dc);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFontW(int height, int width, int escapement, int orientation, int weight, uint italic, uint underline,
                                                 uint strikeOut, uint charSet, uint outPrecision, uint clipPrecision, uint quality,
                                                 uint pitchAndFamily, string faceName);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr dc, IntPtr gdiObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetTextFaceW(IntPtr dc, int count, StringBuilder faceName);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr gdiObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr dc);

        // The first method of IDWriteFactory, which is all the probe calls.
        [ComImport, Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDWriteFactory
        {
            [PreserveSig]
            int GetSystemFontCollection(out IDWriteFontCollection fontCollection, [MarshalAs(UnmanagedType.Bool)] bool checkForUpdates);
        }

        // The first three methods of IDWriteFontCollection, which are all the probe calls.
        [ComImport, Guid("a84cee02-3eea-4eee-a827-87c1a02a0fcc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDWriteFontCollection
        {
            [PreserveSig]
            uint GetFontFamilyCount();

            [PreserveSig]
            int GetFontFamily(uint index, out IntPtr fontFamily);

            [PreserveSig]
            int FindFamilyName([MarshalAs(UnmanagedType.LPWStr)] string familyName, out uint index, [MarshalAs(UnmanagedType.Bool)] out bool exists);
        }
    }
}

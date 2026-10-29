using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Map_SkiaStd
{
    // EXPERIMENT ONLY, not for merging: records how font families resolve on a CI machine, to find out why some families
    // came out in another face in one run. Enabled by the environment variable FONT_PROBE_DIR, where each process writes
    // font-probe-<process>-<pid>.log: one line per resolution through SkiaFontManager.CreateTypeface, with the steps it
    // took and the face it ended with, and the lines of a watcher thread that asks every half second whether Skia's
    // default font manager, and a font manager created afresh, know each watched family, and whether its files can be
    // read, writing a line whenever the answer changes.
    public static class FontProbe
    {
        // The families the watcher asks about: those of the store the failing cases draw with, others of the store, and
        // two of the system's.
        private static readonly string[] watched = {
            "Arial Narrow", "Lucida Bright", "Baskerville Old Face", "Book Antiqua", "Forte", "Cooper Black", "Bodoni MT",
            "Arial", "Times New Roman"
        };

        // Files of the store the watcher checks can be read, in the fonts folder of Windows.
        private static readonly string[] watchedFiles = {
            "ARIALN.TTF", "arialnarrow.ttf", "ARIALNI.TTF", "arialnarrow_italic.ttf", "LBRITE.TTF", "BASKVILL.TTF", "ANTQUAB.TTF", "FORTE.TTF"
        };

        private static readonly object writeLock = new object();
        private static readonly StreamWriter writer;
        private static readonly Dictionary<string, string> hashes = new Dictionary<string, string>(StringComparer.Ordinal);

        [ThreadStatic] private static int depth;
        [ThreadStatic] private static StringBuilder trace;

        // Open the log and start the watcher, when the probe is enabled.
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
                Write("default manager families=" + SKFontManager.Default.FontFamilyCount + " skiaDefault=" + Describe(SKTypeface.Default));
                Thread thread = new Thread(Watch) { IsBackground = true, Name = "FontProbe" };
                thread.Start();
            }
            catch (Exception e) {
                writer = null;
                Console.Error.WriteLine("FontProbe could not start: " + e);
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
        public static void End(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, SKTypeface result)
        {
            if (writer == null)
                return;
            if (--depth > 0)
                return;

            try {
                StringBuilder line = new StringBuilder();
                line.Append("resolve '").Append(familyName).Append("' ").Append((int)weight).Append('/').Append((int)width).Append('/').Append(slant)
                    .Append(" -> ").Append(Describe(result)).Append(" :: ").Append(trace);

                if (result == null || !string.Equals(result.FamilyName, familyName, StringComparison.OrdinalIgnoreCase)) {
                    line.Append(" MISMATCH defaultStyles=").Append(Count(SKFontManager.Default, familyName));
                    using (SKTypeface again = SKTypeface.FromFamilyName(familyName, weight, width, slant))
                        line.Append(" again=").Append(Describe(again));
                    using (SKFontManager fresh = SKFontManager.CreateDefault()) {
                        line.Append(" freshStyles=").Append(Count(fresh, familyName));
                        using (SKTypeface freshFace = fresh.MatchFamily(familyName, new SKFontStyle(weight, width, slant)))
                            line.Append(" freshFace=").Append(Describe(freshFace));
                    }
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

        // Ask about the watched families and files every half second, and write what changed.
        private static void Watch()
        {
            Dictionary<string, string> last = new Dictionary<string, string>(StringComparer.Ordinal);
            string fontFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            int round = 0;
            while (true) {
                try {
                    using (SKFontManager fresh = SKFontManager.CreateDefault()) {
                        string families = "families default=" + SKFontManager.Default.FontFamilyCount + " fresh=" + fresh.FontFamilyCount;
                        Report(last, "*", families);
                        foreach (string family in watched)
                            Report(last, family, "family '" + family + "' default=" + Count(SKFontManager.Default, family) + " fresh=" + Count(fresh, family));
                    }
                    if (OperatingSystem.IsWindows()) {
                        foreach (string file in watchedFiles)
                            Report(last, file, "file " + file + " " + Readable(Path.Combine(fontFolder, file)));
                    }
                    if (++round % 60 == 0)
                        Write("watching, round " + round);
                }
                catch (Exception e) {
                    Write("watch failed: " + e.GetType().Name + ": " + e.Message);
                }
                Thread.Sleep(500);
            }
        }

        // Write a watched answer when it differs from the last one.
        private static void Report(Dictionary<string, string> last, string key, string answer)
        {
            string before;
            if (last.TryGetValue(key, out before) && before == answer)
                return;
            last[key] = answer;
            Write("watch " + answer);
        }

        // Whether a file can be opened and read, as "ok <length>" or the error.
        private static string Readable(string path)
        {
            try {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                    byte[] head = new byte[4];
                    int read = stream.Read(head, 0, head.Length);
                    return "ok " + stream.Length + (read == 4 ? "" : " short");
                }
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
                        text.Append(" data=").Append(length).Append(" sha256=").Append(hash).Append(" ttc=").Append(ttcIndex);
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
    }
}

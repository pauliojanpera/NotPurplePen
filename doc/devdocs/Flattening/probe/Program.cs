using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// The probe of phase 0 of the flattening work. It measures the boolean path operations of Skia.
    ///   inventory <map file> <results folder>     what the map holds: question 1
    ///   whole-map <map file> <results folder>     the union of each run and the knockout over the whole map: 2, 3 and 8
    ///   builder <map file> <results folder>       the union of each run with SKPath.OpBuilder: 2
    ///   tiles <map file> <results folder>         the same with the marks cut to tiles first: 6 and 8
    ///   digests <map file> <results folder>       the digests of results that are made two times: 5
    ///   shapes <results folder>                   shapes that the probe makes: 4 and 7
    /// The stages of a map take the options --slow-limit and --pairs-limit, in seconds, --tiles with the sides of the
    /// tiles in millimetres, --tile-limit with the seconds for each side, and --keep-uncut, with which a tile keeps
    /// a run that a difference could not cut.
    /// </summary>
    public static class Program
    {
        /// <summary>Runs one command of the probe. It gives 0 when the command ran, and 2 when the arguments are wrong.</summary>
        public static int Main(string[] arguments)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            if (arguments.Length == 2 && arguments[0] == "shapes") {
                ShapeProbe.Measure(arguments[1]);
                return 0;
            }
            if (arguments.Length < 3)
                return ShowUsage();

            MapProbeOptions options = new MapProbeOptions();
            for (int i = 3; i < arguments.Length; ++i) {
                switch (arguments[i]) {
                    case "--slow-limit": options.SlowWayLimitSeconds = double.Parse(arguments[++i], CultureInfo.InvariantCulture); break;
                    case "--pairs-limit": options.PairsLimitSeconds = double.Parse(arguments[++i], CultureInfo.InvariantCulture); break;
                    case "--tile-limit": options.TileSizeLimitSeconds = arguments[++i].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(limit => double.Parse(limit, CultureInfo.InvariantCulture)).ToList(); break;
                    case "--keep-uncut": options.IsUncutKeptOnFailure = true; break;
                    case "--tiles": options.TileSides = arguments[++i].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(side => float.Parse(side, CultureInfo.InvariantCulture)).ToList(); break;
                    default: return ShowUsage();
                }
            }
            return new MapProbe(arguments[1], arguments[2], options).RunStage(arguments[0]) ? 0 : ShowUsage();
        }

        /// <summary>Writes the commands of the probe and gives the exit code of wrong arguments.</summary>
        private static int ShowUsage()
        {
            Console.Error.WriteLine("usage: inventory|whole-map|builder|tiles|digests <map file> <results folder> [--slow-limit <s>] [--pairs-limit <s>] [--tile-limit <s>] [--tiles <mm>,...] [--keep-uncut]");
            Console.Error.WriteLine("       shapes <results folder>");
            return 2;
        }
    }
}

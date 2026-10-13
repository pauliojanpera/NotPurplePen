using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestingUtils
{
    public static class TextFileTestUtil
    {
        // Copies what a failed comparison produced next to the baseline, so that
        // an unattended run leaves something to look at.
        //
        // Parameters:
        //   newFile - the file the test produced.
        //   baseline - full path of the baseline it was compared against.
        private static void SaveComparison(string newFile, string baseline)
        {
            if (!File.Exists(newFile))
                return;

            string directory = Path.GetDirectoryName(baseline);
            string stem = Path.GetFileNameWithoutExtension(baseline);
            string target = Path.Combine(directory, stem + "_actual" + Path.GetExtension(baseline));

            // Saving the evidence is worth attempting and not worth failing the
            // test over; the comparison's own verdict is the result.
            try {
                File.Copy(newFile, target, true);
            }
            catch (IOException) {
            }
            catch (UnauthorizedAccessException) {
            }
        }

        public static void CompareTextFileBaseline(string newFile, string baseline)
        {
            CompareTextFileBaseline(newFile, baseline, new Dictionary<string, string>());
        }

        // Compare text file against a baseline, showing a dialog if they don't compare.
        public static void CompareTextFileBaseline(string newFile, string baseline, Dictionary<string, string> exceptionMap)
        {
            baseline = TestUtil.GetSpecificFileName(baseline);

            if (!File.Exists(baseline) || !TestUtil.CompareTextFiles(newFile, baseline, exceptionMap)) {
                if (TestUtil.SilentRun) {
                    // Nobody is watching, so leave what was produced beside the
                    // baseline instead: the file the test wrote is often a
                    // temporary one that is gone by the time anyone looks.
                    SaveComparison(newFile, baseline);
                    Assert.Fail(string.Format("{0} and {1} do not compare", newFile, baseline));
                }
                else {
                    TextFileCompareDialog dialog = new TextFileCompareDialog();
                    dialog.BaselineFilename = baseline;
                    dialog.NewFilename = newFile;
                    DialogResult result = dialog.ShowDialog();
                    dialog.Dispose();

                    if (result == DialogResult.Cancel)
                        Assert.Fail(string.Format("{0} and {1} do not compare", newFile, baseline));
                }
            }
        }


    }
}

using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks that a document's text is drawn in the forms the application draws it in, whatever the locale of the
    // browser that reads it (T-11): a browser shapes text that names no language in the language of its locale, with
    // the forms a font holds for that language.
    [TestFixture]
    public class TextLanguageTests
    {
        [Test]
        public async Task ABrowserInARomanianLocaleDrawsTheTextInTheFormsTheApplicationDraws()
        {
            // Caladea, registered as Cambria, draws the s with a cedilla with a comma below for Romanian.
            RectangleF area = new RectangleF(0, 0, 40, 15);
            Size pixels = new Size(400, 150);
            string difference = await TextDrawingComparison.CompareAsync("romanian-locale", area, pixels, target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Cambria", 8, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("Şş şaş", font, black, new PointF(2, 2));
            }, "ro-RO");
            Assert.IsNull(difference, difference);
        }
    }
}

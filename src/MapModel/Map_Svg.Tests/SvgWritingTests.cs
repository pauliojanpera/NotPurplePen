using System;
using NUnit.Framework;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks how the SVG target writes numbers: whole steps, rounded halves up, and other values with only the
    // decimals they need, in its own culture-independent form (S-1, S-2, C-5, COL-4).
    [TestFixture]
    public class SvgNumbersTests
    {
        [Test]
        public void HalvesRoundUp()
        {
            Assert.AreEqual(3, SvgNumbers.RoundHalfUp(2.5));
            Assert.AreEqual(-2, SvgNumbers.RoundHalfUp(-2.5));
            Assert.AreEqual(-3, SvgNumbers.RoundHalfUp(-2.6));
        }

        [Test]
        public void LengthsBecomeWholeSteps()
        {
            Assert.AreEqual(12346, SvgNumbers.ToSteps(12.3456));
            Assert.AreEqual(0, SvgNumbers.ToSteps(-0.0004));
        }

        [Test]
        public void DecimalsAreWrittenInTheirShortestForm()
        {
            Assert.AreEqual(".5", SvgNumbers.FormatDecimal(0.5, 3));
            Assert.AreEqual("-.25", SvgNumbers.FormatDecimal(-0.25, 2));
            Assert.AreEqual("2", SvgNumbers.FormatDecimal(2, 3));
            Assert.AreEqual("12.34", SvgNumbers.FormatDecimal(12.34, 2));
            Assert.AreEqual(".13", SvgNumbers.FormatDecimal(0.125, 2));
            Assert.AreEqual("0", SvgNumbers.FormatDecimal(-0.0004, 3));
            Assert.AreEqual("1234567.89", SvgNumbers.FormatDecimal(1234567.891, 2));
        }

        [Test]
        public void StoredValuesKeepTheFewestDigitsThatGiveThemBack()
        {
            Assert.AreEqual(".455", SvgNumbers.FormatShortest(0.455F));
            Assert.AreEqual("1", SvgNumbers.FormatShortest(1F));
            Assert.AreEqual("0", SvgNumbers.FormatShortest(0F));
            Assert.AreEqual(".1", SvgNumbers.FormatShortest(0.1F));
            Assert.AreEqual(".33333334", SvgNumbers.FormatShortest(1F / 3));
        }

        [Test]
        public void DecimalsFollowTheTolerance()
        {
            Assert.AreEqual(0, SvgNumbers.DecimalsWithin(0.5));
            Assert.AreEqual(1, SvgNumbers.DecimalsWithin(0.05));
            Assert.AreEqual(2, SvgNumbers.DecimalsWithin(0.04));
            Assert.AreEqual(4, SvgNumbers.DecimalsWithin(0.0004));
        }

        [Test]
        public void IntegersAreWrittenWithTheirSign()
        {
            Assert.AreEqual("-1234", SvgNumbers.FormatInteger(-1234));
            Assert.AreEqual("0", SvgNumbers.FormatInteger(0));
        }
    }

    // Checks that classes and ids get the shortest free names, in turn (S-4, L-5).
    [TestFixture]
    public class SvgNamesTests
    {
        [Test]
        public void NamesGrowOnlyWhenTheShorterOnesAreGiven()
        {
            Assert.AreEqual("a", SvgNames.NameOf(0));
            Assert.AreEqual("z", SvgNames.NameOf(25));
            Assert.AreEqual("A", SvgNames.NameOf(26));
            Assert.AreEqual("Z", SvgNames.NameOf(51));
            Assert.AreEqual("aa", SvgNames.NameOf(52));
            Assert.AreEqual("a9", SvgNames.NameOf(113));
            Assert.AreEqual("ba", SvgNames.NameOf(114));
            Assert.AreEqual("Z9", SvgNames.NameOf(3275));
            Assert.AreEqual("aaa", SvgNames.NameOf(3276));
        }

        [Test]
        public void EachNameIsGivenOnce()
        {
            SvgNames names = new SvgNames();
            Assert.AreEqual("a", names.Next());
            Assert.AreEqual("b", names.Next());
        }
    }

    // Checks that path data takes the shortest form of each command (S-3).
    [TestFixture]
    public class SvgPathDataTests
    {
        // Build path data and return its text.
        // build: gives the commands.
        private static string Write(Action<SvgPathData> build)
        {
            SvgPathData path = new SvgPathData();
            build(path);
            return path.ToString();
        }

        [Test]
        public void HorizontalAndVerticalLinesTakeTheirShorterLetters()
        {
            Assert.AreEqual("M0 0h10v-20H-5h0z", Write(path => {
                path.MoveTo(0, 0);
                path.LineTo(10, 0);
                path.LineTo(10, -20);
                path.LineTo(-5, -20);
                path.LineTo(-5, -20);
                path.Close();
            }));
        }

        [Test]
        public void ARepeatedCommandLeavesOutItsLetter()
        {
            Assert.AreEqual("M100 100l10 20 10 20 10 20", Write(path => {
                path.MoveTo(100, 100);
                path.LineTo(110, 120);
                path.LineTo(120, 140);
                path.LineTo(130, 160);
            }));
        }

        [Test]
        public void EachCommandIsAbsoluteOrRelativeWhicheverIsShorter()
        {
            Assert.AreEqual("M100000 100000 5 7l99995 99993", Write(path => {
                path.MoveTo(100000, 100000);
                path.LineTo(5, 7);
                path.LineTo(100000, 100000);
            }));
        }

        [Test]
        public void ACurveWhoseFirstControlPointMirrorsTheLastIsSmooth()
        {
            Assert.AreEqual("M0 0c0 10 10 20 20 20S40 10 40 0c10 5 20 5 30 0", Write(path => {
                path.MoveTo(0, 0);
                path.CubicTo(0, 10, 10, 20, 20, 20);
                path.CubicTo(30, 20, 40, 10, 40, 0);
                path.CubicTo(50, 5, 60, 5, 70, 0);
            }));
        }

        [Test]
        public void ArcFlagsKeepTheirSeparators()
        {
            Assert.AreEqual("M0-10A10 10 0 1 1 10 0", Write(path => {
                path.MoveTo(0, -10);
                path.ArcTo(10, 10, true, true, 10, 0);
            }));
        }

        [Test]
        public void ASubpathAfterACloseStartsWithAMove()
        {
            Assert.AreEqual("M0 0h10v10zM20 0h10z", Write(path => {
                path.MoveTo(0, 0);
                path.LineTo(10, 0);
                path.LineTo(10, 10);
                path.Close();
                path.MoveTo(20, 0);
                path.LineTo(30, 0);
                path.Close();
            }));
        }
    }

    // Checks that transforms take their shortest form, with the digits their reach needs (S-2, S-8).
    [TestFixture]
    public class SvgTransformTests
    {
        // A rotation about the origin.
        // degrees: the angle.
        private static SvgTransform Rotation(double degrees)
        {
            double radians = degrees * Math.PI / 180;
            return new SvgTransform(Math.Cos(radians), Math.Sin(radians), -Math.Sin(radians), Math.Cos(radians), 0, 0);
        }

        [Test]
        public void ATranslationInStepsIsWritten()
        {
            Assert.AreEqual("translate(1000-2000)", new SvgTransform(1, 0, 0, 1, 1000, -2000).Format(500, 1));
        }

        [Test]
        public void AnAngleKeepsTheDigitsItsReachNeeds()
        {
            Assert.AreEqual("rotate(30)", Rotation(30).Format(1000, 1));
            Assert.AreEqual("rotate(12.35)", Rotation(12.3456).Format(1000, 1));
            Assert.AreEqual("rotate(12.3456)", Rotation(12.3456).Format(100000, 1));
        }

        [Test]
        public void AUniformScaleIsWrittenOnce()
        {
            Assert.AreEqual("scale(2)", new SvgTransform(2, 0, 0, 2, 0, 0).Format(1000, 1));
        }

        [Test]
        public void AMatrixIsWrittenWhereItIsShorterOrTheOnlyForm()
        {
            Assert.AreEqual("matrix(2 0 0-3 500 0)", new SvgTransform(2, 0, 0, -3, 500, 0).Format(1000, 1));
            Assert.AreEqual("matrix(1 0 .5 1 0 0)", new SvgTransform(1, 0, 0.5, 1, 0, 0).Format(1000, 1));
            Assert.AreEqual("matrix(.001 0 0-.001 10 287)", new SvgTransform(0.001, 0, 0, -0.001, 10, 287).Format(315000, 0.001));
        }

        [Test]
        public void TheIdentityIsNotWritten()
        {
            Assert.AreEqual("", SvgTransform.Identity.Format(1000, 1));
        }
    }
}

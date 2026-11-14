using System.Linq;
using NUnit.Framework;
using PurplePen.MapModel;
using static Map_Svg.Tests.TestJpegData;

namespace Map_Svg.Tests
{
    // Checks which JPEG data the SVG target takes as drawn by the browsers as the application draws the bitmap decoded
    // from it, which it then writes as it is (B-3): what the headers of the data say, read up to the scan.
    [TestFixture]
    public class SvgJpegDataTests
    {
        // The size of the bitmaps.
        private const int Width = 160, Height = 120;

        [Test]
        public void DataAsMostWritersWriteItIsDrawnAsDecoded()
        {
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(Plain(Width, Height), Width, Height));
        }

        [TestCase(BaselineFrame, 1)]
        [TestCase(BaselineFrame, 3)]
        [TestCase(BaselineFrame, 4)]
        [TestCase(ExtendedSequentialFrame, 3)]
        [TestCase(ProgressiveFrame, 1)]
        [TestCase(ProgressiveFrame, 3)]
        [TestCase(ProgressiveFrame, 4)]
        public void HuffmanCodedFramesOfGreyColourAndCmykAreDrawnAsDecoded(int frame, int channels)
        {
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(Data(Frame(frame, 8, Width, Height, channels)), Width, Height));
        }

        [TestCase(LosslessFrame)]
        [TestCase(HierarchicalFrame)]
        [TestCase(ArithmeticFrame)]
        [TestCase(ArithmeticProgressiveFrame)]
        public void FramesCodedOtherwiseAreNot(int frame)
        {
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Frame(frame, 8, Width, Height, 3)), Width, Height));
        }

        [TestCase(12, 3)]
        [TestCase(16, 3)]
        [TestCase(8, 2)]
        [TestCase(8, 5)]
        public void FramesOfMoreThan8BitsOrOfTwoOrFiveChannelsAreNot(int precision, int channels)
        {
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Frame(BaselineFrame, precision, Width, Height, channels)), Width, Height));
        }

        [TestCase(Width + 1, Height)]
        [TestCase(Width, Height - 1)]
        [TestCase(Width, 0)]
        public void AFrameOfAnotherSizeThanTheBitmapsIsNot(int width, int height)
        {
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Frame(BaselineFrame, 8, width, height, 3)), Width, Height));
        }

        [Test]
        public void DataWithTwoFramesOrNoneBeforeItsScanIsNot()
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(frame, frame), Width, Height));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Jfif()), Width, Height));
        }

        [Test]
        public void AFrameHeaderOfTheWrongLengthIsNot()
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            byte[] longer = Segment(BaselineFrame, frame.Skip(4).Concat(new byte[] { 0 }).ToArray());
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(longer), Width, Height));
        }

        [Test]
        public void TablesCommentsAndApplicationSegmentsAreDrawnAsDecoded()
        {
            byte[] adobe = Segment(AdobeSegment, new byte[] { (byte)'A', (byte)'d', (byte)'o', (byte)'b', (byte)'e', 0, 100, 0, 0, 0, 0, 1 });
            byte[] xmp = Segment(ExifSegment, System.Text.Encoding.ASCII.GetBytes("http://ns.adobe.com/xap/1.0/\0<x:xmpmeta/>"));
            byte[] multiPicture = Segment(ProfileSegment, new byte[] { (byte)'M', (byte)'P', (byte)'F', 0, 0, 0, 0, 0 });
            byte[] data = Data(Jfif(), adobe, xmp, multiPicture, Segment(Comment, new byte[] { (byte)'h', (byte)'i' }),
                               Segment(QuantizationTables, new byte[65]), Segment(RestartInterval, new byte[] { 0, 50 }),
                               Frame(BaselineFrame, 8, Width, Height, 3), Segment(HuffmanTables, new byte[29]));
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(data, Width, Height));
        }

        [TestCase(ArithmeticConditioning)]
        [TestCase(HierarchicalProgression)]
        [TestCase(0xC8)]
        [TestCase(0xF0)]
        public void OtherSegmentsAreNot(int marker)
        {
            byte[] data = Data(Segment(marker, new byte[4]), Frame(BaselineFrame, 8, Width, Height, 3));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(data, Width, Height));
        }

        [Test]
        public void ExifDataThatLeavesTheImageUprightIsDrawnAsDecoded()
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(Data(ExifOrientation(1), frame), Width, Height));
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(Data(Exif(true, (0x010F, ShortType, 7), (OrientationTag, ShortType, 1)), frame), Width, Height));
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(Data(Exif(false, (0x0110, ShortType, 7)), frame), Width, Height),
                          "Exif data without an orientation");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(0)]
        [TestCase(9)]
        public void ExifDataThatTurnsOrMirrorsTheImageOrSaysNothingValidIsNot(int orientation)
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(ExifOrientation(orientation), frame), Width, Height));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Exif(true, (OrientationTag, ShortType, orientation)), frame), Width, Height));
        }

        [Test]
        public void ExifDataThatCannotBeReadIsNot()
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            byte[] upright = ExifOrientation(1);
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Exif(false, (OrientationTag, LongType, 1)), frame), Width, Height),
                           "an orientation that is not a SHORT");

            byte[] byteOrder = (byte[])upright.Clone();
            byteOrder[10] = (byte)'X';
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(byteOrder, frame), Width, Height), "no byte order");

            byte[] magic = (byte[])upright.Clone();
            magic[12] = 43;
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(magic, frame), Width, Height), "not 42");

            byte[] directoryBeyond = (byte[])upright.Clone();
            directoryBeyond[14] = 200;
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(directoryBeyond, frame), Width, Height), "a directory beyond the segment");

            byte[] entriesBeyond = (byte[])upright.Clone();
            entriesBeyond[18] = 9;
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(entriesBeyond, frame), Width, Height), "entries beyond the segment");
        }

        [Test]
        public void DataWithAColourProfileIsNot()
        {
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Jfif(), Profile(), Frame(BaselineFrame, 8, Width, Height, 3)), Width, Height));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(Data(Frame(BaselineFrame, 8, Width, Height, 4), Profile()), Width, Height));
        }

        [Test]
        public void MarkersMayFollowFillBytes()
        {
            byte[] frame = Frame(BaselineFrame, 8, Width, Height, 3);
            byte[] data = Data(new byte[] { 0xFF, 0xFF }.Concat(Jfif()).ToArray(), new byte[] { 0xFF }.Concat(frame).ToArray());
            Assert.IsTrue(SvgJpegData.IsDrawnAsDecoded(data, Width, Height));
        }

        [Test]
        public void DataThatIsNotJpegOrIsCutShortIsNot()
        {
            byte[] plain = Plain(Width, Height);
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(null, Width, Height));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(new byte[0], Width, Height));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' }, Width, Height));
            int scan = plain.Length - 18;
            for (int length = 1; length < scan + 2; ++length)
                Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(plain.Take(length).ToArray(), Width, Height), "cut short to " + length + " bytes");

            byte[] gap = plain.Take(20).Concat(new byte[] { 0x12 }).Concat(plain.Skip(20)).ToArray();
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(gap, Width, Height), "a byte between segments that starts no marker");

            byte[] tooShort = Data(Segment(Comment, new byte[0]).Take(3).Concat(new byte[] { 1 }).ToArray(), Frame(BaselineFrame, 8, Width, Height, 3));
            Assert.IsFalse(SvgJpegData.IsDrawnAsDecoded(tooShort, Width, Height), "a segment shorter than its length");
        }
    }
}

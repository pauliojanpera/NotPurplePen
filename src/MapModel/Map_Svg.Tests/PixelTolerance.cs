namespace Map_Svg.Tests
{
    // How far Chromium's image of a document may differ from the image it should match (F-1). Two rasterizers shade
    // the antialiased edges of shapes differently, so a pixel is held not to the one pixel at its place but to the
    // colors the other image has around it: each color channel may lie anywhere in the range that channel takes in
    // the other image within Reach pixels, widened by Slack levels, and this holds both ways round, so neither image
    // may show what the other lacks. Where neither image has an edge within that reach, the rule asks for a match
    // within Slack. Where the edges of two shapes meet in one pixel, each rasterizer mixes their colors and the color
    // beneath its own way, which can fall outside every neighbour's range; such a pixel still passes when the two images,
    // each blurred by a Gaussian of EdgeBlur pixels, differ there by at most EdgeLevels in every channel. Beyond the
    // image the page is blank paper, so both images count as white there.
    public readonly struct PixelTolerance
    {
        // The tolerance of the rendering comparison, chosen from the first 99 drawings the target draws. Their pixels
        // differ by up to 191 levels, along edges, which the two rasterizers shade differently; where the edges of two
        // shapes meet, 53 of them fall up to 60 levels outside the range, while their blurred images differ there by at
        // most 34.2. The rule passes all 99, and fails each of those 53 shifted by two pixels or with its features two
        // pixels wider, and all but one of them with its smallest solid feature removed.
        public static readonly PixelTolerance Default = new PixelTolerance(1, 4, 1, 40);

        // No tolerance at all, for shapes on whole pixels, which every rasterizer draws alike.
        public static readonly PixelTolerance Exact = new PixelTolerance(0, 0, 0, 0);

        // How many pixels away a color may come from in the other image.
        public readonly int Reach;

        // How many levels a color channel may lie beyond the range it is held to.
        public readonly int Slack;

        // The standard deviation, in pixels, of the blur under which an edge pixel outside the range is judged.
        public readonly double EdgeBlur;

        // How many levels the blurred images may differ by at such a pixel.
        public readonly int EdgeLevels;

        // Create a tolerance.
        // reach: how many pixels away a color may come from in the other image.
        // slack: how many levels a color channel may lie beyond the range it is held to.
        // edgeBlur: the standard deviation of the blur under which an edge pixel outside the range is judged.
        // edgeLevels: how many levels the blurred images may differ by at such a pixel.
        public PixelTolerance(int reach, int slack, double edgeBlur, int edgeLevels)
        {
            Reach = reach;
            Slack = slack;
            EdgeBlur = edgeBlur;
            EdgeLevels = edgeLevels;
        }

        // Describe the tolerance in the words of a test failure.
        public override string ToString()
        {
            return string.Format("each color channel within the range the other image takes within {0} pixel{1}, give or take {2} levels, " +
                                 "or at an edge within {3} levels of it once both are blurred by {4} pixel{5}",
                                 Reach, Reach == 1 ? "" : "s", Slack, EdgeLevels, EdgeBlur, EdgeBlur == 1 ? "" : "s");
        }
    }
}

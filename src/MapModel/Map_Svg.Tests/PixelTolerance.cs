namespace Map_Svg.Tests
{
    // How far Chromium's image of a document may differ from the image it should match (F-1). Two rasterizers shade
    // the antialiased edges of shapes differently, so a pixel is held not to the one pixel at its place but to the
    // colors the other image has around it: each color channel may lie anywhere in the range that channel takes in
    // the other image within Reach pixels, widened by Slack levels. The rule holds both ways round, so neither image
    // may show what the other lacks. Inside a shape it asks for a match within Slack; along an edge it lets any shade
    // between the colors that meet there pass. Beyond the image the page is blank paper, so both images count as white
    // there.
    public readonly struct PixelTolerance
    {
        // The tolerance of the rendering comparison, chosen from the first cases, whose edges differ by up to 108
        // levels while every shape lies within 0.15 pixel of where the reference puts it: the rule passes them with no
        // pixel beyond a slack of 2, and fails them shifted by two pixels, with a feature removed, with features two
        // pixels wider, or with an ink six levels redder.
        public static readonly PixelTolerance Default = new PixelTolerance(1, 4);

        // No tolerance at all, for shapes on whole pixels, which every rasterizer draws alike.
        public static readonly PixelTolerance Exact = new PixelTolerance(0, 0);

        // How many pixels away a color may come from in the other image.
        public readonly int Reach;

        // How many levels a color channel may lie beyond the range it is held to.
        public readonly int Slack;

        // Create a tolerance.
        // reach: how many pixels away a color may come from in the other image.
        // slack: how many levels a color channel may lie beyond the range it is held to.
        public PixelTolerance(int reach, int slack)
        {
            Reach = reach;
            Slack = slack;
        }

        // Describe the tolerance in the words of a test failure.
        public override string ToString()
        {
            return string.Format("each color channel within the range the other image takes within {0} pixel{1}, give or take {2} levels",
                                 Reach, Reach == 1 ? "" : "s", Slack);
        }
    }
}

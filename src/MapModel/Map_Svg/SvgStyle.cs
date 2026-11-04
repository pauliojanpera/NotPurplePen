namespace PurplePen.MapModel
{
    // The declarations of a shape's style, which name its class when the shape is written (S-4), and the pattern they
    // paint with, if any. A pattern's id is named when the document is written, in the order patterns are first used
    // (F-2), so the declarations refer to it as url(#), which the id then fills in.
    public sealed class SvgStyle
    {
        // How the declarations refer to their pattern, before its id is known.
        public const string PatternReference = "url(#)";

        // The declarations, such as "fill:#f00" or "stroke:url(#)".
        public readonly string Declarations;

        // The pattern the declarations paint with, or null for none.
        public readonly SvgPattern Pattern;

        // Create a style that paints with colors alone.
        // declarations: the declarations.
        public SvgStyle(string declarations)
            : this(declarations, null)
        {
        }

        // Create a style.
        // declarations: the declarations, which refer to the pattern, if any, as url(#).
        // pattern: the pattern they paint with, or null for none.
        public SvgStyle(string declarations, SvgPattern pattern)
        {
            Declarations = declarations;
            Pattern = pattern;
        }

        // The style with further declarations around its own.
        // before: the declarations that go before, each followed by a semicolon.
        // after: the declarations that go after, each preceded by a semicolon.
        public SvgStyle Around(string before, string after)
        {
            return new SvgStyle(before + Declarations + after, Pattern);
        }

        // The declarations as they are written: with the pattern's id, which is named when the pattern is first used.
        // writer: the writer of the document.
        public string Resolve(SvgWriter writer)
        {
            if (Pattern == null)
                return Declarations;
            return Declarations.Replace(PatternReference, "url(#" + writer.IdOfPattern(Pattern) + ")");
        }
    }
}

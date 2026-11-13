using System;

namespace PurplePen.MapModel
{
    // The declarations of an element's style, which the element, or the group that holds it, writes as its style
    // attribute (S-4), the pattern they paint with, if any, and the filter that lightens an image, if any (B-6). The ids
    // of patterns and filters are named when the document is written, in the order they are first used (F-2), so the
    // declarations refer to a pattern as url(#) and to a filter as url(#!), which the ids then fill in.
    public sealed class SvgStyle
    {
        // How the declarations refer to their pattern, before its id is known.
        public const string PatternReference = "url(#)";

        // How the declarations refer to their filter, before its id is known.
        public const string FilterReference = "url(#!)";

        // The declarations, such as "fill:#f00", "stroke:url(#)" or "filter:url(#!)".
        public readonly string Declarations;

        // The pattern the declarations paint with, or null for none.
        public readonly SvgPattern Pattern;

        // The filter the declarations draw an image through, or null for none.
        public readonly SvgLighterFilter Filter;

        // Create a style that paints with colors alone.
        // declarations: the declarations.
        public SvgStyle(string declarations)
            : this(declarations, null, null)
        {
        }

        // Create a style that may paint with a pattern.
        // declarations: the declarations, which refer to the pattern, if any, as url(#).
        // pattern: the pattern they paint with, or null for none.
        public SvgStyle(string declarations, SvgPattern pattern)
            : this(declarations, pattern, null)
        {
        }

        // Create a style.
        // declarations: the declarations, which refer to the pattern, if any, as url(#), and to the filter, if any, as
        //               url(#!).
        // pattern: the pattern they paint with, or null for none.
        // filter: the filter they draw an image through, or null for none.
        public SvgStyle(string declarations, SvgPattern pattern, SvgLighterFilter filter)
        {
            Declarations = declarations;
            Pattern = pattern;
            Filter = filter;
        }

        // The style with further declarations around its own.
        // before: the declarations that go before, each followed by a semicolon.
        // after: the declarations that go after, each preceded by a semicolon.
        public SvgStyle Around(string before, string after)
        {
            return new SvgStyle(before + Declarations + after, Pattern, Filter);
        }

        // Whether another style is the same: the same declarations, painting with the same pattern, if any, and drawing
        // through the same filter, if any. Elements of the same style share it through their group (S-4).
        // other: the other style, or null for none.
        public bool IsSameAs(SvgStyle other)
        {
            return other != null && Pattern == other.Pattern && Filter == other.Filter &&
                   string.Equals(Declarations, other.Declarations, StringComparison.Ordinal);
        }

        // The declarations as they are written: with the ids of the pattern and the filter, which are named when each is
        // first used.
        // writer: the writer of the document.
        public string Resolve(SvgWriter writer)
        {
            string declarations = Declarations;
            if (Pattern != null)
                declarations = declarations.Replace(PatternReference, "url(#" + writer.IdOfPattern(Pattern) + ")");
            if (Filter != null)
                declarations = declarations.Replace(FilterReference, "url(#" + writer.IdOfFilter(Filter) + ")");
            return declarations;
        }
    }
}

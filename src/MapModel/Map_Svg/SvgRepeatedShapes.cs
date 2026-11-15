using System;
using System.Collections.Generic;
using System.Text;

namespace PurplePen.MapModel
{
    // Shapes that several groups of the document's parts hold alike, each group under a transform of its own: a glyph's
    // parts of one color, which a point symbol draws for each of its objects, a line for each glyph along it and an area
    // drawn as separate shapes for each glyph of its pattern. The document defines them once, and the use of the
    // definition takes the place of each group that held them (S-5).
    public sealed class SvgRepeatedShapes
    {
        // The fewest groups that must hold the same shapes for them to be defined once: shapes that one group holds stay
        // in it, since their definition and its use would be longer than the group.
        private const int FewestHolders = 2;

        // The shapes, as the first group that held them drew them.
        private readonly List<SvgShape> shapes;

        // Whether each shape carries its own style, as when they differ in style. Shapes of one style are defined
        // without it, and each use carries it instead.
        private readonly bool withStyles;

        // A group that holds shapes which may repeat, where it lies, and what its shapes are written as.
        private sealed class Holder
        {
            // The group that holds the group, and where in it the group lies.
            public readonly SvgGroup Parent;
            public readonly int Index;

            // The group.
            public readonly SvgGroup Group;

            // The shapes' text, as their definition holds them: the text by which repeats are found.
            public readonly string Text;

            // The style the shapes share, or null when they differ in style.
            public readonly SvgStyle SharedStyle;

            // The number of the object the shapes belong to, or 0 for none.
            public readonly int ObjectNumber;

            // Create the holder of shapes.
            // parent: the group that holds the group.
            // index: where in the parent the group lies.
            // group: the group.
            // text: the shapes' text, as their definition holds them.
            // sharedStyle: the style the shapes share, or null when they differ in style.
            // objectNumber: the number of the object the shapes belong to, or 0 for none.
            public Holder(SvgGroup parent, int index, SvgGroup group, string text, SvgStyle sharedStyle, int objectNumber)
            {
                Parent = parent;
                Index = index;
                Group = group;
                Text = text;
                SharedStyle = sharedStyle;
                ObjectNumber = objectNumber;
            }
        }

        // Create the definition of repeated shapes.
        // shapes: the shapes, as the first group that held them drew them.
        // withStyles: whether each shape carries its own style.
        private SvgRepeatedShapes(List<SvgShape> shapes, bool withStyles)
        {
            this.shapes = shapes;
            this.withStyles = withStyles;
        }

        // Define once the shapes that two or more groups within the parts hold alike, and put the use of the definition
        // in place of each such group (S-5). A group holds such shapes when it carries a transform, and no clip, style or
        // blending, and holds nothing but shapes of one object; the shapes are alike when they are written alike, without
        // their object numbers, and, when the shapes of each group share one style, without the style, which the use
        // then carries. A group that also carries an identity, such as a symbol's, keeps it, and holds the use instead.
        // contents: the tidied contents of the parts.
        public static void PlaceByReference(IEnumerable<SvgGroup> contents)
        {
            List<Holder> holders = new List<Holder>();
            foreach (SvgGroup content in contents)
                FindHolders(content, holders);

            Dictionary<string, int> holdersOfText = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Holder holder in holders) {
                int count;
                holdersOfText.TryGetValue(holder.Text, out count);
                holdersOfText[holder.Text] = count + 1;
            }

            Dictionary<string, SvgRepeatedShapes> definitions = new Dictionary<string, SvgRepeatedShapes>(StringComparer.Ordinal);
            foreach (Holder holder in holders) {
                if (holdersOfText[holder.Text] < FewestHolders)
                    continue;
                SvgRepeatedShapes definition;
                if (!definitions.TryGetValue(holder.Text, out definition)) {
                    definition = new SvgRepeatedShapes(holder.Group.Children.ConvertAll(child => (SvgShape)child), holder.SharedStyle == null);
                    definitions.Add(holder.Text, definition);
                }

                SvgRepeatedShapesUse use = new SvgRepeatedShapesUse(definition, holder.Group.Transform, holder.SharedStyle, holder.Group.GetBoundsOfChildren()) {
                    ObjectNumber = holder.ObjectNumber
                };
                if (holder.Group.Identity == null) {
                    holder.Parent.Children[holder.Index] = use;
                }
                else {
                    holder.Group.Children = new List<SvgNode> { use };
                    holder.Group.Transform = null;
                }
            }
        }

        // Write the definition: a single shape carries the id itself, and several are held in a group that carries it.
        // writer: the writer of the document.
        public void WriteDefinition(SvgWriter writer)
        {
            string id = writer.IdOfRepeatedShapes(this);
            if (shapes.Count == 1) {
                shapes[0].AppendDefinition(writer.Text, id, withStyles);
                return;
            }
            writer.Text.Append("<g id=\"").Append(id).Append("\">\n");
            foreach (SvgShape shape in shapes)
                shape.AppendDefinition(writer.Text, null, withStyles);
            writer.Text.Append("</g>\n");
        }

        // Find the groups within a group, at any depth, that hold shapes which may repeat, in the order they are drawn.
        // parent: the group.
        // holders: receives the holders found.
        private static void FindHolders(SvgGroup parent, List<Holder> holders)
        {
            for (int index = 0; index < parent.Children.Count; ++index) {
                SvgGroup group = parent.Children[index] as SvgGroup;
                if (group == null)
                    continue;
                Holder holder = HolderOf(parent, index, group);
                if (holder != null)
                    holders.Add(holder);
                else
                    FindHolders(group, holders);
            }
        }

        // The holder that a group is when it holds shapes which may repeat, or else null: when it carries a transform and
        // no clip, style or blending, and holds nothing but shapes of one object. Shapes that differ in style and paint
        // with a pattern may not repeat, since their text would name the pattern before its id is known.
        // parent: the group that holds the group.
        // index: where in the parent the group lies.
        // group: the group.
        private static Holder HolderOf(SvgGroup parent, int index, SvgGroup group)
        {
            if (group.Transform == null || group.Clip != null || group.Style != null || group.Blends || group.Children.Count == 0)
                return null;

            SvgShape first = group.Children[0] as SvgShape;
            if (first == null)
                return null;
            bool stylesDiffer = false, paintsWithPattern = false;
            foreach (SvgNode child in group.Children) {
                SvgShape shape = child as SvgShape;
                if (shape == null || shape.ObjectNumber != first.ObjectNumber)
                    return null;
                stylesDiffer = stylesDiffer || !shape.ElementStyle.IsSameAs(first.ElementStyle);
                paintsWithPattern = paintsWithPattern || shape.ElementStyle.Pattern != null;
            }
            if (stylesDiffer && paintsWithPattern)
                return null;

            StringBuilder text = new StringBuilder();
            foreach (SvgNode child in group.Children)
                ((SvgShape)child).AppendDefinition(text, null, stylesDiffer);
            return new Holder(parent, index, group, text.ToString(), stylesDiffer ? null : first.ElementStyle, first.ObjectNumber);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PurplePen.MapModel
{
    // A node of a document being written: a drawn element, a group of nodes, or a reference to a part.
    public abstract class SvgNode
    {
        // The bounds of what the node draws, in its parent's frame.
        public abstract SvgBounds GetBounds();

        // Write the node and everything within it, one element to a line (S-9).
        // writer: the writer of the document.
        public abstract void Write(SvgWriter writer);
    }

    // An element that a drawing call writes: a shape, or the use of an image. Drawn within an object, it carries the
    // object's number (M-6, S-11).
    public abstract class SvgElement : SvgNode
    {
        // The number of the object the element belongs to, or 0 for none.
        public int ObjectNumber;

        // Write the number of the element's object, if it belongs to one.
        // writer: the writer of the document.
        protected void WriteObjectNumber(SvgWriter writer)
        {
            if (ObjectNumber != 0)
                writer.Text.Append(" data-o=\"").Append(SvgNumbers.FormatInteger(ObjectNumber)).Append('"');
        }
    }

    // A shape drawn with one style: a path, a circle or an ellipse (F-3). Its element carries its class, the number of
    // its object, if any, and its geometry, and nothing else (S-4, S-11).
    public abstract class SvgShape : SvgElement
    {
        // The shape's style, which names its class when it is written (S-4).
        protected readonly SvgStyle Style;

        // Create a shape.
        // style: its style.
        protected SvgShape(SvgStyle style)
        {
            Style = style;
        }

        // Write the element: its class, the number of its object, if any, and its geometry.
        public override void Write(SvgWriter writer)
        {
            string className = writer.ClassOf(Style.Resolve(writer));
            writer.Text.Append('<').Append(ElementName).Append(" class=\"").Append(className).Append('"');
            WriteObjectNumber(writer);
            writer.Text.Append(' ');
            AppendGeometry(writer.Text);
            writer.Text.Append("/>\n");
        }

        // The name of the element.
        protected abstract string ElementName { get; }

        // Append the attributes that give the shape's geometry.
        // text: the text to append to.
        protected abstract void AppendGeometry(StringBuilder text);
    }

    // A path (F-3).
    public sealed class SvgPath : SvgShape
    {
        // The path's data, in steps.
        private readonly SvgPathData data;

        // Create a path.
        // style: its style.
        // data: its data.
        public SvgPath(SvgStyle style, SvgPathData data)
            : base(style)
        {
            this.data = data;
        }

        // The bounds of the path's points, control points included.
        public override SvgBounds GetBounds()
        {
            return data.Bounds;
        }

        // The name of the element.
        protected override string ElementName
        {
            get { return "path"; }
        }

        // Append the path's data.
        protected override void AppendGeometry(StringBuilder text)
        {
            text.Append("d=\"").Append(data.ToString()).Append('"');
        }
    }

    // An ellipse, written as a circle when its radii are equal. A center coordinate of 0 is the default, which is left
    // out (F-3, S-9).
    public sealed class SvgEllipse : SvgShape
    {
        // The center and the radii, in steps.
        private readonly long centerX, centerY, radiusX, radiusY;

        // Create an ellipse.
        // style: its style.
        // centerX, centerY: its center, in steps.
        // radiusX, radiusY: its radii, in steps.
        public SvgEllipse(SvgStyle style, long centerX, long centerY, long radiusX, long radiusY)
            : base(style)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.radiusX = radiusX;
            this.radiusY = radiusY;
        }

        // The same ellipse moved by a whole number of steps, in the same object.
        // x, y: how far it moves along each axis, in steps.
        public SvgEllipse MovedBy(long x, long y)
        {
            return new SvgEllipse(Style, centerX + x, centerY + y, radiusX, radiusY) { ObjectNumber = ObjectNumber };
        }

        // The bounds of the ellipse.
        public override SvgBounds GetBounds()
        {
            SvgBounds bounds = new SvgBounds();
            bounds.Include(centerX - radiusX, centerY - radiusY);
            bounds.Include(centerX + radiusX, centerY + radiusY);
            return bounds;
        }

        // The name of the element: a circle when the radii are equal.
        protected override string ElementName
        {
            get { return radiusX == radiusY ? "circle" : "ellipse"; }
        }

        // Append the center, where it is not the default, and the radii.
        protected override void AppendGeometry(StringBuilder text)
        {
            if (centerX != 0)
                text.Append("cx=\"").Append(SvgNumbers.FormatInteger(centerX)).Append("\" ");
            if (centerY != 0)
                text.Append("cy=\"").Append(SvgNumbers.FormatInteger(centerY)).Append("\" ");
            if (radiusX == radiusY)
                text.Append("r=\"").Append(SvgNumbers.FormatInteger(radiusX)).Append('"');
            else
                text.Append("rx=\"").Append(SvgNumbers.FormatInteger(radiusX)).Append("\" ry=\"").Append(SvgNumbers.FormatInteger(radiusY)).Append('"');
        }
    }

    // The use of an image, which draws it over a rectangle through the image's one definition (B-1, B-5).
    public sealed class SvgImageUse : SvgElement
    {
        // The image.
        private readonly SvgImage image;

        // The transform from the image's pixels to the rectangle, in steps.
        private readonly SvgTransform placement;

        // The declarations of the element's style, or null for none.
        private readonly string style;

        // The bounds of the rectangle, in steps.
        private readonly SvgBounds bounds;

        // Create the use of an image.
        // image: the image.
        // placement: the transform from its pixels to the rectangle, in steps.
        // style: the declarations of the element's style, or null for none.
        // bounds: the bounds of the rectangle.
        public SvgImageUse(SvgImage image, SvgTransform placement, string style, SvgBounds bounds)
        {
            this.image = image;
            this.placement = placement;
            this.style = style;
            this.bounds = bounds;
        }

        // The bounds of the rectangle.
        public override SvgBounds GetBounds()
        {
            return bounds;
        }

        // Write the element: its class, if any, the number of its object, if any, the reference to the image and the
        // placement, with the digits that the image's corners need (S-2, S-4).
        public override void Write(SvgWriter writer)
        {
            writer.Text.Append("<use");
            if (style != null)
                writer.Text.Append(" class=\"").Append(writer.ClassOf(style)).Append('"');
            WriteObjectNumber(writer);
            writer.Text.Append(" xlink:href=\"#").Append(writer.IdOfImage(image)).Append('"');
            string transform = placement.Format(Math.Sqrt((double)image.Width * image.Width + (double)image.Height * image.Height), 1);
            if (transform.Length > 0)
                writer.Text.Append(" transform=\"").Append(transform).Append('"');
            writer.Text.Append("/>\n");
        }
    }

    // A text in one font and one paint, its baseline starting at a point, each character after the first moved by the
    // kerning between it and the one before, as the application's shaping kerns them (T-1 to T-3). Its element carries
    // its class, the number of its object, if any, its start, unless at the origin, and the kerning, unless there is
    // none, in steps (S-9, S-11); then the characters, escaped for XML.
    public sealed class SvgText : SvgElement
    {
        // The style: the font and the paint, which name the element's class (S-4).
        private readonly SvgStyle style;

        // The start of the baseline, in steps.
        private readonly long x, y;

        // The kerning before each character after the first, in steps, without the zeros that end it.
        private readonly long[] kerning;

        // The characters.
        private readonly string text;

        // The bounds of the text: from its start to its advance, and from its top to its descent.
        private readonly SvgBounds bounds;

        // Create a text.
        // style: its font and paint.
        // x, y: the start of its baseline, in steps.
        // kerning: the kerning before each character after the first, in steps, without the zeros that end it.
        // text: the characters.
        // bounds: the bounds of the text.
        public SvgText(SvgStyle style, long x, long y, long[] kerning, string text, SvgBounds bounds)
        {
            this.style = style;
            this.x = x;
            this.y = y;
            this.kerning = kerning;
            this.text = text;
            this.bounds = bounds;
        }

        // The bounds of the text.
        public override SvgBounds GetBounds()
        {
            return bounds;
        }

        // Write the element. The kerning is written as dx, whose first value, the shift of the first character, is 0.
        public override void Write(SvgWriter writer)
        {
            writer.Text.Append("<text class=\"").Append(writer.ClassOf(style.Resolve(writer))).Append('"');
            WriteObjectNumber(writer);
            if (x != 0)
                writer.Text.Append(" x=\"").Append(SvgNumbers.FormatInteger(x)).Append('"');
            if (y != 0)
                writer.Text.Append(" y=\"").Append(SvgNumbers.FormatInteger(y)).Append('"');
            if (kerning.Length > 0) {
                writer.Text.Append(" dx=\"0");
                foreach (long shift in kerning)
                    writer.Text.Append(' ').Append(SvgNumbers.FormatInteger(shift));
                writer.Text.Append('"');
            }
            writer.Text.Append('>');
            foreach (char character in text) {
                if (character == '&')
                    writer.Text.Append("&amp;");
                else if (character == '<')
                    writer.Text.Append("&lt;");
                else if (character == '>')
                    writer.Text.Append("&gt;");
                else
                    writer.Text.Append(character);
            }
            writer.Text.Append("</text>\n");
        }
    }

    // A group that encloses what a push or a structure call opened it for, with the attributes it carries: a transform,
    // a clip, a style, or the identity of what it holds. Groups are tidied before they are written (S-8).
    public sealed class SvgGroup : SvgNode
    {
        // The transform from the group's frame to its parent's, or null for none.
        public SvgTransform Transform;

        // The clip, in the group's own frame, or null for none.
        public SvgClip Clip;

        // The declarations of the group's style, or null for none.
        public string Style;

        // Whether the style blends the group with what lies below it, so that it never merges with another group.
        public bool Blends;

        // The attributes that give the identity of what the group holds, such as data-m="1", or null for none.
        public string Identity;

        // What the group holds, in drawing order.
        public List<SvgNode> Children = new List<SvgNode>();

        // The bounds of what the group draws, in its parent's frame, once computed.
        private SvgBounds bounds;

        // Whether the group carries anything that its children would not carry without it.
        public bool CarriesAttributes
        {
            get { return Transform != null || Clip != null || Style != null || Identity != null; }
        }

        // The bounds of what the group draws, in its parent's frame.
        public override SvgBounds GetBounds()
        {
            if (bounds == null) {
                SvgBounds inner = GetBoundsOfChildren();
                if (Transform == null) {
                    bounds = inner;
                }
                else {
                    bounds = new SvgBounds();
                    bounds.IncludeTransformed(inner, Transform);
                }
            }
            return bounds;
        }

        // The bounds of what the group draws, in its own frame.
        public SvgBounds GetBoundsOfChildren()
        {
            SvgBounds inner = new SvgBounds();
            foreach (SvgNode child in Children)
                inner.Include(child.GetBounds());
            return inner;
        }

        // Write the group's start tag, its children and its end tag.
        public override void Write(SvgWriter writer)
        {
            writer.Text.Append("<g");
            if (Style != null)
                writer.Text.Append(" class=\"").Append(writer.ClassOf(Style)).Append('"');
            if (Transform != null) {
                string transform = Transform.Format(GetBoundsOfChildren().ReachFromOrigin, 1);
                if (transform.Length > 0)
                    writer.Text.Append(" transform=\"").Append(transform).Append('"');
            }
            if (Clip != null)
                writer.Text.Append(" clip-path=\"url(#").Append(writer.IdOfClip(Clip)).Append(")\"");
            if (Identity != null)
                writer.Text.Append(' ').Append(Identity);
            writer.Text.Append(">\n");
            WriteChildren(writer);
            writer.Text.Append("</g>\n");
        }

        // Write the children.
        // writer: the writer of the document.
        public void WriteChildren(SvgWriter writer)
        {
            foreach (SvgNode child in Children)
                child.Write(writer);
        }

        // Tidy the group and everything within it, and return what takes its place in its parent: nothing when it holds
        // nothing, its children when it carries nothing, the ellipse it holds moved into place when it only moves a
        // single ellipse, and otherwise the group itself (S-8).
        public List<SvgNode> Tidy()
        {
            TidyChildren();

            List<SvgNode> replacement = new List<SvgNode>();
            if (Children.Count == 0)
                return replacement;
            if (!CarriesAttributes) {
                replacement.AddRange(Children);
                return replacement;
            }

            SvgGroup onlyChild = Children.Count == 1 ? Children[0] as SvgGroup : null;
            while (onlyChild != null && CanAbsorb(onlyChild)) {
                Absorb(onlyChild);
                onlyChild = Children.Count == 1 ? Children[0] as SvgGroup : null;
            }

            // The center of an ellipse takes the move in whole steps, as every coordinate is written.
            SvgEllipse onlyEllipse = Children.Count == 1 ? Children[0] as SvgEllipse : null;
            if (onlyEllipse != null && Transform != null && Transform.IsTranslation && Clip == null && Style == null && Identity == null) {
                replacement.Add(onlyEllipse.MovedBy(SvgNumbers.RoundHalfUp(Transform.E), SvgNumbers.RoundHalfUp(Transform.F)));
                return replacement;
            }

            replacement.Add(this);
            return replacement;
        }

        // Tidy the children: each in turn, with adjacent groups that carry the same attributes merged into one.
        public void TidyChildren()
        {
            List<SvgNode> tidied = new List<SvgNode>();
            foreach (SvgNode child in Children) {
                SvgGroup group = child as SvgGroup;
                if (group == null) {
                    AppendMerging(tidied, child);
                    continue;
                }
                foreach (SvgNode node in group.Tidy())
                    AppendMerging(tidied, node);
            }
            Children = tidied;
            bounds = null;
        }

        // Append a tidied node to tidied nodes, merging it into the last of them when both are groups that carry the
        // same attributes; the children of a merged group then meet the last one's children, and merge in turn.
        // nodes: the tidied nodes.
        // node: the node to append.
        private static void AppendMerging(List<SvgNode> nodes, SvgNode node)
        {
            SvgGroup previous = nodes.Count > 0 ? nodes[nodes.Count - 1] as SvgGroup : null;
            SvgGroup current = node as SvgGroup;
            if (previous != null && current != null && previous.CarriesTheSameAttributesAs(current)) {
                foreach (SvgNode child in current.Children)
                    AppendMerging(previous.Children, child);
                previous.bounds = null;
            }
            else {
                nodes.Add(node);
            }
        }

        // Whether another group carries the same attributes, so that the two can be one group when adjacent; groups that
        // blend never merge, since blending each of two groups differs from blending them together where they overlap.
        // other: the other group.
        private bool CarriesTheSameAttributesAs(SvgGroup other)
        {
            if (Blends || other.Blends)
                return false;
            bool sameTransform = (Transform == null) ? other.Transform == null
                                                     : other.Transform != null && Transform.HasSameElements(other.Transform);
            return sameTransform && Clip == other.Clip && Style == other.Style && Identity == other.Identity;
        }

        // Whether the group can take on the attributes of its only child and hold the child's children instead, with
        // nothing drawn differently: not when both carry one kind of attribute, and not when the child's transform would
        // move this group's clip.
        // child: the only child.
        private bool CanAbsorb(SvgGroup child)
        {
            if (Style != null && child.Style != null)
                return false;
            if (Identity != null && child.Identity != null)
                return false;
            if (Clip != null && (child.Clip != null || child.Transform != null))
                return false;
            return true;
        }

        // Take on the attributes of the only child and hold its children instead.
        // child: the only child.
        private void Absorb(SvgGroup child)
        {
            if (child.Transform != null)
                Transform = (Transform == null) ? child.Transform : Transform.After(child.Transform);
            Clip = Clip ?? child.Clip;
            Style = Style ?? child.Style;
            Blends = Blends || child.Blends;
            Identity = Identity ?? child.Identity;
            Children = child.Children;
            bounds = null;
        }
    }

    // A reference to a part, by which a page shows it.
    public sealed class SvgPartReference : SvgNode
    {
        // The part referred to.
        private readonly SvgPart part;

        // Create a reference to a part.
        // part: the part.
        public SvgPartReference(SvgPart part)
        {
            this.part = part;
        }

        // The bounds of the part.
        public override SvgBounds GetBounds()
        {
            return part.Content.GetBoundsOfChildren();
        }

        // Write the reference, through xlink:href, which SVG 1.1 readers need (F-3).
        public override void Write(SvgWriter writer)
        {
            writer.Text.Append("<use xlink:href=\"#").Append(writer.IdOf(part)).Append("\"/>\n");
        }
    }
}

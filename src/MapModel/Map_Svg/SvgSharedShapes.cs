using System.Collections.Generic;

namespace PurplePen.MapModel
{
    // The shared shapes of a glyph in a palette color: the shapes of the parts of the glyph in the color, each with its
    // fill rule or its pen, which each placement of the glyph refers to (ClippingObject.IsSharedShape). The document
    // defines them once in its definitions, and a use places them at each placement (SvgSharedShapesUse). They carry no
    // paint: a use takes the paint of the transparency group of the palette color, or of the pattern whose tile holds it.
    public sealed class SvgSharedShapes
    {
        // The shapes, each with its style, in the order of the parts of the glyph.
        private readonly IReadOnlyList<SvgShape> shapes;

        // Create the definition of shared shapes.
        // shapes: the shapes, each with its style, in the order of the parts of the glyph.
        public SvgSharedShapes(IReadOnlyList<SvgShape> shapes)
        {
            this.shapes = shapes;
        }

        // The bounds of the shapes, in their own frame.
        public SvgBounds GetBounds()
        {
            SvgBounds bounds = new SvgBounds();
            foreach (SvgShape shape in shapes)
                bounds.Include(shape.GetBounds());
            return bounds;
        }

        // Write the definition: a single shape carries the id itself, and several are held in a group that carries it.
        // writer: the writer of the document.
        public void WriteDefinition(SvgWriter writer)
        {
            string id = writer.IdOfSharedShapes(this);
            if (shapes.Count == 1) {
                shapes[0].AppendDefinition(writer.Text, id);
                return;
            }
            writer.Text.Append("<g id=\"").Append(id).Append("\">\n");
            foreach (SvgShape shape in shapes)
                shape.AppendDefinition(writer.Text, null);
            writer.Text.Append("</g>\n");
        }
    }
}

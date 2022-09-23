using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemorySurface : IDrawSurface
    {
        internal List<IDrawOperation> Operations { get; } = new List<IDrawOperation>();

        internal List<MemDrawStyle> Styles { get; } = new List<MemDrawStyle>();

        internal List<MemDrawTextStyle> TextStyles { get; } = new List<MemDrawTextStyle>();

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            var style = new MemDrawStyle(fill, pen);
            Styles.Add(style);
            return style;
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            var textstyle = new MemDrawTextStyle(fontNames, style, size, fill, pen, fillCoverPen, textAnchor);
            TextStyles.Add(textstyle);
            return textstyle;
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            Operations.Add(new DrawCircle(center, radius, (MemDrawStyle)style));
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            Operations.Add(new DrawImage(image, pos, size, alpha));
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IDrawStyle style)
        {
            Operations.Add(new DrawPolygon(contour.ToList(), null, (MemDrawStyle)style));
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            Operations.Add(new DrawPolygon(contour.ToList(), holes.Select(h => h.ToList()).ToList(), (MemDrawStyle)style));
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            Operations.Add(new DrawPolyline(points.ToList(), (MemDrawStyle)style));
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            Operations.Add(new DrawText(point, text, (MemDrawTextStyle)style));
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            Operations.Add(new DrawTextPath(points.ToList(), text, (MemDrawTextStyle)style));
        }

        public MemorySurface ToScale(double scale, double penScale)
        {
            return new MemDrawScale(this, scale, penScale).ToMemorySurface();
        }

        public MemorySurface ToSimplified()
        {
            var target = new MemorySurface();
            target.Styles.AddRange(Styles);
            target.TextStyles.AddRange(TextStyles);
            target.Operations.AddRange(Operations.SelectMany(o => o.Simplify()));
            return target;
        }

        public void DrawTo(IDrawSurface surface)
        {
            var context = new MemDrawContext(this, surface);
            foreach(var op in Operations)
            {
                op.Draw(context);
            }
        }
    }
}

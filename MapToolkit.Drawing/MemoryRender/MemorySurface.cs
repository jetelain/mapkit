using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pmad.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing.MemoryRender
{
    internal class MemorySurface : IDrawSurface
    {
        internal List<IDrawOperation> Operations { get; } = new List<IDrawOperation>();

        internal List<MemDrawStyle> Styles { get; }

        internal List<MemDrawTextStyle> TextStyles { get; }

        internal List<MemDrawIcon> Icons { get; }

        public MemorySurface()
        {        
            Styles = new List<MemDrawStyle>();
            TextStyles = new List<MemDrawTextStyle>();
            Icons = new List<MemDrawIcon>();
        }

        private MemorySurface(MemorySurface other)
        {
            Styles = other.Styles;
            TextStyles = other.TextStyles;
            Icons = other.Icons;
        }

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

        internal List<IDrawOperation> DrawAttachedSurface(Action<IDrawSurface> draw)
        {
            var sub = new MemorySurface(this);
            draw(sub);
            return sub.Operations;
        }

        public IDrawIcon AllocateIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            var icon = new MemDrawIcon(size, DrawAttachedSurface(draw));
            Icons.Add(icon);
            return icon;
        }

        public void DrawCircle(Vector2D center, float radius, IDrawStyle style)
        {
            Operations.Add(new DrawCircle(center, radius, (MemDrawStyle)style));
        }

        public void DrawImage(Image image, Vector2D pos, Vector2D size, double alpha)
        {
            Operations.Add(new DrawImage(image, pos, size, alpha));
        }

        public void DrawPolygon(IEnumerable<Vector2D[]> paths, IDrawStyle style)
        {
            Operations.Add(new DrawPolygon(paths.ToList(),(MemDrawStyle)style));
        }

        public void DrawPolyline(IEnumerable<Vector2D> points, IDrawStyle style)
        {
            Operations.Add(new DrawPolyline(points.ToList(), (MemDrawStyle)style));
        }

        public void DrawText(Vector2D point, string text, IDrawTextStyle style)
        {
            Operations.Add(new DrawText(point, text, (MemDrawTextStyle)style));
        }

        public void DrawTextPath(IEnumerable<Vector2D> points, string text, IDrawTextStyle style)
        {
            Operations.Add(new DrawTextPath(points.ToList(), text, (MemDrawTextStyle)style));
        }

        public MemorySurface ToScale(double scale, double penScale, double lengthSquared = 4)
        {
            return new MemDrawScale(this, scale, penScale).ToMemorySurface(lengthSquared);
        }

        public MemorySurface ToSimplified(double lengthSquared = 4)
        {
            var target = new MemorySurface();
            target.Styles.AddRange(Styles);
            target.TextStyles.AddRange(TextStyles);
            target.Icons.AddRange(Icons);
            target.Operations.AddRange(Operations.SelectMany(o => o.Simplify(lengthSquared)));
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

        public void DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            Operations.Add(new DrawArc(center, radius, startAngle, sweepAngle, (MemDrawStyle)style));
        }

        public void DrawIcon(Vector2D center, IDrawIcon icon)
        {
            Operations.Add(new DrawIcon(center, (MemDrawIcon)icon));
        }

        public void DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, IDrawStyle style, float radius)
        {
            Operations.Add(new DrawRoundedRectangle(topLeft, bottomRight, (MemDrawStyle)style, radius));
        }
    }
}

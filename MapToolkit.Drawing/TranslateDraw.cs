using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    internal sealed class TranslateDraw : IDrawSurface
    {
        private readonly IDrawSurface drawSurface;
        private readonly double dx;
        private readonly double dy;

        public TranslateDraw(IDrawSurface drawSurface, double dx, double dy)
        {
            this.drawSurface = drawSurface;
            this.dx = dx;
            this.dy = dy;
        }

        public IDrawIcon AllocateIcon(Vector size, Action<IDrawSurface> draw)
        {
            return drawSurface.AllocateIcon(size, draw);
        }

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            return drawSurface.AllocateStyle(fill, pen);
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            return drawSurface.AllocateTextStyle(fontNames, style, size, fill, pen, fillCoverPen, textAnchor);
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            drawSurface.DrawCircle(Translate(center), radius, style);
        }

        private Vector Translate(Vector p)
        {
            return new Vector(dx + p.X, dy + p.Y);
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            drawSurface.DrawImage(image, Translate(pos), size, alpha);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IDrawStyle style)
        {
            drawSurface.DrawPolygon(contour.Select(Translate), style);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            drawSurface.DrawPolygon(contour.Select(Translate), holes.Select(h => h.Select(Translate)), style);
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            drawSurface.DrawPolygon(points.Select(Translate), style);
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            drawSurface.DrawText(Translate(point), text, style);
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            drawSurface.DrawTextPath(points.Select(Translate), text, style);
        }

        public void DrawArc(Vector center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            drawSurface.DrawArc(Translate(center), radius, startAngle, sweepAngle, style);
        }

        public void DrawIcon(Vector center, IDrawIcon icon)
        {
            drawSurface.DrawIcon(Translate(center), icon);
        }
    }
}

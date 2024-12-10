using System;
using System.Collections.Generic;
using System.Linq;
using Pmad.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing
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

        public IDrawIcon AllocateIcon(Vector2D size, Action<IDrawSurface> draw)
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

        public void DrawCircle(Vector2D center, float radius, IDrawStyle style)
        {
            drawSurface.DrawCircle(Translate(center), radius, style);
        }

        private Vector2D Translate(Vector2D p)
        {
            return new Vector2D(dx + p.X, dy + p.Y);
        }

        public void DrawImage(Image image, Vector2D pos, Vector2D size, double alpha)
        {
            drawSurface.DrawImage(image, Translate(pos), size, alpha);
        }

        public void DrawPolygon(IEnumerable<Vector2D[]> paths, IDrawStyle style)
        {
            drawSurface.DrawPolygon(paths.Select(h => h.Select(Translate).ToArray()), style);
        }

        public void DrawPolyline(IEnumerable<Vector2D> points, IDrawStyle style)
        {
            drawSurface.DrawPolyline(points.Select(Translate), style);
        }

        public void DrawText(Vector2D point, string text, IDrawTextStyle style)
        {
            drawSurface.DrawText(Translate(point), text, style);
        }

        public void DrawTextPath(IEnumerable<Vector2D> points, string text, IDrawTextStyle style)
        {
            drawSurface.DrawTextPath(points.Select(Translate), text, style);
        }

        public void DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            drawSurface.DrawArc(Translate(center), radius, startAngle, sweepAngle, style);
        }

        public void DrawIcon(Vector2D center, IDrawIcon icon)
        {
            drawSurface.DrawIcon(Translate(center), icon);
        }

        public void DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, IDrawStyle style, float radius)
        {
            drawSurface.DrawRoundedRectangle(Translate(topLeft), Translate(bottomRight), style, radius);
        }
    }
}

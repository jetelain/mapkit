using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.PdfRender
{
    internal sealed class ScaleAndShiftDraw : IDrawSurface
    {
        private readonly PdfSurface drawSurface;
        private readonly double scalePoints;
        private readonly double dx;
        private readonly double dy;

        public ScaleAndShiftDraw(PdfSurface drawSurface, double scalePoints, double dx, double dy)
        {
            this.drawSurface = drawSurface;
            this.scalePoints = scalePoints;
            this.dx = dx;
            this.dy = dy;
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
            drawSurface.DrawCircle(Transform(center), (float)(radius * scalePoints), style);
        }

        private Vector Transform(Vector center)
        {
            return new Vector(dx + center.X * scalePoints, dy + center.Y * scalePoints);
        }

        private Vector ScaleSize(Vector size)
        {
            return new Vector(size.X * scalePoints, size.Y * scalePoints);
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            drawSurface.DrawImage(image, Transform(pos), ScaleSize(size), alpha);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IDrawStyle style)
        {
            drawSurface.DrawPolygon(contour.Select(Transform), style);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            drawSurface.DrawPolygon(contour.Select(Transform), holes.Select(h => h.Select(Transform)), style);
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            drawSurface.DrawPolygon(points.Select(Transform), style);
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            throw new NotSupportedException();
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            throw new NotSupportedException();
        }
    }
}

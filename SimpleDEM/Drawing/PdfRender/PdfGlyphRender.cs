using System.Collections.Generic;
using System.Numerics;
using PdfSharpCore.Drawing;
using SixLabors.Fonts;

namespace SimpleDEM.Drawing.PdfRender
{
    /// <summary>
    /// Render SixLabors.Fonts to PdfSharpCore paths
    /// </summary>
    internal class PdfGlyphRender : IGlyphRenderer
    {
        private readonly List<XGraphicsPath> paths = new List<XGraphicsPath>();
        private readonly double dx;
        private readonly double dy;
        private XGraphicsPath currentPath;
        private Vector2 currentPoint;

        public PdfGlyphRender(double dx, double dy)
        {
            this.dx = dx;
            this.dy = dy;
            this.currentPath = new XGraphicsPath();
        }

        public List<XGraphicsPath> Paths => paths;

        public void EndText()
        {
        }

        public void BeginText(FontRectangle bounds)
        {
        }

        public bool BeginGlyph(FontRectangle bounds, GlyphRendererParameters paramaters)
        {
            currentPath = new XGraphicsPath();
            return true;
        }

        public void BeginFigure()
        {
            currentPath.StartFigure();
        }

        public void CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
        {
            currentPath.AddBezier(
                dx + currentPoint.X, dy + currentPoint.Y,
                dx + secondControlPoint.X, dy + secondControlPoint.Y,
                dx + thirdControlPoint.X, dy + thirdControlPoint.Y,
                dx + point.X, dy + point.Y);
            currentPoint = point;
        }

        public void EndGlyph()
        {
            paths.Add(this.currentPath.Clone());
        }

        public void EndFigure()
        {
            currentPath.CloseFigure();
        }

        public void LineTo(Vector2 point)
        {
            currentPath.AddLine(dx + currentPoint.X, dy + currentPoint.Y, dx + point.X, dy + point.Y);
            currentPoint = point;
        }

        public void MoveTo(Vector2 point)
        {
            currentPath.StartFigure();
            currentPoint = point;
        }

        public void QuadraticBezierTo(Vector2 secondControlPoint, Vector2 point)
        {
            var startPointVector = currentPoint;
            var controlPointVector = secondControlPoint;
            var endPointVector = point;
            var c1 = ((controlPointVector - startPointVector) * 2 / 3) + startPointVector;
            var c2 = ((controlPointVector - endPointVector) * 2 / 3) + endPointVector;
            currentPath.AddBezier(
                dx + currentPoint.X, dy + currentPoint.Y,
                dx + c1.X, dy + c1.Y,
                dx + c2.X, dy + c2.Y,
                dx + point.X, dy + point.Y);
            currentPoint = point;
        }

    }
}
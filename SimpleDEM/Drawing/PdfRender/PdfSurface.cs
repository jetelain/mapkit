using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace SimpleDEM.Drawing.PdfRender
{
    internal class PdfSurface : IDrawSurface
    {
        private readonly XGraphics graphics;
        private readonly double scaleLines;

        public PdfSurface(XGraphics graphics, double scaleLines = 1.0)
        {
            this.graphics = graphics;
            this.scaleLines = scaleLines;
        }

        public double Scale => scaleLines;

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen, string? name = null)
        {
            return new PdfStyle(fill, pen, scaleLines);
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, string? name = null)
        {
            return new PdfTextStyle(fill, pen, scaleLines, new XFont(fontNames[0], size * scaleLines, XFontStyle.Bold), fillCoverPen);
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            if (pstyle.Pen != null && pstyle.Brush != null)
            {
                graphics.DrawEllipse(pstyle.Pen, pstyle.Brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
            }
            else if (pstyle.Pen != null)
            {
                graphics.DrawEllipse(pstyle.Pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
            }
            else if (pstyle.Brush != null)
            {
                graphics.DrawEllipse(pstyle.Brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
            }
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            graphics.DrawImage(
                XImage.FromImageSource(ImageSharpImageSource<Rgba32>.FromImageSharpImage(image.CloneAs<Rgba32>(), PngFormat.Instance)),
                pos.X,
                pos.Y,
                size.X,
                size.Y);
        }

        public void DrawPolygon(IEnumerable<Vector> points, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            if (pstyle.VectorBrush != null)
            {
                DrawPolygon(points, Enumerable.Empty<IEnumerable<Vector>>(), pstyle);
                return;
            }
            var xpoints = points.Select(p => new XPoint(p.X, p.Y)).ToArray();
            if (pstyle.Pen != null && pstyle.Brush != null)
            {
                graphics.DrawPolygon(pstyle.Pen, pstyle.Brush, xpoints, XFillMode.Alternate);
            }
            else if (pstyle.Pen != null)
            {
                graphics.DrawPolygon(pstyle.Pen, xpoints);
            }
            else if (pstyle.Brush != null)
            {
                graphics.DrawPolygon(pstyle.Brush, xpoints, XFillMode.Alternate);
            }
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            var xpoints = points.Select(p => new XPoint(p.X, p.Y)).ToArray();
            graphics.DrawLines(pstyle.Pen, xpoints);
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            var pstyle = (PdfTextStyle)style;

            if (pstyle.SixFont != null)
            {
                var result = new PdfGlyphRender(point.X, point.Y);
                var textRender = new TextRenderer(result);
                textRender.RenderText(text, new TextOptions(pstyle.SixFont));
                if (pstyle.Pen != null)
                {
                    foreach (var path in result.Paths)
                    {
                        graphics.DrawPath(pstyle.Pen, path);
                    }
                }
                if (pstyle.Brush != null)
                {
                    foreach (var path in result.Paths)
                    {
                        graphics.DrawPath(pstyle.Brush, path);
                    }
                }
            }
            else
            {
                graphics.DrawString(text, pstyle.Font, pstyle.Brush, new XPoint(point.X, point.Y), XStringFormats.TopLeft);
            }
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            var first = points.First();
            var last = points.Last();
            var state = graphics.Save();

            graphics.RotateAtTransform(Math.Atan2(last.Y - first.Y, last.X - first.X) * 180.0 / Math.PI, new XPoint(first.X, first.Y));

            DrawText(first, text, style);

            graphics.Restore(state);
        }
        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            var pb = new XGraphicsPath();
            pb.AddLines(contour.Select(p => new XPoint(p.X, p.Y)).ToArray());
            pb.CloseFigure();
            foreach (var hole in holes)
            {
                pb.AddLines(hole.Select(p => new XPoint(p.X, p.Y)).ToArray());
                pb.CloseFigure();
            }
            if (pstyle.VectorBrush != null)
            {
                FillPolygon(pb, contour, pstyle.VectorBrush);
            }
            if (pstyle.Pen != null && pstyle.Brush != null)
            {
                graphics.DrawPath(pstyle.Pen, pstyle.Brush, pb);
            }
            else if (pstyle.Pen != null)
            {
                graphics.DrawPath(pstyle.Pen, pb);
            }
            else if (pstyle.Brush != null)
            {
                graphics.DrawPath(pstyle.Brush, pb);
            }
        }

        private void FillPolygon(XGraphicsPath path, IEnumerable<Vector> contour, VectorBrush vbrush)
        {
            var state = graphics.Save();
            graphics.IntersectClip(path);
            var minX = contour.Min(p => p.X);
            var minY = contour.Min(p => p.Y);
            var maxX = contour.Max(p => p.X);
            var maxY = contour.Max(p => p.Y);
            var w = vbrush.Width * scaleLines;
            var h = vbrush.Height * scaleLines;
            for (var x = minX; x < maxX; x += w)
            {
                for(var y = minY; y < maxY; y += h)
                {
                    vbrush.Draw(new ScaleAndShiftDraw(this, scaleLines, x, y));
                }
            }
            graphics.Restore(state);
        }

    }
}

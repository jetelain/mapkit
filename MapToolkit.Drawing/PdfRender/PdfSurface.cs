using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing.PdfRender
{
    internal class PdfSurface : IDrawSurface
    {
        private readonly XGraphics graphics;
        private readonly double pixelSize;

        public PdfSurface(XGraphics graphics, double pixelSize = 0.24)
        {
            this.graphics = graphics;
            this.pixelSize = pixelSize;
        }

        public IDrawIcon AllocateIcon(Vector size, Action<IDrawSurface> draw)
        {
            return new PdfIcon(size, draw);
        }

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            return new PdfStyle(fill, pen, pixelSize);
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            var xstyle = XFontStyle.Regular;
            switch (style)
            {
                case FontStyle.Bold:
                    xstyle = XFontStyle.Bold;
                    break;
                case FontStyle.Italic:
                    xstyle = XFontStyle.Italic;
                    break;
                case FontStyle.BoldItalic:
                    xstyle = XFontStyle.BoldItalic;
                    break;
            }

            return new PdfTextStyle(fill, pen, pixelSize, new XFont(fontNames[0], size * pixelSize, xstyle), fillCoverPen, textAnchor, style);
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            if (pstyle.Pen != null && pstyle.Brush != null)
            {
                graphics.DrawEllipse(pstyle.Pen, pstyle.Brush, (center.X - radius) * pixelSize, (center.Y - radius) * pixelSize, radius * 2 * pixelSize, radius * 2 * pixelSize);
            }
            else if (pstyle.Pen != null)
            {
                graphics.DrawEllipse(pstyle.Pen, (center.X - radius) * pixelSize, (center.Y - radius) * pixelSize, radius * 2 * pixelSize, radius * 2 * pixelSize);
            }
            else if (pstyle.Brush != null)
            {
                graphics.DrawEllipse(pstyle.Brush, (center.X - radius) * pixelSize, (center.Y - radius) * pixelSize, radius * 2 * pixelSize, radius * 2 * pixelSize);
            }
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            var rgba32 = image.CloneAs<Rgba32>();
            if (alpha != 1.0)
            {
                var matrix = new ColorMatrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, (float)alpha, 0f, 0f, 0f, 0f);
                rgba32.Mutate(i => i.Filter(matrix));
            }
            graphics.DrawImage(
                XImage.FromImageSource(ImageSharpImageSource<Rgba32>.FromImageSharpImage(rgba32, PngFormat.Instance)),
                pos.X * pixelSize,
                pos.Y * pixelSize,
                size.X * pixelSize,
                size.Y * pixelSize);
        }

        public void DrawPolygon(IEnumerable<Vector> points, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            if (pstyle.VectorBrush != null)
            {
                DrawPolygon(points, Enumerable.Empty<IEnumerable<Vector>>(), pstyle);
                return;
            }
            var xpoints = points.Select(p => new XPoint(p.X * pixelSize, p.Y * pixelSize)).ToArray();
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
            var xpoints = points.Select(p => new XPoint(p.X * pixelSize, p.Y * pixelSize)).ToArray();
            graphics.DrawLines(pstyle.Pen, xpoints);
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            var pstyle = (PdfTextStyle)style;

            if (pstyle.SixFont != null)
            {
                var result = new PdfGlyphRender(point.X * pixelSize, point.Y * pixelSize);
                var textRender = new TextRenderer(result);
                var to = new TextOptions(pstyle.SixFont);
                to.VerticalAlignment = pstyle.VerticalAlignment;
                to.HorizontalAlignment = pstyle.HorizontalAlignment;
                textRender.RenderText(text, to);

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
                graphics.DrawString(text, pstyle.Font, pstyle.Brush, new XPoint(point.X * pixelSize, point.Y * pixelSize), pstyle.GetXStringFormats());
            }
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            var first = points.First();
            var last = points.Last();
            var state = graphics.Save();

            graphics.RotateAtTransform(Math.Atan2(last.Y - first.Y, last.X - first.X) * 180.0 / Math.PI, new XPoint(first.X * pixelSize, first.Y * pixelSize));

            DrawText(first, text, style);

            graphics.Restore(state);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;
            var pb = new XGraphicsPath();
            pb.AddLines(contour.Select(p => new XPoint(p.X * pixelSize, p.Y * pixelSize)).ToArray());
            pb.CloseFigure();
            foreach (var hole in holes)
            {
                pb.AddLines(hole.Select(p => new XPoint(p.X * pixelSize, p.Y * pixelSize)).ToArray());
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

            var icon = (PdfIcon)vbrush.Icon;
            var w = icon.Size.X;
            var h = icon.Size.Y;
            for (var x = minX; x < maxX; x += w)
            {
                for(var y = minY; y < maxY; y += h)
                {
                    icon.Draw(new TranslateDraw(this, x, y));
                }
            }
            graphics.Restore(state);
        }

        public void DrawArc(Vector center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            var pstyle = (PdfStyle)style;

            graphics.DrawArc(pstyle.Pen, 
                (center.X - radius) * pixelSize,
                (center.Y - radius) * pixelSize, 
                radius * 2 * pixelSize, 
                radius * 2 * pixelSize, 
                startAngle, 
                sweepAngle);
        }

        public void DrawIcon(Vector center, IDrawIcon icon)
        {
            var picon = (PdfIcon)icon;
            var top = center - (picon.Size / 2);
            picon.Draw(new TranslateDraw(this, top.X, top.Y));
        }

        public void DrawRoundedRectangle(Vector topLeft, Vector bottomRight, IDrawStyle style, float radius)
        {
            var pstyle = (PdfStyle)style;

            graphics.DrawRoundedRectangle(pstyle.Pen, 
                pstyle.Brush, 
                topLeft.X * pixelSize, 
                topLeft.Y * pixelSize, 
                (bottomRight.X - topLeft.X) * pixelSize, 
                (bottomRight.Y - topLeft.Y) * pixelSize, 
                radius * pixelSize, 
                radius * pixelSize);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace SimpleDEM.Drawing.ImageRender
{
    internal class ImageSurface : IDrawSurface
    {
        private readonly IImageProcessingContext target;

        public ImageSurface(IImageProcessingContext target)
        {
            this.target = target;
        }

        public double Scale => 1;

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen, string? name = null)
        {
            return new ImageStyle(fill, pen);
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, string? name = null)
        {
            FontFamily fontFamily;
            var success = false;
            foreach(var font in fontNames)
            {
                if (SystemFonts.Collection.TryGet(font, out fontFamily))
                {
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                fontFamily = SystemFonts.Collection.Get("Arial");
            }
            return new ImageTextStyle(fill, pen, fontFamily.CreateFont((float)size, FontStyle.Bold), fillCoverPen);
        }

        public void DrawCircle(Vector center, float radius, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            var ipoints = new EllipsePolygon(new PointF((float)center.X, (float)center.Y), radius);
            if (istyle.Brush != null)
            {
                target.Fill(istyle.Brush, ipoints);
            }
            if (istyle.Pen != null)
            {
                target.Draw(istyle.Pen, ipoints);
            }
        }

        public void DrawImage(Image image, Vector pos, Vector size, double alpha)
        {
            var scaled = image;
            if (scaled.Width != size.X || scaled.Height != size.Y )
            {
                scaled = image.Clone(i => i.Resize((int)size.X,(int)size.Y));
            }
            target.DrawImage(scaled, new Point((int)pos.X, (int)pos.Y), (float)alpha);
        }

        public void DrawPolygon(IEnumerable<Vector> points, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            var ipoints = points.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
            if (istyle.Brush != null)
            {
                target.FillPolygon(istyle.Brush, ipoints);
            }
            if (istyle.Pen != null)
            {
                target.DrawPolygon(istyle.Pen, ipoints);
            }
        }

        public void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            var ipoints = points.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
            if (istyle.Pen != null)
            {
                target.DrawLines(istyle.Pen, ipoints);
            }
        }

        public void DrawText(Vector point, string text, IDrawTextStyle style)
        {
            var istyle = (ImageTextStyle)style;

            var to = new TextOptions(istyle.Font);
            to.Origin = new PointF((float)point.X, (float)point.Y);
            to.VerticalAlignment = VerticalAlignment.Center;

            if (istyle.FillCoverPen && istyle.Pen != null)
            {
                target.DrawText(to, text, null, istyle.Pen);
                target.DrawText(to, text, istyle.Brush, null);
            }
            else
            {
                target.DrawText(to, text, istyle.Brush, istyle.Pen);
            }
        }

        public void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style)
        {
            var first = points.First();
            var last = points.Last();
            var angle = Math.Atan2(last.Y - first.Y, last.X - first.X);

            target.SetDrawingTransform(Matrix3x2.CreateRotation((float)angle, new Vector2((float)first.X, (float)first.Y)));

            DrawText(first, text, style);

            target.SetDrawingTransform(Matrix3x2.Identity);
        }

        public void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style)
        {
            if (!holes.Any())
            {
                DrawPolygon(contour, style);
                return;
            }
            var istyle = (ImageStyle)style;
            var pb = new PathBuilder();
            pb.AddLines(contour.Select(p => new PointF((float)p.X, (float)p.Y))).CloseFigure();
            foreach(var hole in holes)
            {
                pb.AddLines(hole.Select(p => new PointF((float)p.X, (float)p.Y))).CloseFigure();
            }

            var path = pb.Build();
            if (istyle.Brush != null)
            {
                target.Fill(istyle.Brush, path);
            }
            if (istyle.Pen != null)
            {
                target.Draw(istyle.Pen, path);
            }
        }

        private static LinearLineSegment ToLinearLineSegment(IEnumerable<Vector> contour)
        {
            return new LinearLineSegment(contour.Select(p => new PointF((float)p.X, (float)p.Y)).Concat(contour.Take(1).Select(p => new PointF((float)p.X, (float)p.Y))).ToArray());
        }
    }
}

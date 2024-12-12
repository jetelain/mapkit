using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pmad.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Pmad.Drawing.ImageRender
{
    internal class ImageSurface : IDrawSurface
    {
        private readonly IImageProcessingContext target;

        public ImageSurface(IImageProcessingContext target)
        {
            this.target = target;
        }

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            return new ImageStyle(fill, pen);
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            return new ImageTextStyle(fill, pen, FontHelper.GetFont(fontNames, style, size), fillCoverPen, textAnchor);
        }

        public IDrawIcon AllocateIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            return new ImageIcon(size, draw);
        }

        public void DrawCircle(Vector2D center, float radius, IDrawStyle style)
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

        public void DrawImage(Image image, Vector2D pos, Vector2D size, double alpha)
        {
            var scaled = image;
            if (scaled.Width != size.X || scaled.Height != size.Y )
            {
                scaled = image.Clone(i => i.Resize((int)size.X,(int)size.Y));
            }
            target.DrawImage(scaled, new Point((int)pos.X, (int)pos.Y), (float)alpha);
        }

        public void DrawPolyline(IEnumerable<Vector2D> points, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            var ipoints = points.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
            if (istyle.Pen != null)
            {
                target.DrawLine(istyle.Pen, ipoints);
            }
        }

        public void DrawText(Vector2D point, string text, IDrawTextStyle style)
        {
            var istyle = (ImageTextStyle)style;

            var to = new RichTextOptions(istyle.Font);
            to.Origin = new PointF((float)point.X, (float)point.Y);
            to.VerticalAlignment = istyle.VerticalAlignment;
            to.HorizontalAlignment = istyle.HorizontalAlignment;

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

        public void DrawTextPath(IEnumerable<Vector2D> points, string text, IDrawTextStyle style)
        {
            var first = points.First();
            var last = points.Last();
            var angle = Math.Atan2(last.Y - first.Y, last.X - first.X);

            target.SetDrawingTransform(Matrix3x2.CreateRotation((float)angle, new Vector2((float)first.X, (float)first.Y)));

            DrawText(first, text, style);

            target.SetDrawingTransform(Matrix3x2.Identity);
        }

        public void DrawPolygon(IEnumerable<Vector2D[]> paths, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            var pb = new PathBuilder();
            foreach(var hole in paths)
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

        public void DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            var istyle = (ImageStyle)style;
            if (istyle.Pen != null)
            {
                var pb = new PathBuilder();
                pb.AddArc(new PointF((float)center.X, (float)center.Y), radius, radius, 0, startAngle, sweepAngle);
                target.Draw(istyle.Pen, pb.Build());
            }
        }

        public void DrawIcon(Vector2D center, IDrawIcon icon)
        {
            var iicon = (ImageIcon)icon;
            target.DrawImage(iicon.Image, new Point( (int)(center.X - (iicon.Image.Width / 2)), (int)(center.Y - (iicon.Image.Height / 2))), 1);
        }

        public void DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, IDrawStyle style, float radius)
        {
            var istyle = (ImageStyle)style;

            var path = new PathBuilder()
                .AddArc((float)topLeft.X + radius, (float)topLeft.Y + radius, radius, radius, 0, -90, -90)
                .AddArc((float)topLeft.X + radius, (float)bottomRight.Y - radius, radius, radius, 180, 0, -90)
                .AddArc((float)bottomRight.X - radius, (float)bottomRight.Y - radius, radius, radius, 0, 90, -90)
                .AddArc((float)bottomRight.X - radius, (float)topLeft.Y + radius, radius, radius, 180, 180, -90)
                .CloseFigure()
                .Build();

            if (istyle.Brush != null)
            {
                target.Fill(istyle.Brush, path);
            }
            if (istyle.Pen != null)
            {
                target.Draw(istyle.Pen, path);
            }
        }
    }
}

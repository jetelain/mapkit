using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
//using ClipperLib;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawPolyline : IDrawOperation
    {
        public DrawPolyline(List<Vector> points, MemDrawStyle style)
        {
            Points = points;
            Style = style;
            Min = new Vector(points.Min(v => v.X), points.Min(v => v.Y));
            Max = new Vector(points.Max(v => v.X), points.Max(v => v.Y));
        }

        public List<Vector> Points { get; }

        public MemDrawStyle Style { get; }
        public Vector Min { get; }
        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawPolyline(Points, context.MapStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            if ( Min.X >= context.ClipMin.X &&
                 Min.Y >= context.ClipMin.Y &&
                 Max.X <= context.ClipMax.X &&
                 Max.Y <= context.ClipMax.Y )
            {
                context.Target.DrawPolyline(Points.Select(context.Translate), context.MapStyle(Style));
            }
            else
            {
                //var clipper = new Clipper();
                //clipper.AddPath(Points.Select(p => new IntPoint(p.X * 100, p.Y * 100)).ToList(), PolyType.ptSubject, false);
                //clipper.AddPath(context.Clip, PolyType.ptClip, true);
                //var result = new PolyTree();
                //clipper.Execute(ClipType.ctIntersection, result);
                //foreach (var c in result.Childs)
                //{
                //    context.Target.DrawPolyline(c.Contour.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0))), context.MapStyle(Style));
                //}

                var subject = new Paths64(1);
                subject.Add(new Path64(Points.Select(p => new Point64(p.X * 100, p.Y * 100))));
                var result = Clipper2Lib.Clipper.RectClipLines(context.Clip, subject);
                foreach (var line in result)
                {
                    context.Target.DrawPolyline(line.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0))), context.MapStyle(Style));
                }
            }
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawPolyline(Points.Select(p => p * context.Scale).ToList(), context.MapStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared)
        {
            var line = LevelOfDetailHelper.SimplifyDistances(Points, lengthSquared);
            if (line.Count > 0)
            {
                yield return new DrawPolyline(line, Style);
            }
        }
    }
}
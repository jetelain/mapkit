using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
//using ClipperLib;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawPolygon : IDrawOperation
    {
        public DrawPolygon(List<Vector[]> paths, MemDrawStyle style)
        {
            Paths = paths;
            Style = style;
            Min = new Vector(paths.SelectMany(p => p).Min(v => v.X), paths.SelectMany(p => p).Min(v => v.Y));
            Max = new Vector(paths.SelectMany(p => p).Max(v => v.X), paths.SelectMany(p => p).Max(v => v.Y));
        }

        public List<Vector[]> Paths { get; }

        public MemDrawStyle Style { get; }
        public Vector Min { get; }
        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawPolygon(Paths, context.MapStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            if ((Min.X >= context.ClipMin.X &&
                 Min.Y >= context.ClipMin.Y &&
                 Max.X <= context.ClipMax.X &&
                 Max.Y <= context.ClipMax.Y) /*|| Style.Fill is VectorBrush*/)
            {
                context.Target.DrawPolygon(Paths.Select(h => h.Select(context.Translate).ToArray()), context.MapStyle(Style));
            }
            else
            {
                //var clipper = new Clipper();
                //clipper.AddPath(Contour.Select(p => new IntPoint(p.X * 100, p.Y * 100)).ToList(), PolyType.ptSubject, true);
                //if (Holes != null)
                //{
                //    foreach (var hole in Holes)
                //    {
                //        clipper.AddPath(hole.Select(p => new IntPoint(p.X * 100, p.Y * 100)).ToList(), PolyType.ptSubject, true);
                //    }
                //}
                //clipper.AddPath(context.Clip, PolyType.ptClip, true);
                //var result = new PolyTree();
                //clipper.Execute(ClipType.ctIntersection, result);
                //foreach (var c in result.Childs)
                //{
                //    context.Target.DrawPolygon(
                //        c.Contour.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0))),
                //        c.Childs.Select(c => c.Contour.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0)))),
                //        context.MapStyle(Style));
                //}

                var subject = new Paths64(Paths.Count);
                subject.AddRange(Paths.Select(h => new Path64(h.Select(p => new Point64(p.X * 100, p.Y * 100)))));
                var result = Clipper2Lib.Clipper.RectClip(context.Clip, subject);
                if (result.Count > 0)
                {
                    context.Target.DrawPolygon(
                        result.Select(c => c.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0))).ToArray()),
                        context.MapStyle(Style));
                }
            }
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawPolygon(Paths.Select(h => h.Select(p => p * context.Scale).ToArray()).ToList(), context.MapStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            var contours = LevelOfDetailHelper.SimplifyAnglesAndDistancesClosed(Paths, lengthSquared);
            if (contours.Count > 0)
            {
                yield return new DrawPolygon(contours, Style);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawPolygon : IDrawOperation
    {
        public DrawPolygon(List<Vector> contour, IEnumerable<IEnumerable<Vector>>? holes, MemDrawStyle style)
        {
            Contour = contour;
            Holes = holes;
            Style = style;
            Min = new Vector(contour.Min(v => v.X), contour.Min(v => v.Y));
            Max = new Vector(contour.Max(v => v.X), contour.Max(v => v.Y));
        }

        public List<Vector> Contour { get; }

        public IEnumerable<IEnumerable<Vector>>? Holes { get; }

        public MemDrawStyle Style { get; }
        public Vector Min { get; }
        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            if (Holes != null)
            {
                context.Target.DrawPolygon(Contour, Holes, context.MapStyle(Style));
            }
            else
            {
                context.Target.DrawPolygon(Contour, context.MapStyle(Style));
            }
        }

        public void DrawClipped(MemDrawClipped context)
        {
            if ((Min.X >= context.ClipMin.X &&
                 Min.Y >= context.ClipMin.Y &&
                 Max.X <= context.ClipMax.X &&
                 Max.Y <= context.ClipMax.Y) /*|| Style.Fill is VectorBrush*/)
            {
                if (Holes != null)
                {
                    context.Target.DrawPolygon(Contour.Select(context.Translate), Holes.Select(h => h.Select(context.Translate)), context.MapStyle(Style));
                }
                else
                {
                    context.Target.DrawPolygon(Contour.Select(context.Translate), context.MapStyle(Style));
                }
            }
            else
            {
                var clipper = new Clipper();
                clipper.AddPath(Contour.Select(p => new IntPoint(p.X * 100, p.Y * 100)).ToList(), PolyType.ptSubject, true);
                if ( Holes != null)
                {
                    foreach(var hole in Holes)
                    {
                        clipper.AddPath(hole.Select(p => new IntPoint(p.X * 100, p.Y * 100)).ToList(), PolyType.ptSubject, true);
                    }
                }
                clipper.AddPath(context.Clip, PolyType.ptClip, true);
                var result = new PolyTree();
                clipper.Execute(ClipType.ctIntersection, result);
                foreach (var c in result.Childs)
                {
                    context.Target.DrawPolygon(
                        c.Contour.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0))),
                        c.Childs.Select(c => c.Contour.Select(p => context.Translate(new Vector(p.X / 100.0, p.Y / 100.0)))),
                        context.MapStyle(Style));
                }
            }
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawPolygon(Contour.Select(c => c * context.Scale).ToList(), Holes?.Select(h => h.Select(p => p * context.Scale))?.ToList(), context.MapStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify()
        {
            var contour = DrawHelper.SimplifyClosed(Contour);
            if (contour != null)
            {
                yield return new DrawPolygon(contour, DrawHelper.SimplifyClosed(Holes), Style);
            }
        }
    }
}
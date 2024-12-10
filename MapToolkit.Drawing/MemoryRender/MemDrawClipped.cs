using Pmad.Geometry;
using Pmad.Geometry.Clipper2Lib;

namespace Pmad.Cartography.Drawing.MemoryRender
{
    internal class MemDrawClipped : MemDrawContext
    {
        internal MemDrawClipped(MemorySurface source, IDrawSurface target, Vector2D min, Vector2D max) : base(source, target)
        {
            ClipMin = min;
            ClipMax = max;
            //Clip = new List<IntPoint>() {
            //    new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMin.Y * 100.0 - 200.0),
            //    new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMax.Y * 100.0 + 200.0),
            //    new IntPoint(ClipMax.X * 100.0 + 200.0, ClipMax.Y * 100.0 + 200.0),
            //    new IntPoint(ClipMax.X * 100.0 + 200.0, ClipMin.Y * 100.0 - 200.0),
            //    new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMin.Y * 100.0 - 200.0)
            //};
            Clip = new Rect64(
                    (long)(ClipMin.X * 100) - 200, (long)(ClipMin.Y * 100) - 200,
                    (long)(ClipMax.X * 100) + 200, (long)(ClipMax.Y * 100) + 200);
        }

        public Vector2D ClipMax { get; }

        public Rect64 Clip { get; }

        public Vector2D ClipMin { get; }

        internal Vector2D Translate(Vector2D p)
        {
            return p - ClipMin;
        }

        internal void Draw()
        {
            foreach(var op in Source.Operations)
            {
                if (Overlaps(op))
                {
                    op.DrawClipped(this);
                }
            }
        }

        internal bool Overlaps(IDrawOperation op)
        {
            return op.Min.X <= ClipMax.X &&
                    op.Min.Y <= ClipMax.Y &&
                    op.Max.X >= ClipMin.X &&
                    op.Max.Y >= ClipMin.Y;
        }

    }
}
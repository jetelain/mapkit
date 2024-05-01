using System.Collections.Generic;
using ClipperLib;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class MemDrawClipped : MemDrawContext
    {
        internal MemDrawClipped(MemorySurface source, IDrawSurface target, Vector min, Vector max) : base(source, target)
        {
            ClipMin = min;
            ClipMax = max;
            Clip = new List<IntPoint>() {
                new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMin.Y * 100.0 - 200.0),
                new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMax.Y * 100.0 + 200.0),
                new IntPoint(ClipMax.X * 100.0 + 200.0, ClipMax.Y * 100.0 + 200.0),
                new IntPoint(ClipMax.X * 100.0 + 200.0, ClipMin.Y * 100.0 - 200.0),
                new IntPoint(ClipMin.X * 100.0 - 200.0, ClipMin.Y * 100.0 - 200.0)
            };
        }

        public Vector ClipMax { get; }

        public List<IntPoint> Clip { get; }

        public Vector ClipMin { get; }

        internal Vector Translate(Vector p)
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
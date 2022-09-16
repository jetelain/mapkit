using System;

namespace SimpleDEM.Drawing
{
    public sealed class VectorBrush : IBrush
    {
        public VectorBrush(float width, float height, Action<IDrawSurface> draw)
        {
            Width = width;
            Height = height;
            Draw = draw;
        }
        public float Width { get; }
        public float Height { get; }
        public Action<IDrawSurface> Draw { get; }
    }
}

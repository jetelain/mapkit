using System;

namespace Pmad.Cartography.Drawing
{
    public sealed class VectorBrush : IBrush
    {
        public VectorBrush(IDrawIcon icon)
        {
            Icon = icon;
        }

        public VectorBrush(IDrawSurface target, float width, float height, Action<IDrawSurface> draw)
        {
            Icon = target.AllocateIcon(new Vector(width, height), draw);
            //Width = width;
            //Height = height;
            //Draw = draw;
        }

        public IDrawIcon Icon { get; }
        //public float Width { get; }
        //public float Height { get; }
        //public Action<IDrawSurface> Draw { get; }

        public bool Equals(IBrush? other)
        {
            return other == this;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as VectorBrush);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

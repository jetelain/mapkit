using System;
using Pmad.Geometry;

namespace Pmad.Drawing.SvgRender
{
    internal class SvgIcon : IDrawIcon
    {
        public SvgIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            Size = size;
            Draw = draw;
        }

        public Vector2D Size { get; }
        public Action<IDrawSurface> Draw { get; }
    }
}
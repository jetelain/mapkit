using System;

namespace MapToolkit.Drawing.SvgRender
{
    internal class SvgIcon : IDrawIcon
    {
        public SvgIcon(Vector size, Action<IDrawSurface> draw)
        {
            Size = size;
            Draw = draw;
        }

        public Vector Size { get; }
        public Action<IDrawSurface> Draw { get; }
    }
}
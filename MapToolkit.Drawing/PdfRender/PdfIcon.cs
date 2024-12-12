using System;
using Pmad.Geometry;

namespace Pmad.Drawing.PdfRender
{
    internal class PdfIcon : IDrawIcon
    {
        public PdfIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            Size = size;
            Draw = draw;
        }

        public Vector2D Size { get; }
        public Action<IDrawSurface> Draw { get; }
    }
}
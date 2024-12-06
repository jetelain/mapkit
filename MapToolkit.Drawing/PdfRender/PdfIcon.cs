using System;

namespace Pmad.Cartography.Drawing.PdfRender
{
    internal class PdfIcon : IDrawIcon
    {
        public PdfIcon(Vector size, Action<IDrawSurface> draw)
        {
            Size = size;
            Draw = draw;
        }

        public Vector Size { get; }
        public Action<IDrawSurface> Draw { get; }
    }
}
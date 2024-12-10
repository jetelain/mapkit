using System;
using Pmad.Geometry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Pmad.Cartography.Drawing.ImageRender
{
    internal class ImageIcon : IDrawIcon
    {
        public ImageIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            Image = new Image<Rgba32>((int)size.X, (int)size.Y);
            Image.Mutate(p => draw(new ImageSurface(p)));
        }

        public Image<Rgba32> Image { get; }
    }
}
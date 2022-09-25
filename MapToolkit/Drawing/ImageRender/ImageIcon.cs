using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing.ImageRender
{
    internal class ImageIcon : IDrawIcon
    {
        public ImageIcon(Vector size, Action<IDrawSurface> draw)
        {
            Image = new Image<Rgba32>((int)size.X, (int)size.Y);
            Image.Mutate(p => draw(new ImageSurface(p)));
        }

        public Image<Rgba32> Image { get; }
    }
}
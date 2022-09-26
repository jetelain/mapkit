using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing.ImageRender
{
    internal class ImageStyle : IDrawStyle
    {
        public ImageStyle(IBrush? fill, Pen? pen)
        {
            Brush = ToBrush(fill);
            Pen = ToPen(pen);
        }

        private SixLabors.ImageSharp.Drawing.Processing.IPen? ToPen(Pen? pen)
        {
            if (pen != null)
            {
                if (pen.Pattern != null)
                {
                    return new SixLabors.ImageSharp.Drawing.Processing.Pen(ToBrush(pen.Brush), (float)pen.Width, pen.Pattern.Select(v => (float)(v/pen.Width)).ToArray());
                }
                return new SixLabors.ImageSharp.Drawing.Processing.Pen(ToBrush(pen.Brush), (float)pen.Width);
            }
            return null;
        }

        private SixLabors.ImageSharp.Drawing.Processing.IBrush? ToBrush(IBrush? fill)
        {
            switch (fill)
            {
                case SolidColorBrush solid:
                    return new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(solid.Color);
                case VectorBrush vector:
                    return new SixLabors.ImageSharp.Drawing.Processing.ImageBrush(ToImage(vector));
            }
            return null;
        }

        private Image ToImage(VectorBrush vector)
        {
            return ((ImageIcon)vector.Icon).Image;
        }

        public SixLabors.ImageSharp.Drawing.Processing.IBrush? Brush { get; set; }
        public SixLabors.ImageSharp.Drawing.Processing.IPen? Pen { get; set; }
    }
}

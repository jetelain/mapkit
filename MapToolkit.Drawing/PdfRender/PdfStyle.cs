using System;
using System.Linq;
using PdfSharpCore.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.Cartography.Drawing.PdfRender
{
    internal class PdfStyle : IDrawStyle
    {
        public PdfStyle(IBrush? fill, Pen? pen, double scaleLines)
        {
            Brush = ToBrush(fill);
            VectorBrush = fill as VectorBrush;
            Pen = ToPen(pen, scaleLines);
        }

        private static XPen? ToPen(Pen? pen, double scaleLines)
        {
            if (pen != null)
            {
                var xpen = new XPen(GetColor(pen.Brush), pen.Width * scaleLines);
                if (pen.Pattern != null)
                {
                    xpen.DashPattern = pen.Pattern.Select(v => v / pen.Width).ToArray();
                }
                return xpen;
            }
            return null;
        }

        private static XBrush? ToBrush(IBrush? fill)
        {
            switch (fill)
            {
                case SolidColorBrush solid:
                    return new XSolidBrush(GetColor(solid));
                    //case VectorBrush vector:
                    //    return new SixLabors.ImageSharp.Drawing.Processing.ImageBrush(ToImage(vector));
                    // TODO: find a workaround !
            }
            return null;
        }

        private static XColor GetColor(SolidColorBrush solid)
        {
            var c = solid.Color.ToPixel<Argb32>();
            var xc = XColor.FromArgb(c.A, c.R, c.G, c.B);
            return xc;
        }

        public XBrush? Brush { get; }

        public XPen? Pen { get; }

        public VectorBrush? VectorBrush { get; }
    }
}
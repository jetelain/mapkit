using System;
using System.Linq;
using PdfSharpCore.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.Drawing.PdfRender
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
                var brush = ToBrush(pen.Brush);
                XPen xpen;
                if (brush is XSolidBrush sb)
                {
                    xpen = new XPen(sb.Color, pen.Width * scaleLines);
                }
                else
                {
                    xpen = new XPen(ToBrush(pen.Brush), pen.Width * scaleLines);
                }
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
                    var c = solid.Color.ToPixel<Argb32>();
                    return new XSolidBrush(XColor.FromArgb(c.A, c.R, c.G, c.B));
                //case VectorBrush vector:
                //    return new SixLabors.ImageSharp.Drawing.Processing.ImageBrush(ToImage(vector));
                // TODO: find a workaround !
            }
            return null;
        }

        public XBrush? Brush { get; }

        public XPen? Pen { get; }

        public VectorBrush? VectorBrush { get; }
    }
}
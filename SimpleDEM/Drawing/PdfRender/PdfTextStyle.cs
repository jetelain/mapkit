using PdfSharpCore.Drawing;
using SixLabors.Fonts;

namespace SimpleDEM.Drawing.PdfRender
{
    internal class PdfTextStyle : PdfStyle, IDrawTextStyle
    {
        public PdfTextStyle(IBrush? fill, Pen? pen, double scaleLines, XFont xFont, bool fillCoverPen) 
            : base(fill, pen, scaleLines)
        {
            Font = xFont;

            if (Pen != null || fillCoverPen)
            {
                SixFont = SystemFonts.Collection.Get(xFont.Name).CreateFont((float)xFont.Size, FontStyle.Bold);
            }
        }

        public XFont Font { get; }

        public Font? SixFont { get; }
    }
}
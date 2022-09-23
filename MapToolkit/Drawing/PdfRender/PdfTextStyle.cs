using PdfSharpCore.Drawing;
using SixLabors.Fonts;

namespace MapToolkit.Drawing.PdfRender
{
    internal class PdfTextStyle : PdfStyle, IDrawTextStyle
    {
        public PdfTextStyle(IBrush? fill, Pen? pen, double scaleLines, XFont xFont, bool fillCoverPen, TextAnchor textAnchor, FontStyle style) 
            : base(fill, pen, scaleLines)
        {
            Font = xFont;
            TextAnchor = textAnchor;
            if (Pen != null || fillCoverPen)
            {
                SixFont = SystemFonts.Collection.Get(xFont.Name).CreateFont((float)xFont.Size, style);
            }
            VerticalAlignment = FontHelper.GetVerticalAlignment(textAnchor);
            HorizontalAlignment = FontHelper.GetHorizontalAlignment(textAnchor);
        }

        public XFont Font { get; }

        public TextAnchor TextAnchor { get; }

        public Font? SixFont { get; }
        public VerticalAlignment VerticalAlignment { get; }
        public HorizontalAlignment HorizontalAlignment { get; }
    }
}
using SixLabors.Fonts;

namespace Pmad.Drawing.ImageRender
{
    internal class ImageTextStyle : ImageStyle, IDrawTextStyle
    {
        public ImageTextStyle(IBrush? fill, Pen? pen, Font font, bool fillCoverPen, TextAnchor textAnchor)
            : base(fill, pen)
        {
            Font = font;
            FillCoverPen = fillCoverPen;
            VerticalAlignment = FontHelper.GetVerticalAlignment(textAnchor);
            HorizontalAlignment = FontHelper.GetHorizontalAlignment(textAnchor);
        }

        public Font Font { get; }
        public bool FillCoverPen { get; }
        public VerticalAlignment VerticalAlignment { get; }
        public HorizontalAlignment HorizontalAlignment { get; }
    }
}
using SixLabors.Fonts;

namespace SimpleDEM.Drawing.ImageRender
{
    internal class ImageTextStyle : ImageStyle, IDrawTextStyle
    {
        public ImageTextStyle(IBrush? fill, Pen? pen, Font font, bool fillCoverPen) 
            : base(fill, pen)
        {
            Font = font;
            FillCoverPen = fillCoverPen;
        }

        public Font Font { get; }
        public bool FillCoverPen { get; }
    }
}
using SixLabors.Fonts;

namespace SimpleDEM.Drawing.ImageRender
{
    internal class ImageTextStyle : ImageStyle, IDrawTextStyle
    {
        public ImageTextStyle(IBrush? fill, Pen? pen, Font font, bool fillCoverPen, TextAnchor textAnchor)
            : base(fill, pen)
        {
            Font = font;
            FillCoverPen = fillCoverPen;

            switch (textAnchor)
            {
                case TextAnchor.CenterLeft:
                    this.VerticalAlignment = VerticalAlignment.Center;
                    this.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case TextAnchor.CenterRight:
                    this.VerticalAlignment = VerticalAlignment.Center;
                    this.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case TextAnchor.TopCenter:
                    this.VerticalAlignment = VerticalAlignment.Bottom;
                    this.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case TextAnchor.BottomCenter:
                    this.VerticalAlignment = VerticalAlignment.Top;
                    this.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
            }
        }

        public Font Font { get; }
        public bool FillCoverPen { get; }
        public VerticalAlignment VerticalAlignment { get; }
        public HorizontalAlignment HorizontalAlignment { get; }
    }
}
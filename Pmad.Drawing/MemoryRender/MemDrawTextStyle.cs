using SixLabors.Fonts;

namespace Pmad.Drawing.MemoryRender
{
    internal class MemDrawTextStyle : IDrawTextStyle
    {
        public MemDrawTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen, TextAnchor textAnchor)
        {
            FontNames = fontNames;
            Style = style;
            Size = size;
            Fill = fill;
            Pen = pen;
            FillCoverPen = fillCoverPen;
            TextAnchor = textAnchor;

            VerticalAlignment = FontHelper.GetVerticalAlignment(textAnchor);
            HorizontalAlignment = FontHelper.GetHorizontalAlignment(textAnchor);
            Font = FontHelper.GetFont(fontNames, style, size);
        }

        public string[] FontNames { get; }
        public FontStyle Style { get; }
        public double Size { get; }
        public IBrush? Fill { get; }
        public Pen? Pen { get; }
        public bool FillCoverPen { get; }
        public TextAnchor TextAnchor { get; }
        public VerticalAlignment VerticalAlignment { get; }
        public HorizontalAlignment HorizontalAlignment { get; }
        public Font Font { get; }
    }
}
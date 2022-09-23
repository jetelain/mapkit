using SixLabors.Fonts;

namespace MapToolkit.Drawing
{
    internal class FontHelper
    {
        internal static FontFamily GetFontFamily(string[] fontNames)
        {
            foreach (var font in fontNames)
            {
                if (SystemFonts.Collection.TryGet(font, out var fontFamily))
                {
                    return fontFamily;
                }
            }
            return SystemFonts.Collection.Get("Arial");
        }

        internal static Font GetFont(string[] fontNames, FontStyle style, double size)
        {
            return GetFontFamily(fontNames).CreateFont((float)size, style);
        }

        internal static VerticalAlignment GetVerticalAlignment(TextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case TextAnchor.CenterLeft:
                    return VerticalAlignment.Center;
                case TextAnchor.CenterRight:
                    return VerticalAlignment.Center;
                case TextAnchor.TopCenter:
                    return VerticalAlignment.Bottom;
                case TextAnchor.BottomCenter:
                    return VerticalAlignment.Top;
            }
            return VerticalAlignment.Top;
        }

        internal static HorizontalAlignment GetHorizontalAlignment(TextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case TextAnchor.CenterLeft:
                    return HorizontalAlignment.Left;
                case TextAnchor.CenterRight:
                    return HorizontalAlignment.Right;
                case TextAnchor.TopCenter:
                    return HorizontalAlignment.Center;
                case TextAnchor.BottomCenter:
                    return HorizontalAlignment.Center;
            }
            return HorizontalAlignment.Left;
        }
    }
}

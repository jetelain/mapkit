using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public sealed class SolidColorBrush : IBrush
    {
        public SolidColorBrush(Color color)
        {
            Color = color;
        }

        public Color Color { get; }
    }
}

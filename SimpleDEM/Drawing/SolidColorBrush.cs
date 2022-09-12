using SixLabors.ImageSharp;

namespace SimpleDEM.Drawing
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

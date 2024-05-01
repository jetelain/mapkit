using System;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public sealed class SolidColorBrush : IBrush, IEquatable<SolidColorBrush>
    {
        public SolidColorBrush(Color color)
        {
            Color = color;
        }

        public Color Color { get; }

        public bool Equals(SolidColorBrush? other)
        {
            return other != null && Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SolidColorBrush);
        }

        public bool Equals(IBrush? other)
        {
            return Equals(other as SolidColorBrush);
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }
    }
}

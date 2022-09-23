using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public class Pen
    {
        public Pen(SolidColorBrush brush, double width = 1, IEnumerable<double>? pattern = null)
        {
            Brush = brush;
            Width = width;
            Pattern = pattern;
        }

        public Pen(Color color, double width = 1, IEnumerable<double>? pattern = null)
        {
            Brush = new SolidColorBrush(color);
            Width = width;
            Pattern = pattern;
        }

        public SolidColorBrush Brush { get; }

        public double Width { get; }

        public IEnumerable<double>? Pattern { get; }

    }
}

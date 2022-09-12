using System.Collections.Generic;

namespace SimpleDEM.Drawing
{
    public class Pen
    {
        public Pen(IBrush brush, double width = 1, IEnumerable<double>? pattern = null)
        {
            Brush = brush;
            Width = width;
            Pattern = pattern;
        }

        public IBrush Brush { get; }

        public double Width { get; }

        public IEnumerable<double>? Pattern { get; }

    }
}

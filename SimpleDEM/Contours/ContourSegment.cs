using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.Contours
{
    internal class ContourSegment
    {
        public ContourSegment(Coordinates point1, Coordinates point2, double level)
        {
            Point1 = point1;
            Point2 = point2;
            Level = level;
        }

        public Coordinates Point1 { get; }
        public Coordinates Point2 { get; }
        public double Level { get; }
    }
}

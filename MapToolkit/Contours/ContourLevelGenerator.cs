using System;
using System.Collections.Generic;

namespace SimpleDEM.Contours
{
    public class ContourLevelGenerator : IContourLevelGenerator
    {
        private readonly double step;

        public ContourLevelGenerator (double step = 10)
        {
            this.step = step;
        }

        public IEnumerable<double> Levels(double min, double max)
        {
            var start = Math.Ceiling(min / step) * step;
            var end = Math.Ceiling(max / step) * step;
            if (start != end)
            {
                for (var level = start; level <= end; level += step)
                {
                    yield return level;
                }
            }
        }
    }
}

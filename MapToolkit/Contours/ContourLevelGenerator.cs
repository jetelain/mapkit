using System;
using System.Collections.Generic;

namespace Pmad.Cartography.Contours
{
    public class ContourLevelGenerator : IContourLevelGenerator
    {
        private readonly double step;
        private readonly double strictMin;

        public ContourLevelGenerator (double strictMin = double.MinValue, double step = 10)
        {
            this.strictMin = strictMin;
            this.step = step;
        }

        public IEnumerable<double> Levels(double min, double max)
        {
            var start = Math.Max(strictMin, Math.Ceiling(min / step) * step);
            var end = Math.Max(strictMin, Math.Ceiling(max / step) * step);
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

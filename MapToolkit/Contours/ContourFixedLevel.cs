using System.Collections.Generic;
using System.Linq;

namespace SimpleDEM.Contours
{
    public class ContourFixedLevel : IContourLevelGenerator
    {
        private readonly List<double> levels;

        public ContourFixedLevel(List<double> levels)
        {
            this.levels = levels;
        }

        public IEnumerable<double> Levels(double min, double max)
        {
            return levels.Where(l => l >= min && l <= max);
        }
    }
}

using System.Collections.Generic;

namespace SimpleDEM.Contours
{
    public interface IContourLevelGenerator
    {
        IEnumerable<double> Levels(double min, double max);
    }
}
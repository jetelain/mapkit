using System.Collections.Generic;

namespace MapToolkit.Contours
{
    public interface IContourLevelGenerator
    {
        IEnumerable<double> Levels(double min, double max);
    }
}
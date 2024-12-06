using System.Collections.Generic;

namespace Pmad.Cartography.Contours
{
    public interface IContourLevelGenerator
    {
        IEnumerable<double> Levels(double min, double max);
    }
}
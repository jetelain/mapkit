using System.Collections.Generic;

namespace Pmad.Cartography
{
    public interface IInterpolation
    {
        double Interpolate(double f00, double f10, double f01, double f11, double x, double y);

        double Interpolate(Coordinates coordinates, List<DemDataPoint> points);
    }
}

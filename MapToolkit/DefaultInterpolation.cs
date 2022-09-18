using System;
using System.Collections.Generic;

namespace MapToolkit
{
    public sealed class DefaultInterpolation : IInterpolation
    {
        public static readonly DefaultInterpolation Instance = new DefaultInterpolation();

        public double Interpolate(double f00, double f10, double f01, double f11, double x, double y)
        {
#if DEBUG
            if ( x < 0 || x > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < 0 || y > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
#endif
            // https://en.wikipedia.org/wiki/Bilinear_interpolation
            // with (x,y) in unit square
            return f00 * (1 - x) * (1 - y)
                 + f10 * x * (1 - y)
                 + f01 * (1 - x) * y
                 + f11 * x * y;
        }

        public double Interpolate(Coordinates coordinates, List<DemDataPoint> points)
        {
            if (points.Count == 0)
            {
                return double.NaN;
            }
            if (points.Count == 1)
            {
                return points[0].Elevation;
            }
            var elevationWeighted = 0d;
            var totalDistances = 0d;
            foreach(var point in points)
            {
                var distance = point.Coordinates.Distance(coordinates);
                if (distance < 0.000005) // less than 1m at equator
                {
                    // Really close of a point
                    // Also avoid division by zero
                    return point.Elevation;
                }
                elevationWeighted += point.Elevation / distance;
                totalDistances += 1 / distance;
            }
            return elevationWeighted / totalDistances;
        }
    }
}

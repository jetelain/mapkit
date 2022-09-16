using System.Collections.Generic;

namespace MapToolkit
{
    /// <summary>
    /// Triangle Meshing using basic triangles from NW to SE
    /// <pre>
    ///   N
    ///   ^. .. .. 
    ///   | \| \|
    /// 1 +--+--+-..
    ///   |\ |\ |\
    ///   | \| \| .
    /// 0 +--+--+--> E
    ///   0  1  2
    /// </pre>
    /// </summary>
    public sealed class TriangleNWToSEInterpolation : IInterpolation
    {
        public static readonly TriangleNWToSEInterpolation Instance = new TriangleNWToSEInterpolation();

        public double Interpolate(double f00, double f10, double f01, double f11, double x, double y)
        {
            if (x <= 1 - y)
            {
                return f00 + (f01 - f00) * y + (f10 - f00) * x;
            }
            var d1011 = f10 - f11;
            var d0111 = f01 - f11;
            return f10 + d0111 - d0111 * x - d1011 * y;
        }

        public double Interpolate(Coordinates coordinates, List<DemDataPoint> points)
        {
            return DefaultInterpolation.Instance.Interpolate(coordinates, points);
        }
    }
}

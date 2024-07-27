using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using GeoJSON.Text.Geometry;

namespace MapToolkit
{
    public static class GeometryExtensions
    {
        public static bool IsCounterClockWise<T>(this List<T> points) where T : IPosition
        {
            //var north = points.IndexOf(points.OrderByDescending(p => p.Latitude).First());
            //var east = points.IndexOf(points.OrderByDescending(p => p.Longitude).First());
            //var south = points.IndexOf(points.OrderBy(p => p.Latitude).First());
            //var west = points.IndexOf(points.OrderBy(p => p.Longitude).First());

            //var epos = (east - north) % points.Count;
            //var spos = (south - north) % points.Count;
            //var wpos = (west - north) % points.Count;

            //return epos >= spos && spos >= wpos;
            return GetSignedArea(points) > 0;
        }

        private static double GetSignedArea<T>(this IList<T> points) where T : IPosition
        {
            if (points.Count < 3)
                return 0;

            int i;
            double area = 0;

            for (i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;

                var vi = points[i];
                var vj = points[j];

                area += vi.Longitude * vj.Latitude;
                area -= vi.Latitude * vj.Longitude;
            }
            area /= 2.0f;
            return area;
        }

        public static bool IsPointInside(this IReadOnlyList<IPosition> path, IPosition pt)
        {
            return PointInPolygon(path, pt) == 1;
        }

        public static bool IsPointInsideOrOnBoundary(this IReadOnlyList<IPosition> path, IPosition pt)
        {
            return PointInPolygon(path, pt) != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pt"></param>
        /// <returns>0 if false, +1 if true, -1 if pt ON polygon boundary</returns>
        public static int PointInPolygon(IReadOnlyList<IPosition> path, IPosition pt)
        {
            //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
            //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
            int result = 0, cnt = path.Count;
            if (cnt < 3) return 0;
            var ip = path[0];
            for (int i = 1; i <= cnt; ++i)
            {
                var ipNext = (i == cnt ? path[0] : path[i]);
                if (ipNext.Latitude == pt.Latitude)
                {
                    if ((ipNext.Longitude == pt.Longitude) || (ip.Latitude == pt.Latitude && ((ipNext.Longitude > pt.Longitude) == (ip.Longitude < pt.Longitude))))
                        return -1;
                }
                if ((ip.Latitude < pt.Latitude) != (ipNext.Latitude < pt.Latitude))
                {
                    if (ip.Longitude >= pt.Longitude)
                    {
                        if (ipNext.Longitude > pt.Longitude) result = 1 - result;
                        else
                        {
                            double d = (double)(ip.Longitude - pt.Longitude) * (ipNext.Latitude - pt.Latitude) - (double)(ipNext.Longitude - pt.Longitude) * (ip.Latitude - pt.Latitude);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Latitude > ip.Latitude)) result = 1 - result;
                        }
                    }
                    else
                    {
                        if (ipNext.Longitude > pt.Longitude)
                        {
                            double d = (double)(ip.Longitude - pt.Longitude) * (ipNext.Latitude - pt.Latitude) - (double)(ipNext.Longitude - pt.Longitude) * (ip.Latitude - pt.Latitude);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Latitude > ip.Latitude)) result = 1 - result;
                        }
                    }
                }
                ip = ipNext;
            }
            return result;
        }

        public static string ToGeoString(this IEnumerable<IPosition> points)
        {
            return $"({string.Join(", ", points.Select(p => FormattableString.Invariant($"{p.Longitude} {p.Latitude}")))})";
        }
    }
}

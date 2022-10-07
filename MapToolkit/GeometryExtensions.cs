using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using GeoJSON.Text.Geometry;

namespace MapToolkit
{
    public static class GeometryExtensions
    {
        public static MultiLineString Crop(this MultiLineString mls, Coordinates min, Coordinates max)
        {
            var clipper = new Clipper();
            foreach (var line in mls.Coordinates)
            {
                clipper.AddPath(line.Coordinates.Select(p => p.ToIntPoint()).ToList(), PolyType.ptSubject, false);
            }
            AddCropClip(min, max, clipper);
            var result = new PolyTree();
            clipper.Execute(ClipType.ctIntersection, result);
            return new MultiLineString(result.Childs.Select(c => c.ToLineString()).ToList());
        }

        public static MultiPolygon Crop(this MultiPolygon mls, Coordinates min, Coordinates max)
        {
            var clipper = new Clipper();
            foreach (var poly in mls.Coordinates)
            {
                foreach (var line in poly.Coordinates)
                {
                    clipper.AddPath(line.Coordinates.Select(p => p.ToIntPoint()).ToList(), PolyType.ptSubject, true);
                }
            }
            AddCropClip(min, max, clipper);
            var result = new PolyTree();
            clipper.Execute(ClipType.ctIntersection, result);

            return new MultiPolygon(result.ToPolygons());
        }

        internal static bool IsCounterClockWise<T>(this List<T> points) where T : IPosition
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

        private static double GetSignedArea<T>(this List<T> points) where T : IPosition
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

        private static void AddCropClip(Coordinates min, Coordinates max, Clipper clipper)
        {
            var minInt = min.ToIntPoint();
            var maxInt = max.ToIntPoint();
            clipper.AddPath(new List<IntPoint>() {
                minInt,
                new IntPoint(minInt.X, maxInt.Y),
                maxInt,
                new IntPoint(maxInt.X, minInt.Y),
                minInt
            }, PolyType.ptClip, true);
        }

        internal static LineString ToLineStringClosed(this PolyNode c, int rounding = -1)
        {
            var points = c.Contour.Select(c => new Coordinates(c, rounding));
            return new LineString(points.Concat(points.Take(1)).ToList());
        }

        internal static LineString ToLineString(this PolyNode c, int rounding = -1)
        {
            return new LineString(c.Contour.Select(c => new Coordinates(c, rounding)).ToList());
        }

        internal static IntPoint ToIntPoint(this IPosition pos, double scaleForClipper = Coordinates.ScaleForClipper)
        {
            return new IntPoint(pos.Longitude * scaleForClipper, pos.Latitude * scaleForClipper);
        }

        internal static List<Polygon> ToPolygons(this PolyTree result, int rounding = -1)
        {
            return result.Childs
                .Select(c => new Polygon((new[] { ToLineStringClosed(c, rounding) })
                             .Concat(c.Childs.Select(h => ToLineStringClosed(h, rounding))))).ToList();
        }

        /// <summary>
        /// Subtract <paramref name="clip"/> polygons from <paramref name="subject"/> and returns the result.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="clip"></param>
        /// <param name="rounding"></param>
        /// <returns></returns>
        /// <remarks>
        /// Polygons in <paramref name="clip"/> must not overlap between them.
        /// </remarks>
        public static IEnumerable<Polygon> Substract(this Polygon subject, IEnumerable<Polygon> clip, int rounding = -1)
        {
            var clipper = new Clipper();
            foreach (var line in subject.Coordinates)
            {
                clipper.AddPath(line.Coordinates.Select(p => p.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            foreach (var poly in clip)
            {
                foreach (var line in poly.Coordinates)
                {
                    clipper.AddPath(line.Coordinates.Select(p => p.ToIntPoint()).ToList(), PolyType.ptClip, true);
                }
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctXor, result);
            return result.ToPolygons(rounding);
        }

        public static MultiPolygon UnionToMultiPolygon(this IEnumerable<Polygon> polygons)
        {
            var withoutHoles = polygons.Where(p => p.Coordinates.Count == 1).ToList();
            var withHoles = polygons.Where(p => p.Coordinates.Count > 1).ToList();
            if (withoutHoles.Count > 0)
            {
                withoutHoles = UnionWithoutHoles(withoutHoles);
                if (withHoles.Count == 0)
                {
                    return new MultiPolygon(withoutHoles);
                }
                withHoles.AddRange(withHoles);
            }
            // Naive way : merge two by two if bounding boxes overlaps
            throw new NotImplementedException("TODO");
        }

        private static List<Polygon> UnionWithoutHoles(IEnumerable<Polygon> polygons)
        {
            var clipper = new Clipper();
            foreach (var poly in polygons)
            {
                clipper.AddPath(poly.Coordinates[0].Coordinates.Select(p => p.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftNonZero);
            return ToPolygons(result);
        }

        public static IEnumerable<Polygon> Union(this Polygon subject, Polygon other)
        {
            var clipper = new Clipper();
            foreach (var line in subject.Coordinates)
            {
                clipper.AddPath(line.Coordinates.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            foreach (var line in other.Coordinates)
            {
                clipper.AddPath(line.Coordinates.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctUnion, result);
            return ToPolygons(result);
        }

        public static bool IsPointInside(this Polygon polygon, IPosition pt)
        {
            if (polygon.Coordinates.Count == 0)
            {
                return false;
            }
            return polygon.Coordinates[0].Coordinates.IsPointInside(pt) 
                && polygon.Coordinates.Skip(1).All(hole => !hole.Coordinates.IsPointInsideOrOnBoundary(pt));
        }

        public static bool IsPointInside(this LineString path, IPosition pt)
        {
            return path.IsClosed() && path.Coordinates.IsPointInside(pt);
        }

        public static bool IsPointInside(this IReadOnlyList<IPosition> path, IPosition pt)
        {
            return PointInPolygon(path, pt) == 1;
        }


        public static bool IsPointInsideOrOnBoundary(this Polygon polygon, IPosition pt)
        {
            if (polygon.Coordinates.Count == 0)
            {
                return false;
            }
            return polygon.Coordinates[0].Coordinates.IsPointInsideOrOnBoundary(pt)
                && polygon.Coordinates.Skip(1).All(hole => !hole.Coordinates.IsPointInside(pt));
        }

        public static bool IsPointInsideOrOnBoundary(this LineString path, IPosition pt)
        {
            return path.IsClosed() && path.Coordinates.IsPointInsideOrOnBoundary(pt);
        }

        public static bool IsPointInsideOrOnBoundary(this IReadOnlyList<IPosition> path, IPosition pt)
        {
            return PointInPolygon(path, pt) != 0;
        }


        public static bool IsPointOnBoundary(this Polygon polygon, IPosition pt)
        {
            if (polygon.Coordinates.Count == 0)
            {
                return false;
            }
            return polygon.Coordinates.Any(l => l.Coordinates.IsPointOnBoundary(pt));
        }

        public static bool IsPointOnBoundary(this LineString path, IPosition pt)
        {
            return path.IsClosed() && path.Coordinates.IsPointOnBoundary(pt);
        }

        public static bool IsPointOnBoundary(this IReadOnlyList<IPosition> path, IPosition pt)
        {
            return PointInPolygon(path, pt) == -1;
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

    }
}

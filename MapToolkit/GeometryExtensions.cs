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
            return new MultiPolygon(result.Childs
                .Select(c => new Polygon((new[] { ToLineStringClosed(c) })
                             .Concat(c.Childs.Select(h => ToLineStringClosed(h))))).ToList());
        }

        internal static bool IsCounterClockWise<T>(this List<T> points) where T : IPosition
        {
            var north = points.IndexOf(points.OrderByDescending(p => p.Latitude).First());
            var east = points.IndexOf(points.OrderByDescending(p => p.Longitude).First());
            var south = points.IndexOf(points.OrderBy(p => p.Latitude).First());
            var west = points.IndexOf(points.OrderBy(p => p.Longitude).First());

            var epos = (east - north) % points.Count;
            var spos = (south - north) % points.Count;
            var wpos = (west - north) % points.Count;

            return epos >= spos && spos >= wpos;
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

        internal static IEnumerable<Polygon> ToPolygons(this PolyTree result, int rounding = -1)
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
    }
}

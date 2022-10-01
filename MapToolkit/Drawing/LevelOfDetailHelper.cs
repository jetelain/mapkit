using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing
{
    public static class LevelOfDetailHelper
    {

        public static double AngleInRadians(Vector a, Vector b)
        {
            return Math.Atan2(b.Y - a.Y, b.X - a.X);
        }

        public static double DistanceSquared(Vector a, Vector b)
        {
            return (b - a).LengthSquared();
        }

        public static List<Vector> SimplifyAngles(IReadOnlyList<Vector> points, double thresholdInRadians = Math.PI / 36) // 5°
        {
            return SimplifyAngles(points, AngleInRadians, thresholdInRadians);
        }

        public static List<Vector> SimplifyAnglesAndDistances(List<Vector> points, double distanceSquaredThreshold = 1.5, double thresholdInRadians = Math.PI / 36) // 5°
        {
            return SimplifyAnglesAndDistances(points, DistanceSquared, distanceSquaredThreshold, AngleInRadians, thresholdInRadians);
        }

        public static List<T> SimplifyAnglesAndDistances<T>(List<T> points, Func<T, T, double> distance, double distanceThreshold, Func<T, T, double> angle, double thresholdInRadians = Math.PI / 36) // 5°
        {
            var result = points;
            if (points.Count > 2)
            {
                var count = points.Count;
                do
                {
                    result = SimplifyAngles(result, angle, thresholdInRadians);
                    count = result.Count;
                    result = SimplifyDistances(result, distance, distanceThreshold);
                } while (count != result.Count && result.Count > 2);
            }
            if (result.Count == 2 && distance(result[0], result[1]) < distanceThreshold)
            {
                return new List<T>();
            }
            return result;
        }

        public static List<T> SimplifyAngles<T>(IReadOnlyList<T> points, Func<T,T,double> angle, double angleThreshold = Math.PI / 36) // 5°
        {
            var result = new List<T>() { points[0] };
            var i = 1;
            var max = points.Count - 1;
            while(i < max)
            {
                var prev = result[result.Count - 1];
                var point = points[i];
                var next = points[i + 1];
                var delta = Math.Abs(angle(point, next) - angle(prev, point));
                if (delta >= angleThreshold)
                {
                    result.Add(point);
                }
                i++;
            }
            result.Add(points[max]);
            return result;
        }

        public static List<T> SimplifyDistances<T>(IReadOnlyList<T> points, Func<T, T, double> distance, double distanceThreshold)
        {
            var result = new List<T>() { points[0] };
            var i = 1;
            var max = points.Count - 1;
            while (i < max)
            {
                var prev = result[result.Count - 1];
                var point = points[i];
                var next = points[i + 1];
                if (distance(prev, point) >= distanceThreshold && distance(prev, next) >= distanceThreshold)
                {
                    result.Add(point);
                }
                i++;
            }
            result.Add(points[max]);
            return result;
        }

        public static List<Vector> SimplifyClosed(IEnumerable<Vector> enumerable, double lengthSquared)
        {
            var simplified = SimplifyAnglesAndDistances(enumerable.ToList(), lengthSquared);
            if (simplified.Count > 3)
            {
                return simplified;
            }
            return new List<Vector>();
        }

        public static IEnumerable<IEnumerable<Vector>>? SimplifyClosed(IEnumerable<IEnumerable<Vector>>? enumerable, double lengthSquared)
        {
            if(enumerable == null)
            {
                return null;
            }
            var result = new List<IEnumerable<Vector>>();
            foreach(var item in enumerable)
            {
                var simplified = SimplifyAnglesAndDistances(item.ToList(), lengthSquared);
                if (simplified.Count > 3)
                {
                    result.Add(simplified);
                }
            }
            return result;
        }

        internal static List<Vector> Simplify(List<Vector> list, double lengthSquared)
        {
            if (list.Count > 2 && list[0].Equals(list[list.Count - 1]))
            {
                return SimplifyClosed(list, lengthSquared);
            }
            return SimplifyAnglesAndDistances(list, lengthSquared);
        }

    }
}

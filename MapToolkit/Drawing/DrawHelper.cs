using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing
{
    public static class DrawHelper
    {
        public static List<Vector>? SimplifyClosed(IEnumerable<Vector> enumerable, double lengthSquared = 9)
        {
            var simplified = SimplifyKeepLast(enumerable, lengthSquared);
            if ( simplified != null )
            {
                if (!simplified[0].Equals(simplified[simplified.Count-1]))
                {
                    simplified.Add(simplified[0]);
                }
                // XXX: Add an area criteria
                if (simplified.Count > 3)
                {
                    return simplified;
                }
            }
            return null;
        }

        public static IEnumerable<IEnumerable<Vector>>? SimplifyClosed(IEnumerable<IEnumerable<Vector>>? enumerable, double lengthSquared = 9)
        {
            if(enumerable == null)
            {
                return null;
            }
            var result = new List<IEnumerable<Vector>>();
            foreach(var item in enumerable)
            {
                var simplified = SimplifyClosed(item, lengthSquared);
                if (simplified != null)
                {
                    result.Add(simplified);
                }
            }
            return result;
        }

        internal static List<Vector>? Simplify(List<Vector> list)
        {
            if (list.Count > 2 && list[0].Equals(list[list.Count - 1]))
            {
                return SimplifyClosed(list);
            }
            return SimplifyKeepLast(list);
        }

        public static List<Vector>? SimplifyKeepLast(IEnumerable<Vector> enumerable, double lengthSquared = 9)
        {
            var result = new List<Vector>();
            Vector? last = null;
            foreach (var point in enumerable)
            {
                if (result.Count == 0 || (result[result.Count - 1] - point).LengthSquared() > lengthSquared)
                {
                    result.Add(point);
                    last = null;
                }
                else
                {
                    last = point;
                }
            }
            if (result.Count == 1)
            {
                return null;
            }
            if (last != null)
            {
                result.Add(last);
            }
            return result;
        }
    }
}

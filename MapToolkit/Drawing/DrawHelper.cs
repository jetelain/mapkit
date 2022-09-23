using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Drawing
{
    internal class DrawHelper
    {
        internal static List<Vector>? SimplifyClosed(IEnumerable<Vector> enumerable)
        {
            var simplified = SimplifyOpen(enumerable);
            if ( simplified != null )
            {
                if (!simplified[0].Equals(simplified[simplified.Count-1]))
                {
                    simplified.Add(simplified[0]);
                }
                if (simplified.Count > 3)
                {
                    return simplified;
                }
            }
            return null;
        }

        internal static IEnumerable<IEnumerable<Vector>>? SimplifyClosed(IEnumerable<IEnumerable<Vector>>? enumerable)
        {
            if(enumerable == null)
            {
                return null;
            }
            var result = new List<IEnumerable<Vector>>();
            foreach(var item in enumerable)
            {
                var simplified = SimplifyClosed(item);
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
            return SimplifyOpen(list);
        }

        internal static List<Vector>? SimplifyOpen(IEnumerable<Vector> enumerable)
        {
            var result = new List<Vector>();
            foreach (var point in enumerable)
            {
                if (result.Count == 0 || (result[result.Count - 1] - point).LengthSquared() > 9)
                {
                    result.Add(point);
                }
            }
            if (result.Count > 1)
            {
                return result;
            }
            return null;
        }
    }
}

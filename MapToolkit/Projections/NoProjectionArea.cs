using System;
using Pmad.Geometry;

namespace Pmad.Cartography.Projections
{
    public class NoProjectionArea : IProjectionArea
    {
        private readonly double latFactor;
        private readonly double lonFactor;
        private readonly Vector2D factor;
        private readonly double minLon;
        private readonly double maxLat;

        public NoProjectionArea(Coordinates min, Coordinates max, Vector2D size)
        {
            Size = size;
            minLon = min.Longitude;
            maxLat = max.Latitude;
            latFactor = (max.Latitude - min.Latitude) / size.Y;
            lonFactor = (max.Longitude - min.Longitude) / size.X;
            factor = (max.Vector2D - min.Vector2D) / size;
        }

        public Vector2D Min => Vector2D.Zero;

        public Vector2D Size { get; }

        public Vector2D Project(CoordinatesValue point)
        {
            return new Vector2D(point.Longitude - minLon, maxLat - point.Latitude) / factor;
            //return new Vector(
            //    (point.Longitude - minLon) / lonFactor,
            //    (maxLat - point.Latitude) / latFactor);
        }

        public Vector2D[] Project(ReadOnlySpan<CoordinatesValue> coordinates)
        {
            var result = new Vector2D[coordinates.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Project(coordinates[i]);
            }
            return result;
        }
    }
}

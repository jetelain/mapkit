using System;
using Pmad.Geometry;

namespace MapToolkit.Projections
{
    public class NoProjectionArea : IProjectionArea
    {
        private readonly double latFactor;
        private readonly double lonFactor;
        private readonly Vector2D factor;
        private readonly double minLon;
        private readonly double maxLat;

        public NoProjectionArea(Coordinates min, Coordinates max, Vector size)
        {
            Size = size;
            minLon = min.Longitude;
            maxLat = max.Latitude;
            latFactor = (max.Latitude - min.Latitude) / size.Y;
            lonFactor = (max.Longitude - min.Longitude) / size.X;
            factor = (max.Vector2D - min.Vector2D) / size.Vector2D;
        }

        public Vector Min => Vector.Zero;

        public Vector Size { get; }

        public Vector Project(CoordinatesS point)
        {
            return new Vector(new Vector2D(point.Longitude - minLon, maxLat - point.Latitude) / factor);
            //return new Vector(
            //    (point.Longitude - minLon) / lonFactor,
            //    (maxLat - point.Latitude) / latFactor);
        }

        public Vector Project(Vector2D point)
        {
            return new Vector(
                (point.Longitude() - minLon) / lonFactor,
                (maxLat - point.Latitude()) / latFactor);
        }

        public Vector[] Project(ReadOnlySpan<CoordinatesS> coordinates)
        {
            var result = new Vector[coordinates.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Project(coordinates[i]);
            }
            return result;
        }
    }
}

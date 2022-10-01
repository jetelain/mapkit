using System;
using GeoJSON.Text.Geometry;

namespace MapToolkit.Projections
{
    public class NoProjectionArea : IProjectionArea
    {
        private readonly double latFactor;
        private readonly double lonFactor;
        private readonly double minLon;
        private readonly double maxLat;

        public NoProjectionArea(Coordinates min, Coordinates max, Vector size)
        {
            Size = size;
            minLon = min.Longitude;
            maxLat = max.Latitude;
            latFactor = (max.Latitude - min.Latitude) / size.Y;
            lonFactor = (max.Longitude - min.Longitude) / size.X;
        }

        public Vector Min => Vector.Zero;

        public Vector Size { get; }

        public Vector Project(IPosition point)
        {
            return new Vector(
                (point.Longitude - minLon) / lonFactor,
                (maxLat - point.Latitude) / latFactor);
        }
    }
}

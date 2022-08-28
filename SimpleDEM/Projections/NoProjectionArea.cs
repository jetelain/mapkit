using System;

namespace SimpleDEM.Projections
{
    public class NoProjectionArea : IProjectionArea
    {
        private readonly int rounding;
        private readonly double latFactor;
        private readonly double lonFactor;
        private readonly double minLon;
        private readonly double maxLat;

        public NoProjectionArea(Coordinates min, Coordinates max, Vector size, int rounding = 0)
        {
            Size = size;
            minLon = min.Longitude;
            maxLat = max.Latitude;
            latFactor = (max.Latitude - min.Latitude) / size.Y;
            lonFactor = (max.Longitude - min.Longitude) / size.X;
            this.rounding = rounding;
        }

        public Vector Min => Vector.Zero;

        public Vector Size { get; }

        public Vector Project(Coordinates point)
        {
            if (rounding == -1)
            {
                return new Vector(
                    (float)((point.Longitude - minLon) / lonFactor),
                    (float)((maxLat - point.Latitude) / latFactor));
            }
            return new Vector(
                (float)Math.Round((point.Longitude - minLon) / lonFactor, rounding),
                (float)Math.Round((maxLat - point.Latitude) / latFactor, rounding));
        }
    }
}

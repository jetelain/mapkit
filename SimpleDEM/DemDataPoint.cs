using System;

namespace SimpleDEM
{
    public class DemDataPoint : IEquatable<DemDataPoint>
    {
        public DemDataPoint(GeodeticCoordinates coordinates, double elevation)
        {
            Coordinates = coordinates;
            Elevation = elevation;
        }

        public GeodeticCoordinates Coordinates { get; }

        public double Elevation { get; }

        public bool Equals(DemDataPoint? other)
        {
            if (other != null)
            {
                return Coordinates.Equals(other.Coordinates)
                    && Elevation == other.Elevation;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DemDataPoint);
        }

        public override int GetHashCode()
        {
            return Coordinates.GetHashCode() ^ Elevation.GetHashCode();
        }
    }
}

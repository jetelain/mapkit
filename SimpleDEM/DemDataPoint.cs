using System;

namespace SimpleDEM
{
    public sealed class DemDataPoint : IEquatable<DemDataPoint>
    {
        public DemDataPoint(Coordinates coordinates, double elevation)
        {
            Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
            Elevation = elevation;
        }

        public Coordinates Coordinates { get; }

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

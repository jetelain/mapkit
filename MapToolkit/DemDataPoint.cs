using System;
using System.Diagnostics;
using GeoJSON.Text.Geometry;

namespace MapToolkit
{
    [DebuggerDisplay("{Coordinates} => {Elevation}")]
    public sealed class DemDataPoint : IEquatable<DemDataPoint>, IPosition
    {
        public DemDataPoint(Coordinates coordinates, double elevation)
        {
            Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
            Elevation = elevation;
        }

        public Coordinates Coordinates { get; }

        public double Elevation { get; }

        double? IPosition.Altitude => Elevation;

        double IPosition.Latitude => Coordinates.Latitude;

        double IPosition.Longitude => Coordinates.Longitude;

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

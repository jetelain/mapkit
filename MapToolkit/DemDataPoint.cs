using System;
using System.Diagnostics;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;

namespace Pmad.Cartography
{
    [DebuggerDisplay("{Coordinates} => {Elevation}")]
    public sealed class DemDataPoint : IEquatable<DemDataPoint>, IPosition
    {
        private readonly CoordinatesValue vector;

        public DemDataPoint(Coordinates coordinates, double elevation)
        {
            vector = (coordinates ?? throw new ArgumentNullException(nameof(coordinates)));
            Elevation = elevation;
        }

        public DemDataPoint(CoordinatesValue vector, double elevation)
        {
            this.vector = vector;
            Elevation = elevation;
        }

        public Coordinates Coordinates => new Coordinates(vector);

        public double Elevation { get; }

        double? IPosition.Altitude => Elevation;

        public double Latitude => vector.Latitude;

        public double Longitude => vector.Longitude;

        public Vector2D Vector2D => vector.Vector2D;

        public CoordinatesValue CoordinatesS => vector;

        public bool Equals(DemDataPoint? other)
        {
            if (other != null)
            {
                return vector.Vector2D == other.vector.Vector2D
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
            return vector.Vector2D.GetHashCode() ^ Elevation.GetHashCode();
        }
    }
}

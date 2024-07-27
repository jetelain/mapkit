using System;
using System.Diagnostics;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;

namespace MapToolkit
{
    [DebuggerDisplay("{Coordinates} => {Elevation}")]
    public sealed class DemDataPoint : IEquatable<DemDataPoint>, IPosition
    {
        private readonly Vector2D vector;

        public DemDataPoint(Coordinates coordinates, double elevation)
        {
            vector = (coordinates ?? throw new ArgumentNullException(nameof(coordinates))).Vector2D;
            Elevation = elevation;
        }

        public DemDataPoint(Vector2D vector, double elevation)
        {
            this.vector = vector;
            Elevation = elevation;
        }

        public Coordinates Coordinates => new Coordinates(vector);

        public double Elevation { get; }

        double? IPosition.Altitude => Elevation;

        public double Latitude => vector.Y;

        public double Longitude => vector.X;

        public Vector2D Vector2D => vector;

        public bool Equals(DemDataPoint? other)
        {
            if (other != null)
            {
                return vector == other.vector
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
            return vector.GetHashCode() ^ Elevation.GetHashCode();
        }
    }
}

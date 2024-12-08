using System;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;
using Pmad.Geometry.Shapes;

namespace Pmad.Cartography
{
    [DebuggerDisplay("({Latitude};{Longitude})")]
    public class Coordinates : IEquatable<Coordinates>, IPosition
    {
        private readonly Vector2D vector;

        public static ShapeSettings<double, Vector2D> LatLonSettings = new ShapeSettings<double, Vector2D>(2_000_000, 4);
        public static ShapeSettings<double, Vector2D> EastingNorthingSettings = new ShapeSettings<double, Vector2D>(1_000, 4);

        public static readonly Coordinates Zero = new Coordinates(0, 0);

        /// <summary>
        /// Creates a new instance of Coordinates
        /// </summary>
        /// <param name="latitude">Latitude (Y)</param>
        /// <param name="longitude">Longitude (X)</param>
        [JsonConstructor]
        public Coordinates(double latitude, double longitude)
        {
            vector = new Vector2D(longitude, latitude);
        }

        public Coordinates(Vector2D vector)
        {
            this.vector = vector;
        }

        public Coordinates(CoordinatesValue crd)
        {
            this.vector = crd.Vector2D;
        }

        public static Coordinates FromXY(double x, double y)
        {
            return new Coordinates(new Vector2D(x, y));
        }

        public static Coordinates FromXY(Vector2 vector)
        {
            return new Coordinates(new Vector2D(vector.X, vector.Y));
        }

        public static Coordinates FromLatLon(double lat, double lon)
        {
            return new Coordinates(new Vector2D(lon, lat));
        }

        public double Latitude => vector.Y;

        public double Longitude => vector.X;

        [JsonIgnore]
        public Vector2D Vector2D => vector;

        [JsonIgnore]
        public double? Altitude => null;

        public bool Equals(Coordinates? other)
        {
            if (other != null)
            {
                return other.vector == vector;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Coordinates);
        }

        public override int GetHashCode()
        {
            return vector.GetHashCode();
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"({Latitude};{Longitude})");
        }

        internal double Distance(Coordinates coordinates)
        {
            return Math.Sqrt(DistanceSquared(coordinates));
        }

        internal double DistanceSquared(Coordinates coordinates)
        {
            return (coordinates.vector - vector).LengthSquared();
        }

        public bool IsInSquare(Coordinates start, Coordinates end)
        {
            return vector.IsInRange(start.vector, end.vector);
        }

        public static Coordinates operator+ (Coordinates c, Vector v)
        {
            return new Coordinates(c.vector + v.Vector2D);
        }

        public static Coordinates operator -(Coordinates c, Vector v)
        {
            return new Coordinates(c.vector - v.Vector2D);
        }

        public static Vector operator -(Coordinates a, Coordinates b)
        {
            return new Vector(a.vector - b.vector);
        }

        public Coordinates Round(int digits)
        {
            return new Coordinates(Math.Round(Latitude, digits), Math.Round(Longitude, digits));
        }
    }
}

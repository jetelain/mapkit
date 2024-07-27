using System;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;
using Pmad.Geometry.Shapes;

namespace MapToolkit
{
    [DebuggerDisplay("({Latitude};{Longitude})")]
    public class Coordinates : IEquatable<Coordinates>, IPosition
    {
        private readonly Vector2D vector;

        public static ShapeSettings<double, Vector2D> LatLonSettings = new ShapeSettings<double, Vector2D>(2_000_000, 4);
        public static ShapeSettings<double, Vector2D> EastingNorthingSettings = new ShapeSettings<double, Vector2D>(1_000, 4);

        internal const double ScaleForClipper = 2_000_000d;

        public static readonly Coordinates Zero = new Coordinates(0, 0);

        [JsonConstructor]
        public Coordinates(double latitude, double longitude)
        {
            vector = new Vector2D(longitude, latitude);
        }

        public Coordinates(Vector2D vector)
        {
            this.vector = vector;
        }

        public Coordinates(IntPoint point, int rounding = -1, double scaleForClipper = ScaleForClipper)
        {
            vector = new Vector2D(point.X, point.Y) / ScaleForClipper;
            if (rounding > -1)
            {
                vector = new Vector2D(Math.Round(vector.X, rounding), Math.Round(vector.Y, rounding));
            }
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

        internal const double DefaultThreshold = 0.000_005; // Less than 1m at equator

        internal const double DefaultThresholdSquared = DefaultThreshold * DefaultThreshold;

        public bool AlmostEquals(Coordinates? other, double thresholdSqared = DefaultThresholdSquared)
        {
            if (other != null)
            {
                if (other.vector == vector)
                {
                    return true;
                }
                if (DistanceSquared(other) <= thresholdSqared)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInSquare(Coordinates start, Coordinates end)
        {
            return vector.IsInRange(start.vector, end.vector);
        }

        public IntPoint ToIntPoint(double scaleForClipper = ScaleForClipper)
        {
            return new IntPoint(vector.X * scaleForClipper, vector.Y * scaleForClipper);
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

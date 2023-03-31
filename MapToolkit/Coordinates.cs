using System;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoJSON.Text.Geometry;

namespace MapToolkit
{
    public class Coordinates : IEquatable<Coordinates>, IPosition
    {
        internal const double ScaleForClipper = 2_000_000d;

        public static readonly Coordinates Zero = new Coordinates(0, 0);

        [JsonConstructor]
        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Coordinates(IntPoint point, int rounding = -1, double scaleForClipper = ScaleForClipper)
        {
            Latitude = point.Y / scaleForClipper;
            Longitude = point.X / scaleForClipper;

            if (rounding > -1)
            {
                Latitude = Math.Round(Latitude, rounding);
                Longitude = Math.Round(Longitude, rounding);
            }
        }

        public double Latitude { get; }

        public double Longitude { get; }

        [JsonIgnore]
        public double? Altitude => null;

        public bool Equals(Coordinates? other)
        {
            if (other != null)
            {
                return other.Latitude == Latitude 
                    && other.Longitude == Longitude;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Coordinates);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
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
            var dy = Latitude - coordinates.Latitude;
            var dx = Longitude - coordinates.Longitude;
            return (dx * dx) + (dy * dy);
        }

        internal const double DefaultThreshold = 0.000_005; // Less than 1m at equator

        internal const double DefaultThresholdSquared = DefaultThreshold * DefaultThreshold;

        public bool AlmostEquals(Coordinates? other, double thresholdSqared = DefaultThresholdSquared)
        {
            if (other != null)
            {
                if (other.Latitude == Latitude && other.Longitude == Longitude)
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
            return start.Latitude <= Latitude && Latitude <= end.Latitude &&
                   start.Longitude <= Longitude && Longitude <= end.Longitude;
        }

        public IntPoint ToIntPoint(double scaleForClipper = ScaleForClipper)
        {
            return new IntPoint(Longitude * scaleForClipper, Latitude * scaleForClipper);
        }

        public static Coordinates operator+ (Coordinates c, Vector v)
        {
            return new Coordinates(c.Latitude + v.DeltaLat, c.Longitude + v.DeltaLon);
        }

        public static Coordinates operator -(Coordinates c, Vector v)
        {
            return new Coordinates(c.Latitude - v.DeltaLat, c.Longitude - v.DeltaLon);
        }
        public static Vector operator -(Coordinates a, Coordinates b)
        {
            return Vector.FromLatLonDelta(a.Latitude - b.Latitude, a.Longitude - b.Longitude);
        }

        public Coordinates Round(int digits)
        {
            return new Coordinates(Math.Round(Latitude, digits), Math.Round(Longitude, digits));
        }
    }
}

using System;
using System.Text.Json.Serialization;

namespace SimpleDEM
{
    public class GeodeticCoordinates : IEquatable<GeodeticCoordinates>
    {
        [JsonConstructor]
        public GeodeticCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public bool Equals(GeodeticCoordinates? other)
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
            return Equals(obj as GeodeticCoordinates);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"({Latitude};{Longitude})");
        }

        internal double Distance(GeodeticCoordinates coordinates)
        {
            var dx = Latitude - coordinates.Latitude;
            var dy = Longitude - coordinates.Longitude;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public bool IsInSquare(GeodeticCoordinates start, GeodeticCoordinates end)
        {
            return start.Latitude <= Latitude && Latitude <= end.Latitude &&
                   start.Longitude <= Longitude && Longitude <= end.Longitude;
        }

    }
}

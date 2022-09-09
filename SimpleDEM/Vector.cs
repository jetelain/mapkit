using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace SimpleDEM
{
    public sealed class Vector : IEquatable<Vector>
    {
        public static readonly Vector Zero = new Vector(0, 0);

        public static readonly Vector One = new Vector(1, 1);

        [JsonConstructor]
        public Vector(double x, double y)
        {
            Y = y;
            X = x;
        }

        public Vector(Vector2 floatVector)
        {
            Y = floatVector.Y;
            X = floatVector.X;
        }

        public static Vector FromLatLonDelta(float lat, float lon)
        {
            return new Vector(lon, lat);
        }

        public static Vector FromXYDelta(float x, float y)
        {
            return new Vector(x, y);
        }

        public double Y { get; }

        public double X { get; }

        public double DeltaLon => X;

        public double DeltaLat => Y;

        public bool Equals(Vector? other)
        {
            if (other != null)
            {
                return other.Y == Y 
                    && other.X == X;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Vector);
        }

        public override int GetHashCode()
        {
            return Y.GetHashCode() ^ X.GetHashCode();
        }

        public Vector2 ToFloat()
        {
            return new Vector2((float)X, (float)Y);
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"({X};{Y})");
        }
    }
}

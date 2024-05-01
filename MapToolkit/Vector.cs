using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace MapToolkit
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

        public static Vector FromLatLonDelta(double lat, double lon)
        {
            return new Vector(lon, lat);
        }

        public static Vector FromXYDelta(double x, double y)
        {
            return new Vector(x, y);
        }

        [JsonPropertyName("y")]
        public double Y { get; }

        [JsonPropertyName("x")]
        public double X { get; }

        [JsonIgnore]
        public double DeltaLon => X;

        [JsonIgnore]
        public double DeltaLat => Y;

        public double LengthSquared()
        {
            return (X * X) + (Y * Y);
        }

        internal double Surface()
        {
            return X * Y;
        }

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

        public double Atan2()
        {
            return Math.Atan2(Y, X);
        }

        public Vector2 ToFloat()
        {
            return new Vector2((float)X, (float)Y);
        }

        public static Vector operator *(Vector v, double f)
        {
            return new Vector(v.X * f, v.Y * f);
        }
        public static Vector operator /(Vector v, double f)
        {
            return new Vector(v.X / f, v.Y / f);
        }
        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(a.X / b.X, a.Y / b.Y);
        }
        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.X * b.X, a.Y * b.Y);
        }
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }
        public override string ToString()
        {
            return FormattableString.Invariant($"({X};{Y})");
        }
    }
}

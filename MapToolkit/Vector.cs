using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Pmad.Geometry;

namespace MapToolkit
{
    public struct Vector : IEquatable<Vector>
    {
        private readonly Vector2D vector;

        public static readonly Vector Zero = new Vector(0, 0);

        public static readonly Vector One = new Vector(1, 1);

        [JsonConstructor]
        public Vector(double x, double y)
        {
            vector = new Vector2D(x, y);
        }

        public Vector(Vector2D vector)
        {
            this.vector = vector;
        }

        public Vector(Vector2 floatVector)
        {
            vector = new Vector2D(floatVector.X, floatVector.Y);
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
        public double Y => vector.Y;

        [JsonPropertyName("x")]
        public double X => vector.X;

        [JsonIgnore]
        public double DeltaLon => X;

        [JsonIgnore]
        public double DeltaLat => Y;

        public double LengthSquared()
        {
            return vector.LengthSquared();
        }

        public Vector2D Vector2D => vector;

        internal double Surface()
        {
            return vector.Area();
        }

        public bool Equals(Vector other)
        {
            return other.vector == this.vector;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Vector v)
            {
                return Equals(v);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return vector.GetHashCode();
        }

        public double Atan2()
        {
            return vector.Atan2();
        }

        public Vector2 ToFloat()
        {
            return new Vector2((float)X, (float)Y);
        }

        public static Vector operator *(Vector v, double f)
        {
            return new Vector(v.vector * f);
        }
        public static Vector operator /(Vector v, double f)
        {
            return new Vector(v.vector / f);
        }
        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(a.vector / b.vector);
        }
        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.vector * b.vector);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.vector + b.vector);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.vector - b.vector);
        }
        public override string ToString()
        {
            return FormattableString.Invariant($"({X};{Y})");
        }
    }
}

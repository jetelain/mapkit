using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Pmad.Geometry;

namespace Pmad.Cartography
{
    [DebuggerDisplay("({Latitude};{Longitude})")]
    public struct CoordinatesValue : IEquatable<CoordinatesValue>
    {
        public readonly Vector2D Vector2D;

        public CoordinatesValue(double latitude, double longitude)
        {
            Vector2D = new Vector2D(longitude, latitude);
        }

        public CoordinatesValue(Vector2D vector)
        {
            Vector2D = vector;
        }

        public CoordinatesValue(Coordinates crd)
        {
            Vector2D = crd.Vector2D;
        }

        public double Latitude => Vector2D.Y;

        public double Longitude => Vector2D.X;

        public bool AlmostEquals(CoordinatesValue other, double thresholdSqared )
        {
            if (other.Vector2D == Vector2D)
            {
                return true;
            }
            if ((other.Vector2D - Vector2D).LengthSquared() <= thresholdSqared)
            {
                return true;
            }
            return false;
        }

        public bool IsInSquare(VectorEnvelope<Vector2D> range)
        {
            return range.Contains(Vector2D);
        }

        public bool IsInSquare(CoordinatesValue min, CoordinatesValue max)
        {
            return Vector2D.IsInRange(min.Vector2D, max.Vector2D);
        }

        public bool Equals(CoordinatesValue other)
        {
            return Vector2D == other.Vector2D;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is CoordinatesValue other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Vector2D.GetHashCode();
        }

        public static CoordinatesValue operator +(CoordinatesValue c, Vector v)
        {
            return new CoordinatesValue(c.Vector2D + v.Vector2D);
        }

        public static CoordinatesValue operator -(CoordinatesValue c, Vector v)
        {
            return new CoordinatesValue(c.Vector2D - v.Vector2D);
        }

        public static Vector operator -(CoordinatesValue a, CoordinatesValue b)
        {
            return new Vector(a.Vector2D - b.Vector2D);
        }

        public static implicit operator Coordinates(CoordinatesValue d) => new Coordinates(d.Vector2D);

        public static implicit operator CoordinatesValue(Coordinates d) => new CoordinatesValue(d.Vector2D);
    }
}

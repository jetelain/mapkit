using System;
using System.Text.Json.Serialization;

namespace SimpleDEM
{
    public class Vector : IEquatable<Vector>
    {
        public static readonly Vector Zero = new Vector(0, 0);

        [JsonConstructor]
        public Vector(double x, double y)
        {
            Y = y;
            X = x;
        }

        public double Y { get; }

        public double X { get; }

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

        public override string ToString()
        {
            return FormattableString.Invariant($"({X};{Y})");
        }
    }
}

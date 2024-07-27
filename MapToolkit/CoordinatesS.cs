using Pmad.Geometry;

namespace MapToolkit
{
    public struct CoordinatesS
    {
        public readonly Vector2D Vector2D;

        public CoordinatesS(double latitude, double longitude)
        {
            Vector2D = new Vector2D(longitude, latitude);
        }

        public CoordinatesS(Vector2D vector)
        {
            Vector2D = vector;
        }

        public CoordinatesS(Coordinates crd)
        {
            Vector2D = crd.Vector2D;
        }

        public double Latitude => Vector2D.Y;

        public double Longitude => Vector2D.X;

        public bool AlmostEquals(CoordinatesS other, double thresholdSqared )
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

        public bool IsInSquare(CoordinatesS min, CoordinatesS max)
        {
            return Vector2D.IsInRange(min.Vector2D, max.Vector2D);
        }

        public static CoordinatesS operator +(CoordinatesS c, Vector v)
        {
            return new CoordinatesS(c.Vector2D + v.Vector2D);
        }

        public static CoordinatesS operator -(CoordinatesS c, Vector v)
        {
            return new CoordinatesS(c.Vector2D - v.Vector2D);
        }

        public static Vector operator -(CoordinatesS a, CoordinatesS b)
        {
            return new Vector(a.Vector2D - b.Vector2D);
        }

        public static implicit operator Coordinates(CoordinatesS d) => new Coordinates(d.Vector2D);

        public static implicit operator CoordinatesS(Coordinates d) => new CoordinatesS(d.Vector2D);
    }
}

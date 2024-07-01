using System.Numerics;

namespace MapToolkit.Drawing.Topographic
{
    internal class FollowPath
    {
        public static readonly Matrix3x2 Rotate90 = Matrix3x2.CreateRotation(1.57079637f);
        public static readonly Matrix3x2 RotateM90 = Matrix3x2.CreateRotation(-1.57079637f);

        private readonly IEnumerator<MapToolkit.Vector> enumerator;
        private MapToolkit.Vector? previousPoint;
        private MapToolkit.Vector? point;
        private MapToolkit.Vector? previousPosition;
        private MapToolkit.Vector? position;
        private Vector2 delta;
        private double length;
        private double positionOnSegment;
        private bool hasReachedEnd;
        private int index;

        public FollowPath(params MapToolkit.Vector[] points)
            : this((IEnumerable<MapToolkit.Vector>)points)
        {

        }

        public FollowPath(IEnumerable<MapToolkit.Vector> points)
        {
            enumerator = points.GetEnumerator();
            index = 0;
            Init();
        }

        public virtual void Reset()
        {
            enumerator.Reset();
            index = 0;
            Init();
        }

        private void Init()
        {
            previousPosition = null;
            length = 0f;
            positionOnSegment = 0f;
            previousPoint = null;
            if (enumerator.MoveNext())
            {
                position = point = enumerator.Current;
                delta = Vector2.Zero;
                hasReachedEnd = false;
                MoveNextPoint();
            }
            else
            {
                hasReachedEnd = true;
            }
        }

        private bool MoveNextPoint()
        {
            previousPoint = point;
            if (!enumerator.MoveNext())
            {
                point = null;
                length = 0f;
                positionOnSegment = 0f;
                return false;
            }
            index++;
            point = enumerator.Current;
            delta = (point - previousPoint!).ToFloat();
            length = delta.Length();
            positionOnSegment = 0f;
            return true;
        }

        public MapToolkit.Vector Current => position ?? MapToolkit.Vector.Zero;

        public MapToolkit.Vector Previous => previousPosition ?? MapToolkit.Vector.Zero;

        public Vector2 Vector => Vector2.Normalize(delta);

        public MapToolkit.Vector Vector90 => new MapToolkit.Vector(Vector2.Transform(Vector, Rotate90));

        public MapToolkit.Vector VectorM90 => new MapToolkit.Vector(Vector2.Transform(Vector, RotateM90));

        public bool IsLast => hasReachedEnd;

        public bool Move(double step)
        {
            if (hasReachedEnd)
            {
                return false;
            }
            var remainLength = step;
            while (remainLength + positionOnSegment > length)
            {
                remainLength -= length - positionOnSegment;
                var previousDelta = delta;
                if (!MoveNextPoint())
                {
                    hasReachedEnd = true;
                    if (!(position!.Equals(previousPoint)))
                    {
                        previousPosition = position;
                        position = previousPoint;
                        return true;
                    }
                    return false;
                }
            }
            positionOnSegment += remainLength;
            previousPosition = position;
            var amount = positionOnSegment / length;
            position = (previousPoint! * (1.0 - amount)) + (point! * amount);
            return true;
        }
    }
}

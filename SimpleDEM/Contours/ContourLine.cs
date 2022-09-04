using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDEM.Contours
{
    public class ContourLine
    {
        internal ContourLine(ContourSegment segment)
        {
            Points.Add(segment.Point1);
            Points.Add(segment.Point2);
            Level = segment.Level;
        }

        public List<Coordinates> Points { get; } = new List<Coordinates>();

        public double Level { get; }

        public Coordinates First => Points[0];

        public Coordinates Last => Points[Points.Count - 1];

        public bool IsClosed { get; private set; }

        public bool IsSinglePoint => IsClosed && Points.Count == 2;

        public bool IsDiscarded => IsClosed && Points.Count == 0;


        internal bool TryAdd(ContourSegment segment, double thresholdSqared = Coordinates.DefaultThresholdSquared)
        {
            if (IsClosed || segment.Level != Level)
            {
                return false;
            }
            if (segment.Point1.AlmostEquals(First, thresholdSqared))
            {
                Points.Insert(0, segment.Point2);
            }
            else if (segment.Point1.AlmostEquals(Last, thresholdSqared))
            {
                Points.Add(segment.Point2);
            }
            else if (segment.Point2.AlmostEquals(First, thresholdSqared))
            {
                Points.Insert(0, segment.Point1);
            }
            else if (segment.Point2.AlmostEquals(Last, thresholdSqared))
            {
                Points.Add(segment.Point1);
            }
            else
            {
                return false;
            }
            UpdateIsClosed(thresholdSqared);
            return true;
        }

        internal bool TryMerge(ContourLine other, double thresholdSqared = Coordinates.DefaultThresholdSquared, bool log = false)
        {
            if (IsClosed || other.IsClosed || other.Level != Level || other == this)
            {
                return false;
            }
            if (Last.AlmostEquals(other.First, thresholdSqared))
            {
                if (log)
                {

                }

                Points.AddRange(other.Points.Skip(1));
                other.Discard();
            }
            else if (Last.AlmostEquals(other.Last, thresholdSqared))
            {
                if (log)
                {

                }

                other.Points.Reverse(); // other.Last becomes other.First
                Points.AddRange(other.Points.Skip(1));
                other.Discard();
            }
            else if (First.AlmostEquals(other.First, thresholdSqared))
            {
                if (log)
                {


                }
                Points.Reverse(); // First becomes Last
                Points.AddRange(other.Points.Skip(1));
                other.Discard();
            }
            else if (First.AlmostEquals(other.Last, thresholdSqared))
            {
                if (log)
                {

                }
                Points.Reverse(); // First becomes Last
                other.Points.Reverse(); // First becomes Last
                Points.AddRange(other.Points.Skip(1));
                other.Discard();
            }
            else
            {
                return false;
            }
            UpdateIsClosed(thresholdSqared);
            return true;
        }

        private void Discard()
        {
            IsClosed = true;
            Points.Clear();
        }

        internal void UpdateIsClosed(double thresholdSqared)
        {
            if (Last.AlmostEquals(First, thresholdSqared))
            {
                IsClosed = true;
            }
        }
    }
}

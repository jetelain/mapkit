﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.Contours
{
    public sealed class ContourLine
    {
        internal ContourLine(ContourSegment segment)
        {
#if DEBUG
            if (!segment.IsValidHypothesis)
            {
                throw new ArgumentException();
            }
#endif
            Points.Add(segment.Point1);
            Points.Add(segment.Point2);
            Level = segment.Level;
        }

        public ContourLine(IEnumerable<Coordinates> points, double level)
        {
            Points.AddRange(points);
            Level = level;
            UpdateIsClosed(Coordinates.DefaultThresholdSquared);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Points are counter clockwise for hills.
        /// 
        /// Points are clockwise for basin.
        /// </remarks>
        public List<Coordinates> Points { get; } = new List<Coordinates>();

        public double Level { get; }

        public Coordinates First => Points[0];

        public Coordinates Last => Points[Points.Count - 1];

        public bool IsClosed { get; private set; }

        public bool IsSinglePoint => IsClosed && Points.Count == 2;

        public bool IsDiscarded => IsClosed && Points.Count == 0;


        internal bool TryAdd(ContourSegment segment, double thresholdSqared = Coordinates.DefaultThresholdSquared)
        {
#if DEBUG
            if (!segment.IsValidHypothesis)
            {
                throw new ArgumentException();
            }
#endif
            if (IsClosed || segment.Level != Level)
            {
                return false;
            }
            if (segment.Point1.AlmostEquals(Last, thresholdSqared))
            {
                Points.Add(segment.Point2);
            }
            else if (segment.Point2.AlmostEquals(First, thresholdSqared))
            {
                Points.Insert(0, segment.Point1);
            }
            else
            {
                return false;
            }
            UpdateIsClosed(thresholdSqared);
            segment.ValidateHypothesis();
            return true;
        }

        internal bool TryMerge(ContourLine other, double thresholdSqared = Coordinates.DefaultThresholdSquared)
        {
            if (IsClosed || other.IsClosed || other.Level != Level || other == this)
            {
                return false;
            }
            if (Last.AlmostEquals(other.First, thresholdSqared))
            {
                Points.AddRange(other.Points.Skip(1));
                other.Discard();
            }
            else if (First.AlmostEquals(other.Last, thresholdSqared))
            {
                other.Points.AddRange(Points.Skip(1));
                Points.Clear();
                Points.AddRange(other.Points);
                other.Discard();
            }
            else
            {
                return false;
            }
            UpdateIsClosed(thresholdSqared);
            return true;
        }

        public void Append(ContourLine other, double thresholdSqared = Coordinates.DefaultThresholdSquared)
        {
            if (other == this)
            {
                Points.Add(First);
                UpdateIsClosed(thresholdSqared);
                return;
            }
            Points.AddRange(other.Points);
            other.Discard();
            UpdateIsClosed(thresholdSqared);
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

        public bool IsCounterClockWise => Points.IsCounterClockWise();
    }
}

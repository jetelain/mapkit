﻿using System.Collections.Generic;
using System.Linq;

namespace Pmad.Cartography.Contours
{
    /// <summary>
    /// "Marching square algorithm" applied to 4 points that form a square
    /// </summary>
    /// <remarks>
    /// Based on GDAL implementation from <see href="https://github.com/OSGeo/gdal/blob/master/alg/marching_squares/square.h" />
    /// </remarks>
    internal class ContourSquare
    {
        const int NorthWestBit = 0b0001;
        const int SouthWestBit = 0b0010;
        const int SouthEastBit = 0b0100;
        const int NorthEastBit = 0b1000;

        private readonly DemDataPoint northWest;
        private readonly DemDataPoint southWest;
        private readonly DemDataPoint southEast;
        private readonly DemDataPoint northEast;

        internal ContourSquare(DemDataPoint northWest, DemDataPoint southWest, DemDataPoint southEast, DemDataPoint northEast)
        {
            this.northWest = northWest;
            this.southWest = southWest;
            this.southEast = southEast;
            this.northEast = northEast;
        }

        public IEnumerable<ContourSegment> Segments(IContourLevelGenerator generator)
        {
            // TODO: Take care of NaN
            var elevations = new[] { northWest.Elevation, southWest.Elevation, southEast.Elevation, northEast.Elevation };
            var min = elevations.Min();
            var max = elevations.Max();
            return generator.Levels(min, max).SelectMany(level => SegmentsForLevel(level));
        }

        private IEnumerable<ContourSegment> SegmentsForLevel(double level)
        {
            var kind = GetMarchingSquareCase(level);
            switch (kind)
            {
                case NorthWestBit:
                    // 1 0
                    // 0 0
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateNorthBorder(level), level, kind);
                    break;
                case SouthWestBit:
                    // 0 0
                    // 1 0
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateWestBorder(level), level, kind);
                    break;
                case SouthEastBit:
                    // 0 0
                    // 0 1
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateSouthBorder(level), level, kind);
                    break;
                case NorthEastBit:
                    // 0 1
                    // 0 0
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateEastBorder(level), level, kind);
                    break;
                case 0b0011: // NorthWestBit | SouthWestBit
                    // 1 0
                    // 1 0
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateNorthBorder(level), level, kind);
                    break;
                case 0b0110: // SouthWestBit | SouthEastBit
                    // 0 0
                    // 1 1
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateWestBorder(level), level, kind);
                    break;
                case 0b1100: // SouthEastBit | NorthEastBit
                    // 0 1
                    // 0 1
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateSouthBorder(level), level, kind);
                    break;
                case 0b1001: // NorthWestBit | NorthEastBit
                    // 1 1
                    // 0 0
                    yield return new ContourSegment( InterpolateWestBorder(level), InterpolateEastBorder(level), level, kind);
                    break;
                case 0b1110: // Reverse NorthWestBit
                    // 0 1
                    // 1 1
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateWestBorder(level), level, kind);
                    break;
                case 0b1101: // Reverse SouthWestBit
                    // 1 1
                    // 0 1
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateSouthBorder(level), level, kind);
                    break;
                case 0b1011: // Reverse SouthEastBit
                    // 1 1
                    // 1 0
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateEastBorder(level), level, kind) ;
                    break;
                case 0b0111: // Reverse NorthEastBit
                    // 1 0
                    // 1 1
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateNorthBorder(level), level, kind);
                    break;
                case 0b0101: // Saddle North-East : WN+ES or WS+EN
                    // 1 0
                    // 0 1
                    var h = new ContourHypothesis();

                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateNorthBorder(level), level, kind, h, 1);
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateSouthBorder(level), level, kind, h, 1);

                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateSouthBorder(level), level, kind, h, 2);
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateNorthBorder(level), level, kind, h, 2);

                    break;
                case 0b1010: // Saddle North-West : NW+SE or NE+SW
                    // 0 1
                    // 1 0
                    h = new ContourHypothesis();

                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateWestBorder(level), level, kind, h, 1);
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateEastBorder(level), level, kind, h, 1);

                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateEastBorder(level), level, kind, h, 2);
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateWestBorder(level), level, kind, h, 2);

                    break;
                case 0b0000: // None above level
                case 0b1111: // All above level
                    // Generates nothing (and they should not exists if ContourLevelGenerator does it jobs)
                    break;
            }
        }

        private int GetMarchingSquareCase(double level)
        {
            return (level < northWest.Elevation ? NorthWestBit : 0)
                | (level < southWest.Elevation ? SouthWestBit : 0)
                | (level < southEast.Elevation ? SouthEastBit : 0)
                | (level < northEast.Elevation ? NorthEastBit : 0);
        }

        private CoordinatesValue InterpolateWestBorder(double level)
        {
            return new CoordinatesValue(
                Interpolate(level, southWest.Latitude, northWest.Latitude, southWest.Elevation, northWest.Elevation),
                northWest.Longitude);
        }

        private CoordinatesValue InterpolateSouthBorder(double level)
        {
            return new CoordinatesValue(southWest.Latitude,
                Interpolate(level, southWest.Longitude, southEast.Longitude, southWest.Elevation, southEast.Elevation));
        }

        private CoordinatesValue InterpolateEastBorder(double level)
        {
            return new CoordinatesValue(
                Interpolate(level, southEast.Latitude, northEast.Latitude, southEast.Elevation, northEast.Elevation),
                northEast.Longitude);
        }

        private CoordinatesValue InterpolateNorthBorder(double level)
        {
            return new CoordinatesValue(
                northWest.Latitude,
                Interpolate(level, northWest.Longitude, northEast.Longitude, northWest.Elevation, northEast.Elevation));
        }

        private static double Interpolate(double level, double x1, double x2, double e1, double e2)
        {
            double ratio = (level - e1) / (e2 - e1);
            return x1 * (1 - ratio) + x2 * ratio;
        }

    }
}

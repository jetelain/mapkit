using System.Collections.Generic;
using System.Linq;

namespace SimpleDEM.Contours
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

        public IEnumerable<ContourSegment> Segments(ContourLevelGenerator generator)
        {
            // TODO: Take care of NaN
            var elevations = new[] { northWest.Elevation, southWest.Elevation, southEast.Elevation, northEast.Elevation };
            var min = elevations.Min();
            var max = elevations.Max();
            return generator.Levels(min, max).SelectMany(level => SegmentsForLevel(level));
        }

        private IEnumerable<ContourSegment> SegmentsForLevel(double level)
        {
            switch (GetMarchingSquareCase(level))
            {
                case NorthWestBit:
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateWestBorder(level), level);
                    break;
                case SouthWestBit:
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateSouthBorder(level), level);
                    break;
                case SouthEastBit:
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateEastBorder(level), level);
                    break;
                case NorthEastBit:
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateNorthBorder(level), level);
                    break;
                case 0b0011: // NorthWestBit | SouthWestBit
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateSouthBorder(level), level);
                    break;
                case 0b0110: // SouthWestBit | SouthEastBit
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateEastBorder(level), level);
                    break;
                case 0b1100: // SouthEastBit | NorthEastBit
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateNorthBorder(level), level);
                    break;
                case 0b1001: // NorthWestBit | NorthEastBit
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateWestBorder(level), level);
                    break;
                case 0b1110: // Reverse NorthWestBit
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateNorthBorder(level), level);
                    break;
                case 0b1101: // Reverse SouthWestBit
                    yield return new ContourSegment(InterpolateSouthBorder(level), InterpolateWestBorder(level), level);
                    break;
                case 0b1011: // Reverse SouthEastBit
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateSouthBorder(level), level);
                    break;
                case 0b0111: // Reverse NorthEastBit
                    yield return new ContourSegment(InterpolateNorthBorder(level), InterpolateEastBorder(level), level);
                    break;
                case 0b0101: // Saddle North-East
                case 0b1010: // Saddle North-West
                    // Behave like gdal reference implementaton
                    yield return new ContourSegment(InterpolateWestBorder(level), InterpolateSouthBorder(level), level);
                    yield return new ContourSegment(InterpolateEastBorder(level), InterpolateNorthBorder(level), level);
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

        private Coordinates InterpolateWestBorder(double level)
        {
            return new Coordinates(
                Interpolate(level, southWest.Coordinates.Latitude, northWest.Coordinates.Latitude, southWest.Elevation, northWest.Elevation),
                northWest.Coordinates.Longitude);
        }

        private Coordinates InterpolateSouthBorder(double level)
        {
            return new Coordinates(southWest.Coordinates.Latitude,
                Interpolate(level, southWest.Coordinates.Longitude, southEast.Coordinates.Longitude, southWest.Elevation, southEast.Elevation));
        }

        private Coordinates InterpolateEastBorder(double level)
        {
            return new Coordinates(
                Interpolate(level, southEast.Coordinates.Latitude, northEast.Coordinates.Latitude, southEast.Elevation, northEast.Elevation),
                northEast.Coordinates.Longitude);
        }

        private Coordinates InterpolateNorthBorder(double level)
        {
            return new Coordinates(
                northWest.Coordinates.Latitude,
                Interpolate(level, northWest.Coordinates.Longitude, northEast.Coordinates.Longitude, northWest.Elevation, northEast.Elevation));
        }

        private static double Interpolate(double level, double x1, double x2, double e1, double e2)
        {
            double ratio = (level - e1) / (e2 - e1);
            return x1 * (1 - ratio) + x2 * ratio;
        }

    }
}

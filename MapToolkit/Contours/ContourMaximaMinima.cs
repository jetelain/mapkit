using System;
using System.Collections.Generic;
using System.Linq;
using Pmad.Cartography.DataCells;
using Pmad.Cartography.Utils;
using Pmad.Geometry;
using Pmad.Geometry.Collections;

namespace Pmad.Cartography.Contours
{
    /// <summary>
    /// Finds the minima and maxima points of a <see cref="IDemDataView"/> using contour lines.
    /// </summary>
    public static class ContourMaximaMinima
    {
        /// <summary>
        /// Finds the minima points of a <see cref="IDemDataView"/> using contour lines.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="lines"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static List<DemDataPoint> FindMinima(IDemDataView cell, IEnumerable<ContourLine> lines, IProgress<double>? progress = null)
        {
            return FindMinimaOrMaxima(lines.Where(l => l.IsClosed && l.Level > 0 && !l.IsCounterClockWise), -1, cell, progress);
        }

        /// <summary>
        /// Finds the maxima points of a <see cref="IDemDataView"/> using contour lines.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="lines"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static List<DemDataPoint> FindMaxima(IDemDataView cell, IEnumerable<ContourLine> lines, IProgress<double>? progress = null)
        {
            return FindMinimaOrMaxima(lines.Where(l => l.IsClosed && l.Level > 0 && l.IsCounterClockWise), 1, cell, progress);
        }

        private static List<DemDataPoint> FindMinimaOrMaxima(IEnumerable<ContourLine> source, int factor, IDemDataView cell, IProgress<double>? progress)
        {
            var list = new List<ContourLine?>(source);
            var rel = new BasicProgress(new RelativeProgress(progress, 0, 0.5), list.Count);
            for (var i = 0; i < list.Count; ++i)
            {
                var line = list[i];
                if (line != null)
                {
                    var inside = list.FirstOrDefault(l => l != null && l.Level * factor > line.Level * factor && line.IsPointInside(l.First));
                    if (inside != null)
                    {
                        list[i] = null;
                    }
                }
                rel.AddOne();
            }
            var result = new List<DemDataPoint>();
            rel = new BasicProgress(new RelativeProgress(progress, 50, 0.5), list.Where(l => l != null).Count());
            foreach (var line in list.Where(l => l != null).Cast<ContourLine>())
            {
                var minMax = VectorEnvelope<Vector2D>.FromList(line.Points.AsSpan<CoordinatesValue,Vector2D>());

                var sub = cell.CreateView(new Coordinates(minMax.Min), new Coordinates(minMax.Max));

                var matching =
                        Enumerable
                        .Range(0, sub.PointsLat)
                        .SelectMany(lat => sub.GetPointsOnParallel(lat, 0, sub.PointsLon))
                        .Where(p => p.Elevation * factor > line.Level * factor)
                        .OrderByDescending(p => p.Elevation * factor)
                        .Where(p => line.IsPointInside(p.CoordinatesS))
                        .ToList();
                if (matching.Count > 0)
                {
                    var elevation = matching[0].Elevation;
                    var identitcal = matching.Where(m => m.Elevation == elevation);
                    var latitude = identitcal.Average(p => p.Latitude);
                    var longitude = identitcal.Average(p => p.Longitude);
                    result.Add(new DemDataPoint(new CoordinatesValue(latitude, longitude), elevation));
                }
                rel.AddOne();
            }
            return result;
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using MapToolkit.DataCells;

namespace MapToolkit.Contours
{
    public static class ContourMaximaMinima
    {
        public static List<DemDataPoint> FindMinima(IDemDataView cell, IEnumerable<ContourLine> lines)
        {
            return FindMinimaOrMaxima(lines.Where(l => l.IsClosed && l.Level > 0 && !l.IsCounterClockWise), -1, cell);
        }

        public static List<DemDataPoint> FindMaxima(IDemDataView cell, IEnumerable<ContourLine> lines)
        {
            return FindMinimaOrMaxima(lines.Where(l => l.IsClosed && l.Level > 0 && l.IsCounterClockWise), 1, cell);
        }

        private static List<DemDataPoint> FindMinimaOrMaxima(IEnumerable<ContourLine> source, int factor, IDemDataView cell)
        {
            var list = new List<ContourLine?>(source);
            for (var i = 0; i < list.Count; ++i)
            {
                var line = list[i];
                if (line != null)
                {
                    var inside = list.FirstOrDefault(l => l != null && l.Level * factor > line.Level * factor && line.Points.IsPointInside(l.First));
                    if (inside != null)
                    {
                        list[i] = null;
                    }
                }
            }
            var result = new List<DemDataPoint>();
            foreach (var line in list.Where(l => l != null).Cast<ContourLine>())
            {
                var min = new Coordinates(line.Points.Min(l => l.Latitude), line.Points.Min(l => l.Longitude));
                var max = new Coordinates(line.Points.Max(l => l.Latitude), line.Points.Max(l => l.Longitude));

                var sub = cell.CreateView(min, max);

                var matching =
                        Enumerable
                        .Range(0, sub.PointsLat)
                        .SelectMany(lat => sub.GetPointsOnParallel(lat, 0, sub.PointsLon))
                        .Where(p => p.Elevation * factor > line.Level * factor)
                        .OrderByDescending(p => p.Elevation * factor)
                        .Where(p => line.Points.IsPointInside(p.Coordinates))
                        .ToList();
                if (matching.Count > 0)
                {
                    var elevation = matching[0].Elevation;
                    var identitcal = matching.Where(m => m.Elevation == elevation);
                    var latitude = identitcal.Average(p => p.Coordinates.Latitude);
                    var longitude = identitcal.Average(p => p.Coordinates.Longitude);
                    result.Add(new DemDataPoint(new Coordinates(latitude, longitude), elevation));
                }
            }
            return result;
        }
    }
}

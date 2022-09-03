using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using SimpleDEM.DataCells;

namespace SimpleDEM.Contours
{
    public class ContourGraph
    {
        private class LinesByElevation : Dictionary<int, List<ContourLine>>
        {
            public List<ContourLine> GetOrCreateElevation(int level)
            {
                if (!TryGetValue(level, out var lines))
                {
                    lines = new List<ContourLine>();
                    Add(level, lines);
                }
                return lines;
            }
        }

        private readonly double thresholdSqared = Coordinates.DefaultThresholdSquared;

        private readonly LinesByElevation linesByLevel = new LinesByElevation();

        public int Count => linesByLevel.Values.Sum(l => l.Count);

        public IEnumerable<ContourLine> Lines => linesByLevel.Values.SelectMany(l => l);

        private LinesByElevation AddSegments(IEnumerable<ContourSegment> segments, LinesByElevation prevScan)
        {
            var currentScan = new HashSet<ContourLine>();
            var currentScanIndex = new LinesByElevation();
            ContourLine prevLine;
            foreach (var segment in segments)
            {
                if (!segment.Point1.AlmostEquals(segment.Point2, thresholdSqared)) // filters bad segments
                {
                    if (currentScan.Add(prevLine = AddSegment(segment, prevScan, currentScanIndex)))
                    {
                        currentScanIndex.GetOrCreateElevation((int)prevLine.Level).Add(prevLine);
                    }
                }
            }
            return currentScanIndex;
        }

        private ContourLine AddSegment(ContourSegment segment, LinesByElevation prevScan, LinesByElevation currentScan)
        {
            if (currentScan.TryGetValue((int)segment.Level, out var currentLines))
            {
                var prev = currentLines[currentLines.Count - 1];
                if (prev.TryAdd(segment))
                {
                    if (prevScan.TryGetValue((int)segment.Level, out var prevLinesX))
                    {
                        foreach (var line in prevLinesX)
                        {
                            if (line != prev && line.TryMerge(prev, thresholdSqared))
                            {
                                return line;
                            }
                        }
                    }
                    return prev;
                }
            }
            if (prevScan.TryGetValue((int)segment.Level, out var prevLines))
            {
                foreach (var line in prevLines)
                {
                    if (line.TryAdd(segment, thresholdSqared))
                    {
                        // XXX: segment may merge with an other line
                        return line;
                    }
                }
            }
            var newLine = new ContourLine(segment);
            linesByLevel.GetOrCreateElevation((int)segment.Level).Add(newLine);
            return newLine;
        }

        public void Add(IDemDataCell cell, ContourLevelGenerator generator, IProgress<double>? progress = null)
        {
            var prevScan = new LinesByElevation();
            var segments = new List<ContourSegment>();

            for (int lat = 0; lat < cell.PointsLat - 1; lat++)
            {
                DemDataPoint? southWest = null;
                DemDataPoint? northWest = null;
                segments.Clear();
                foreach (var point in cell.GetScanLine(lat, 0, cell.PointsLon).Zip(cell.GetScanLine(lat + 1, 0, cell.PointsLon), (south, north) => new { south, north }))
                {
                    var southEast = point.south;
                    var northEast = point.north;
                    if (southWest != null && northWest != null)
                    {
                        segments.AddRange(new ContourSquare(northWest, southWest, southEast, northEast).Segments(generator));
                    }
                    southWest = southEast;
                    northWest = northEast;
                }

                prevScan = AddSegments(segments, prevScan);

                progress?.Report((double)lat / (cell.PointsLat - 1) * 100d);
            }
            Cleanup();
            progress?.Report(100d);
        }

        public void Cleanup()
        {
            Parallel.ForEach(linesByLevel.Values, lines =>
            {
                lines.RemoveAll(l => l.IsDiscarded && !l.IsSinglePoint);
            });
        }

        public void Simplify(IProgress<double>? progress = null)
        {
            Cleanup();

            var initialCount = Count;
            var done = 0;
            Parallel.ForEach(linesByLevel.Values, lines =>
            {
                var initial = lines.Count;
                var toKeepAsIs = lines.Where(l => l.IsClosed && !l.IsDiscarded).ToArray();
                var toAnalyse = lines.Where(l => !l.IsClosed).ToArray();
                for(var i = 0; i < toAnalyse.Length; ++i)
                {
                    var a = toAnalyse[i];
                    if (!a.IsClosed)
                    {
                        for (var j = 0; j < toAnalyse.Length; ++j)
                        {
                            if (i != j)
                            {
                                var b = toAnalyse[j];
                                if (!b.IsClosed)
                                {
                                    a.TryMerge(b, thresholdSqared);
                                }
                            }
                        }
                    }
                }
                lines.Clear();
                lines.AddRange(toKeepAsIs);
                lines.AddRange(toAnalyse.Where(a => !a.IsDiscarded));
                var total = Interlocked.Add(ref done, initial);
                progress?.Report((double)total / initialCount * 100d);
            });
            progress?.Report(100d);
        }

        public IEnumerable<Feature> ToGeoJsonFeatures(int rounding = -1)
        {
            if (rounding == -1)
            {
                return Lines.Select(l => new Feature(new LineString(l.Points), new Dictionary<string, object>() { { "level", (object)l.Level } }));
            }
            return Lines.Select(l => new Feature(new LineString(l.Points.Select(p => new Position(Math.Round(p.Latitude, rounding), Math.Round(p.Longitude, rounding))).ToList()), new Dictionary<string, object>() { { "level", (object)l.Level} }));
        }



    }
}

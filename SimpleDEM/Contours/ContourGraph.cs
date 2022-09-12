using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClipperLib;
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

            var unknownHypothesis = AddSegments(segments.Where(s => !s.Point1.AlmostEquals(s.Point2, thresholdSqared)), prevScan, currentScan, currentScanIndex);

            if (unknownHypothesis.Count > 0)
            {
                var remain = AddSegments(unknownHypothesis, prevScan, currentScan, currentScanIndex);
                var remain2 = AddSegments(remain, prevScan, currentScan, currentScanIndex);
                var remain3 = AddSegments(remain2, prevScan, currentScan, currentScanIndex);

                if (remain3.Count > 0)
                {
                    // Give up, keep first hypothesis
                    // might create some issues in generated graph
                    foreach (var x in remain3)
                    {
                        if (x.IsValidHypothesis)
                        {
                            x.ValidateHypothesis();
                        }
                    }
                    AddSegments(remain3, prevScan, currentScan, currentScanIndex);
                }
            }
            return currentScanIndex;
        }

        private List<ContourSegment> AddSegments(IEnumerable<ContourSegment> segments, LinesByElevation prevScan, HashSet<ContourLine> currentScan, LinesByElevation currentScanIndex)
        {
            var unknownHypothesis = new List<ContourSegment>();
            foreach (var segment in segments)
            {
                if (segment.IsValidHypothesis)
                {
                    var prevLine = AddSegment(segment, prevScan, currentScanIndex);
                    if (prevLine == null)
                    {
                        unknownHypothesis.Add(segment);
                    }
                    else if (currentScan.Add(prevLine))
                    {
                        currentScanIndex.GetOrCreateElevation((int)prevLine.Level).Add(prevLine);
                    }
                }
            }
            return unknownHypothesis;
        }

        private ContourLine? AddSegment(ContourSegment segment, LinesByElevation previousParallel, LinesByElevation currentParallel)
        {
            var segmentKey = (int)segment.Level;

            if (currentParallel.TryGetValue(segmentKey, out var currentLines))
            {
                var lastest = currentLines.AsEnumerable().Reverse()/*.Take(5)*/;
                foreach (var prev in lastest)
                {
                    if (prev.TryAdd(segment))
                    {
                        var merged = Merge(prev, lastest);
                        if (previousParallel.TryGetValue(segmentKey, out var prevLinesX))
                        {
                            return Merge(merged, prevLinesX);
                        }
                        return merged;
                    }
                }
            }
            if (previousParallel.TryGetValue(segmentKey, out var prevLines))
            {
                foreach (var line in prevLines)
                {
                    if (line.TryAdd(segment, thresholdSqared))
                    {
                        return Merge(line, prevLines);
                    }
                }
            }
            if (segment.IsHypothesis)
            {
                return null;
            }
            var newLine = new ContourLine(segment);
            linesByLevel.GetOrCreateElevation((int)segment.Level).Add(newLine);
            return newLine;
        }

        private ContourLine Merge(ContourLine editedLine, IEnumerable<ContourLine> parallel)
        {
            foreach (var line in parallel)
            {
                if (line != editedLine && line.TryMerge(editedLine, thresholdSqared))
                {
                    return line;
                }
            }
            return editedLine;
        }

        public void Add(IDemDataView cell, IContourLevelGenerator generator, IProgress<double>? progress = null)
        {
            var prevScan = new LinesByElevation();
            var segments = new List<ContourSegment>();

            for (int lat = 0; lat < cell.PointsLat - 1; lat++)
            {
                DemDataPoint? southWest = null;
                DemDataPoint? northWest = null;
                segments.Clear();
                foreach (var point in cell.GetPointsOnParallel(lat, 0, cell.PointsLon).Zip(cell.GetPointsOnParallel(lat + 1, 0, cell.PointsLon), (south, north) => new { south, north }))
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
            var merged = 0;
            Parallel.ForEach(linesByLevel.Values, lines =>
            {
                var initial = lines.Count;
                var toKeepAsIs = lines.Where(l => l.IsClosed && !l.IsDiscarded).ToArray();
                var toAnalyse = lines.Where(l => !l.IsClosed).ToArray();
                for (var i = 0; i < toAnalyse.Length; ++i)
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
                                    if (a.TryMerge(b, thresholdSqared))
                                    {
                                        Interlocked.Increment(ref merged);
                                    }
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
            Console.WriteLine("Merged => " + merged);
        }

        public IEnumerable<Polygon> ToPolygons(int rounding = -1, IProgress<double>? progress = null)
        {
            return linesByLevel.SelectMany(l => ToPolygons(l.Value, rounding, progress)).ToList();
        }

        private IEnumerable<Polygon> ToPolygons(List<ContourLine> value, int rounding, IProgress<double>? progress)
        {
            var clipper = new Clipper(progress);
            foreach (var line in value)
            {
                clipper.AddPath(line.Points.Select(p => p.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctXor, result);
            return result.Childs
                .Select(c => new Polygon((new[] { ToLineString(c, rounding) })
                             .Concat(c.Childs.Select(h => ToLineString(h, rounding))))).ToList();
        }

        private static LineString ToLineString(PolyNode c, int rounding)
        {
            var points = c.Contour.Select(c => new Coordinates(c, rounding));
            return new LineString(points.Concat(points.Take(1)).ToList());
        }
    }
}

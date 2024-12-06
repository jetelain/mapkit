using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;
using Pmad.Geometry;
using Pmad.Geometry.Collections;
using Pmad.Geometry.Shapes;

namespace Pmad.Cartography.Contours
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

        private readonly double thresholdSquared;

        private readonly LinesByElevation linesByLevel = new LinesByElevation();

        private readonly List<VectorEnvelope<Vector2D>> squares = new ();

        public int Count => linesByLevel.Values.Sum(l => l.Count);

        public IEnumerable<ContourLine> Lines => linesByLevel.Values.SelectMany(l => l);

        private readonly ShapeSettings<double,Vector2D> shapeSettings;

        public ContourGraph()
            : this(Coordinates.LatLonSettings)
        {

        }

        public ContourGraph(ShapeSettings<double, Vector2D> shapeSettings)
        {
            this.thresholdSquared = shapeSettings.NegligibleDistanceSquared;
            this.shapeSettings = shapeSettings;
        }

        private LinesByElevation AddSegments(IEnumerable<ContourSegment> segments, LinesByElevation prevScan)
        {
            var currentScan = new HashSet<ContourLine>();
            var currentScanIndex = new LinesByElevation();

            var unknownHypothesis = AddSegments(segments.Where(s => !s.Point1.AlmostEquals(s.Point2, thresholdSquared)), prevScan, currentScan, currentScanIndex);

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
                    if (prev.TryAdd(segment, thresholdSquared))
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
                    if (line.TryAdd(segment, thresholdSquared))
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
                if (line != editedLine && line.TryMerge(editedLine, thresholdSquared))
                {
                    return line;
                }
            }
            return editedLine;
        }

        public void Add(IDemDataView cell, IContourLevelGenerator generator, bool closeLines = false, IProgress<double>? progress = null)
        {
            squares.Add(new(cell.Start.Vector2D, cell.End.Vector2D));

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
#if DEBUG
            Simplify();
#endif
            if (closeLines)
            {
                CloseLines(cell);
            }
        }

        private void CloseLines(IDemDataView cell)
        {
            var edgeSW = cell.GetPointsOnParallel(0, 0, 1).First().CoordinatesS;
            var edgeNE = cell.GetPointsOnParallel(cell.PointsLat - 1, cell.PointsLon - 1, 1).First().CoordinatesS;
            CloseLines(Lines, edgeSW, edgeNE);
            Cleanup();
        }

        public void Cleanup()
        {
            Parallel.ForEach(linesByLevel.Values, lines =>
            {
                lines.RemoveAll(l => l.IsDiscarded && !l.IsSinglePoint);
            });
        }

#if DEBUG
        private void Simplify()
        {
            // Intened only to check for errors in previous algorithm results

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
                                    if (a.TryMerge(b, thresholdSquared))
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
            });
            if (merged > 0)
            {
                Console.WriteLine("Merged => " + merged);
            }
        }
#endif

        public IEnumerable<Polygon<double, Vector2D>> ToPolygons(IProgress<double>? progress = null)
        {
            return linesByLevel.SelectMany(l => ToPolygons(l.Value, progress)).ToList();
        }

        private IEnumerable<Polygon<double, Vector2D>> ToPolygons(List<ContourLine> value, IProgress<double>? progress)
        {
            var outer = value.Where(c => c.IsClosed && c.Points.Count > 3 && c.IsCounterClockWise).ToList();
            var inner = value.Where(c => c.IsClosed && c.Points.Count > 3 && !c.IsCounterClockWise).ToList();
            return ToPoylgons(progress, outer, inner);
        }

        public IEnumerable<Polygon<double, Vector2D>> ToPolygonsReverse(IProgress<double>? progress = null)
        {
            return linesByLevel.SelectMany(l => ToPolygonsReverse(l.Value, progress)).ToList();
        }

        private IEnumerable<Polygon<double, Vector2D>> ToPolygonsReverse(List<ContourLine> value, IProgress<double>? progress)
        {
            var outer = value.Where(c => c.IsClosed && c.Points.Count > 3 && !c.IsCounterClockWise).ToList();
            var inner = value.Where(c => c.IsClosed && c.Points.Count > 3 && c.IsCounterClockWise).ToList();
            return ToPoylgons(progress, outer, inner);
        }

        private IEnumerable<Polygon<double, Vector2D>> ToPoylgons(IProgress<double>? progress, List<ContourLine> outer, List<ContourLine> inner)
        {
            var poly = new List<Polygon<double,Vector2D>>();
            var i = 0;
            foreach (var o in outer)
            {
                var minMax = VectorEnvelope<Vector2D>.FromList(o.Points.AsSpan<CoordinatesValue, Vector2D>());
                var holes = NonOverlaping(inner.Where(i => minMax.Contains(i.Points[0].Vector2D) && o.IsPointInsideOrOnBoundary(i.Points[0])).ToList());
                inner.RemoveAll(holes.Contains);
                poly.Add(new Polygon<double, Vector2D>(shapeSettings, ToLineString2(o), holes.Select(ToLineString2).ToReadOnlyArray()));
                i++;
                if (i % 100 == 0)
                {
                    progress?.Report(i * 100.0 / outer.Count);
                }
            }
            if (inner.Count > 0)
            {
                if (squares.Count == 1) // Trivial case
                {
                    var square = squares[0];
                    var all = shapeSettings.CreateRectanglePolygon(square);
                    foreach (var result in all.SubstractAll(NonOverlaping(inner).Select(l => new Polygon<double, Vector2D>(shapeSettings, ToLineString2(l)))))
                    {
                        poly.Add(result);
                    }
                }
                else
                {
                    throw new NotImplementedException("TODO");
                }
            }
            progress?.Report(100.0);
            return poly;
        }

        private List<ContourLine> NonOverlaping(List<ContourLine> holes)
        {
            if (holes.Count < 2)
            {
                return holes;
            }
            return holes.Where(h1 => !holes.Any(h2 => h2 != h1 && h1.IsPointInside(h2.Points[0]))).ToList();
        }

        private static ReadOnlyArray<Vector2D> ToLineString2(ContourLine o)
        {
            var result = new ReadOnlyArrayBuilder<Vector2D>(o.Points.Count);
            result.AddRange(o.Points.AsSpan<CoordinatesValue,Vector2D>().Slice(0, o.Points.Count-1));
            result.Add(o.Points[0].Vector2D); // Ensures that 1st and last are exactly the same (ContourLine has approximative match)
            return result.Build();
        }

        private void CloseLines(IEnumerable<ContourLine> value, CoordinatesValue edgeSW, CoordinatesValue edgeNE)
        {
            var notClosed = value.Where(l => !l.IsClosed).ToList();
            if (notClosed.Count == 0)
            {
                return;
            }
            foreach (var line in notClosed)
            {
                if (line.IsClosed)
                {
                    continue;
                }
                if (line.First.Latitude == edgeNE.Latitude) // First On North, look EAST
                {
                    LookEast(edgeSW, edgeNE, notClosed, line, line.First);
                }
                else if (line.First.Longitude == edgeNE.Longitude) // First On East, look SOUTH
                {
                    LookSouth(edgeSW, edgeNE, notClosed, line, line.First);
                }
                else if (line.First.Latitude == edgeSW.Latitude) // First On South, look WEST
                {
                    LookWest(edgeSW, edgeNE, notClosed, line, line.First);
                }
                else if (line.First.Longitude == edgeSW.Longitude) // First On West, look North
                {
                    LookNorth(edgeSW, edgeNE, notClosed, line, line.First);
                }
            }
        }

        private void LookEast(CoordinatesValue edgeSW, CoordinatesValue edgeNE, List<ContourLine> notClosed, ContourLine line, CoordinatesValue lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.Last.Latitude == edgeNE.Latitude && n.Last.Longitude > lookFrom.Longitude)
                .OrderBy(n => n.Last.Longitude)
                .FirstOrDefault();
            if (other != null)
            {
                other.Append(line, thresholdSquared);
            }
            else if ( depth < 4 )
            {
                line.Points.Prepend(edgeNE);
                LookSouth(edgeSW, edgeNE, notClosed, line, edgeNE, depth + 1);
            }
        }

        private void LookSouth(CoordinatesValue edgeSW, CoordinatesValue edgeNE, List<ContourLine> notClosed, ContourLine line, CoordinatesValue lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.Last.Longitude == edgeNE.Longitude && n.Last.Latitude < lookFrom.Latitude)
                .OrderByDescending(n => n.Last.Latitude)
                .FirstOrDefault();
            if (other != null)
            {
                other.Append(line, thresholdSquared);
            }
            else if (depth < 4)
            {
                var southEast = new CoordinatesValue(edgeSW.Latitude, edgeNE.Longitude);
                line.Points.Prepend(southEast);
                LookWest(edgeSW, edgeNE, notClosed, line, southEast, depth + 1);
            }
        }

        private void LookWest(CoordinatesValue edgeSW, CoordinatesValue edgeNE, List<ContourLine> notClosed, ContourLine line, CoordinatesValue lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.Last.Latitude == edgeSW.Latitude && n.Last.Longitude < lookFrom.Longitude)
                .OrderByDescending(n => n.Last.Longitude)
                .FirstOrDefault();
            if (other != null)
            {
                other.Append(line, thresholdSquared);
            }
            else if (depth < 4)
            {
                line.Points.Prepend(edgeSW);
                LookNorth(edgeSW, edgeNE, notClosed, line, edgeSW, depth + 1);
            }
        }

        private void LookNorth(CoordinatesValue edgeSW, CoordinatesValue edgeNE, List<ContourLine> notClosed, ContourLine line, CoordinatesValue lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.Last.Longitude == edgeSW.Longitude && n.Last.Latitude > lookFrom.Latitude)
                .OrderBy(n => n.Last.Latitude)
                .FirstOrDefault();
            if (other != null)
            {
                other.Append(line, thresholdSquared);
            }
            else if (depth < 4)
            {
                var northWest = new CoordinatesValue(edgeNE.Latitude, edgeSW.Longitude);
                line.Points.Prepend(northWest);
                LookEast(edgeSW, edgeNE, notClosed, line, northWest, depth + 1);
            }
        }
    }
}

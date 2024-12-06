using MapToolkit.Contours;
using MapToolkit.DataCells;
using MapToolkit.GeodeticSystems;
using MapToolkit.Hillshading;
using Pmad.Geometry;
using Pmad.Geometry.Collections;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.Drawing.Topographic
{
    public sealed class TopoMapRenderData
    {
        private TopoMapRenderData(ITopoMapData data, Image<La16> img, ContourGraph contour, List<DemDataPoint> plotted)
        {
            Data = data;
            Img = img;
            Contour = contour;
            PlottedPoints = plotted;
        }

        public ITopoMapData Data { get; }

        public Image<La16> Img { get; }

        public ContourGraph Contour { get; }

        public List<DemDataPoint> PlottedPoints { get; }

        public double WidthInMeters => End.Longitude - Start.Longitude;

        public double HeightInMeters => End.Latitude - Start.Latitude;

        public Coordinates End => Data.DemDataCell.End;

        public Coordinates Start => Data.DemDataCell.Start;

        public static TopoMapRenderData Create(ITopoMapData data, IProgressScope scope)
        {
            var img = new HillshaderFast(new Vector(10, 10)).GetPixelsAlphaBelowFlat(data.DemDataCell);
            
            var contour = new ContourGraph();
            using (var report = scope.CreatePercent("Contours"))
            {
                contour.Add(data.DemDataCell, new ContourLevelGenerator(10, 10), false, report);
            }
            
            var plotted = data.PlottedPoints ?? ComputePlottedPoints(data.DemDataCell, contour, scope);

            return new TopoMapRenderData(data, img, contour, plotted);
        }

        internal static List<DemDataPoint> ComputePlottedPoints(IDemDataView demView, ContourGraph contour, IProgressScope scope)
        {
            var lines = contour.Lines.Where(l => l.IsClosed && IsValidForMaximaMinima(l)).ToList();
            var plotted = scope.TrackPercent("Maxima", maxima => ContourMaximaMinima.FindMaxima(demView, lines, maxima));
            plotted.AddRange(scope.TrackPercent("Minima", minima => ContourMaximaMinima.FindMinima(demView, lines, minima)));
            return plotted;
        }

        private static bool IsValidForMaximaMinima(ContourLine l)
        {
            var length = LengthInMeters(l.Points);
            return length > 200 && length < 100_000;
        }

        private static double LengthInMeters(ReadOnlyArrayBuilder<CoordinatesValue> points)
        {
            return points.AsSpan<CoordinatesValue, Vector2D>().GetLengthD();

            //return points.Take(points.Count - 1).Zip(points.Skip(1), (a, b) => MeterProjectedDistance.Instance.DistanceInMeters(a, b)).Sum();
        }
    }
}

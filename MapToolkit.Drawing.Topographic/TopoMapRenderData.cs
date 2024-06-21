using MapToolkit.Contours;
using MapToolkit.DataCells;
using MapToolkit.GeodeticSystems;
using MapToolkit.Hillshading;
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

        public static TopoMapRenderData Create(ITopoMapData data)
        {
            var img = new HillshaderFast(new Vector(10, 10)).GetPixelsAlphaBelowFlat(data.DemDataCell);
            
            var contour = new ContourGraph();
            contour.Add(data.DemDataCell, new ContourLevelGenerator(10, 10), false);
            
            var plotted = data.PlottedPoints ?? ComputePlottedPoints(data.DemDataCell, contour);

            return new TopoMapRenderData(data, img, contour, plotted);
        }

        internal static List<DemDataPoint> ComputePlottedPoints(IDemDataView demView, ContourGraph contour)
        {
            var lines = contour.Lines.Where(l => l.IsClosed && LengthInMeters(l.Points) > 200).ToList();
            var plotted = ContourMaximaMinima.FindMaxima(demView, lines);
            plotted.AddRange(ContourMaximaMinima.FindMinima(demView, lines));
            return plotted;
        }

        private static double LengthInMeters(List<Coordinates> points)
        {
            return points.Take(points.Count - 1).Zip(points.Skip(1), (a, b) => MeterProjectedDistance.Instance.DistanceInMeters(a, b)).Sum();
        }
    }
}

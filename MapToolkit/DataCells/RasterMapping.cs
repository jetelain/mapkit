using System;

namespace MapToolkit.DataCells
{
    public abstract class RasterMapping
    {
        private protected RasterMapping(Coordinates start, Coordinates end, int pointsLat, int pointsLon)
        {
            Start = start;
            End = end;
            SizeLat = End.Latitude - Start.Latitude;
            SizeLon = End.Longitude - Start.Longitude;
            PointsLat = pointsLat;
            PointsLon = pointsLon;
        }

        public static RasterMapping Create(DemRasterType type, Coordinates start, Coordinates end, int pointsLat, int pointsLon)
        {
            switch (type)
            {
                default:
                case DemRasterType.Unknown:
                case DemRasterType.PixelIsArea:
                    return new RasterPixelIsArea(start, end, pointsLat, pointsLon);
                case DemRasterType.PixelIsPoint:
                    return new RasterPixelIsPoint(start, end, pointsLat, pointsLon);
            }
        }

        public static RasterMapping Create(DemRasterType type, Coordinates start, Coordinates end, double pixelSizeLat, double pixelSizeLon)
        {
            switch (type)
            {
                default:
                case DemRasterType.Unknown:
                case DemRasterType.PixelIsArea:
                    return new RasterPixelIsArea(start, end, pixelSizeLat, pixelSizeLon);
                case DemRasterType.PixelIsPoint:
                    return new RasterPixelIsPoint(start, end, pixelSizeLat, pixelSizeLon);
            }
        }

        public Coordinates Start { get; }

        public Coordinates End { get; }

        public double SizeLat { get; }

        public double SizeLon { get; }

        public int PointsLat { get; }

        public int PointsLon { get; }

        public abstract DemRasterType RasterType { get; }

        public abstract double PixelSizeLat { get; }

        public abstract double PixelSizeLon { get; }

        public Coordinates PinToGridCeiling(Coordinates coordinates)
        {
            return new Coordinates(
                Start.Latitude + (Math.Ceiling((coordinates.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Ceiling((coordinates.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon));
        }

        public Coordinates PinToGridFloor(Coordinates coordinates)
        {
            return new Coordinates(
                Start.Latitude + (Math.Floor((coordinates.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Floor((coordinates.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon));
        }

        public RasterMapping Crop(Coordinates wantedStart, Coordinates wantedEnd)
        {
            return Create(RasterType, PinToGridFloor(wantedStart).Round(12), PinToGridCeiling(wantedEnd).Round(12), PixelSizeLat, PixelSizeLon);
        }

        internal abstract CellCoordinates CoordinatesToIndexClosest(Coordinates coordinates);

        internal abstract Coordinates IndexToCoordinates(int lat, int lon);
    }
}

using System;

namespace Pmad.Cartography.DataCells
{
    public sealed class RasterPixelIsPoint : RasterMapping
    {
        internal RasterPixelIsPoint(Coordinates start, Coordinates end, int pointsLat, int pointsLon)
           : base(start, end, pointsLat, pointsLon)
        {
            PixelSizeLat = SizeLat / (PointsLat - 1);
            PixelSizeLon = SizeLon / (PointsLon - 1);
        }
        internal RasterPixelIsPoint(Coordinates start, Coordinates end, double pixelSizeLat, double pixelSizeLon)
           : base(start, end, (int)Math.Round((end.Latitude - start.Latitude) / pixelSizeLat) + 1, (int)Math.Round((end.Longitude - start.Longitude) / pixelSizeLon) + 1)
        {
            PixelSizeLat = pixelSizeLat;
            PixelSizeLon = pixelSizeLon;
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsPoint;

        public override double PixelSizeLat { get; }

        public override double PixelSizeLon { get; }

        internal override CellCoordinates CoordinatesToIndexClosest(Coordinates coordinates)
        {
            return new CellCoordinates(
                    (int)Math.Round((coordinates.Latitude - Start.Latitude) / PixelSizeLat),
                    (int)Math.Round((coordinates.Longitude - Start.Longitude) / PixelSizeLon)
                );
        }

        internal override Coordinates IndexToCoordinates(int lat, int lon)
        {
            return new Coordinates(Start.Latitude + (lat * PixelSizeLat), Start.Longitude + (lon * PixelSizeLon));
        }
    }
}

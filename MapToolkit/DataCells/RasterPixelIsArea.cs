using System;

namespace Pmad.Cartography.DataCells
{
    public sealed class RasterPixelIsArea : RasterMapping
    {
        internal RasterPixelIsArea(Coordinates start, Coordinates end, int pointsLat, int pointsLon)
           : base(start, end, pointsLat, pointsLon)
        {
            PixelSizeLat = SizeLat / PointsLat;
            PixelSizeLon = SizeLon / PointsLon;
            StartAsPoint = new Coordinates(Start.Latitude + PixelSizeLat / 2, Start.Longitude + PixelSizeLon / 2);
        }

        internal RasterPixelIsArea(Coordinates start, Coordinates end, double pixelSizeLat, double pixelSizeLon)
            : this(start, end, (int)Math.Round((end.Latitude - start.Latitude) / pixelSizeLat), (int)Math.Round((end.Longitude - start.Longitude) / pixelSizeLon))
        {
            PixelSizeLat = pixelSizeLat;
            PixelSizeLon = pixelSizeLon;
            StartAsPoint = new Coordinates(Start.Latitude + PixelSizeLat / 2, Start.Longitude + PixelSizeLon / 2);
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsArea;

        public override double PixelSizeLat { get; }

        public override double PixelSizeLon { get; }

        public Coordinates StartAsPoint { get; }

        internal override CellCoordinates CoordinatesToIndexClosest(Coordinates coordinates)
        {
            return new CellCoordinates(
                (int)Math.Floor((coordinates.Latitude - StartAsPoint.Latitude) / PixelSizeLat) + 1,
                (int)Math.Floor((coordinates.Longitude - StartAsPoint.Longitude) / PixelSizeLon) + 1);
        }

        internal override Coordinates IndexToCoordinates(int lat, int lon)
        {
            return new Coordinates(StartAsPoint.Latitude + (lat * PixelSizeLat), StartAsPoint.Longitude + (lon * PixelSizeLon));
        }
    }
}

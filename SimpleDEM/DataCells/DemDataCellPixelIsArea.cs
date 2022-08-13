using System.Collections.Generic;

namespace SimpleDEM.DataCells
{
    public class DemDataCellPixelIsArea<T>
        : DemDataCellBase<T> where T : unmanaged
    {
        private readonly DemDataCellPixelIsPoint<T> points;

        public DemDataCellPixelIsArea(GeodeticCoordinates start, GeodeticCoordinates end, T[,] data)
            : base(start, end, data)
        {
            PixelSizeLat = SizeLat / PointsPerCellLat;
            PixelSizeLon = SizeLon / PointsPerCellLon;
            points = ToPixelIsPoint();
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsArea;

        public override double PixelSizeLat { get; }

        public override double PixelSizeLon { get; }

        public override T GetRawElevation(GeodeticCoordinates coordinates)
        {
            var relLat = (int)((coordinates.Latitude - Start.Latitude) / SizeLat * PointsPerCellLat);
            var relLon = (int)((coordinates.Longitude - Start.Longitude) / SizeLon * PointsPerCellLon);
            return Data[relLat, relLon];
        }

        public DemDataCellPixelIsArea<U> ToType<U>() where U : unmanaged
        {
            return new DemDataCellPixelIsArea<U>(Start, End, ConvertData<U>());
        }

        public DemDataCellPixelIsPoint<T> ToPixelIsPoint()
        {
            return new DemDataCellPixelIsPoint<T>(
                new GeodeticCoordinates(Start.Latitude + PixelSizeLat / 2, Start.Longitude + PixelSizeLon / 2),
                new GeodeticCoordinates(End.Latitude - PixelSizeLat / 2, End.Longitude - PixelSizeLon / 2),
                Data);
        }

        public override IEnumerable<DemDataPoint> GetNearbyElevation(GeodeticCoordinates coordinates)
        {
            return points.GetNearbyElevation(coordinates);
        }

        public override double GetLocalElevation(GeodeticCoordinates coordinates, IInterpolation interpolation)
        {
            return points.GetLocalElevation(coordinates, interpolation);
        }

        public override bool IsLocal(GeodeticCoordinates coordinates)
        {
            return points.IsLocal(coordinates);
        }
    }
}

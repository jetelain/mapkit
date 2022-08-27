using System;
using System.Collections.Generic;

namespace SimpleDEM.DataCells
{
    public sealed class DemDataCellPixelIsArea<T>
        : DemDataCellBase<T> where T : unmanaged
    {
        private readonly DemDataCellPixelIsPoint<T> points;

        public DemDataCellPixelIsArea(Coordinates start, Coordinates end, T[,] data)
            : base(start, end, data)
        {
            PixelSizeLat = SizeLat / PointsLat;
            PixelSizeLon = SizeLon / PointsLon;
            points = AsPixelIsPoint();
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsArea;

        public override double PixelSizeLat { get; }

        public override double PixelSizeLon { get; }

        public override T GetRawElevation(Coordinates coordinates)
        {
            var relLat = (int)((coordinates.Latitude - Start.Latitude) / SizeLat * PointsLat);
            var relLon = (int)((coordinates.Longitude - Start.Longitude) / SizeLon * PointsLon);
            return Data[relLat, relLon];
        }

        public DemDataCellPixelIsArea<U> ToType<U>() where U : unmanaged
        {
            return new DemDataCellPixelIsArea<U>(Start, End, ConvertData<U>());
        }

        public override DemDataCellPixelIsPoint<T> AsPixelIsPoint()
        {
            return new DemDataCellPixelIsPoint<T>(
                new Coordinates(Start.Latitude + PixelSizeLat / 2, Start.Longitude + PixelSizeLon / 2),
                new Coordinates(End.Latitude - PixelSizeLat / 2, End.Longitude - PixelSizeLon / 2),
                Data);
        }

        public override DemDataCellPixelIsArea<T> AsPixelIsArea()
        {
            return this;
        }


        public override IEnumerable<DemDataPoint> GetNearbyElevation(Coordinates coordinates)
        {
            return points.GetNearbyElevation(coordinates);
        }

        public override double GetLocalElevation(Coordinates coordinates, IInterpolation interpolation)
        {
            return points.GetLocalElevation(coordinates, interpolation);
        }

        public override bool IsLocal(Coordinates coordinates)
        {
            return points.IsLocal(coordinates);
        }
        protected override DemDataCellBase<T> CropExact(Coordinates realStart, Coordinates realEnd)
        {
            var startRelLat = (int)((realStart.Latitude - Start.Latitude) / SizeLat * PointsLat);
            var startRelLon = (int)((realStart.Longitude - Start.Longitude) / SizeLon * PointsLon);
            var endRelLat = (int)Math.Floor(((realEnd.Latitude - Start.Latitude) / SizeLat * PointsLat));
            var endRelLon = (int)Math.Floor(((realEnd.Longitude - Start.Longitude) / SizeLon * PointsLon));

            return new DemDataCellPixelIsArea<T>(realStart, realEnd, CropData(startRelLat, startRelLon, endRelLat - startRelLat, endRelLon - startRelLon));
        }

        public DemDataCellPixelIsArea<T> Downsample(int factor)
        {
            var newPointsLat = PointsLat / factor;
            var newPointsLon = PointsLon / factor;

            var newData = new T[newPointsLat, newPointsLon];
            var samples = new T[factor * factor];

            DownsampleCore(factor, newPointsLat, newPointsLon, newData, samples);

            return new DemDataCellPixelIsArea<T>(Start, End, newData);
        }

        internal override U Accept<U>(IDemDataCellVisitor<U> visitor)
        {
            return PixelFormat.Accept(visitor, this);
        }
    }
}

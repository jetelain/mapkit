using System;
using System.Collections.Generic;

namespace MapToolkit.DataCells
{
    public sealed class DemDataCellPixelIsArea<TPixel>
        : DemDataCellBase<TPixel> where TPixel : unmanaged
    {
        private readonly DemDataCellPixelIsPoint<TPixel> points;

        public DemDataCellPixelIsArea(Coordinates start, Coordinates end, TPixel[,] data)
            : base(start, end, data)
        {
            PixelSizeLat = SizeLat / PointsLat;
            PixelSizeLon = SizeLon / PointsLon;
            points = AsPixelIsPoint();
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsArea;

        public override double PixelSizeLat { get; }

        public override double PixelSizeLon { get; }

        public override TPixel GetRawElevation(Coordinates coordinates)
        {
            var relLat = (int)((coordinates.Latitude - Start.Latitude) / SizeLat * PointsLat);
            var relLon = (int)((coordinates.Longitude - Start.Longitude) / SizeLon * PointsLon);
            return Data[relLat, relLon];
        }

        public DemDataCellPixelIsArea<TOtherPixel> ConvertTo<TOtherPixel>() where TOtherPixel : unmanaged
        {
            return new DemDataCellPixelIsArea<TOtherPixel>(Start, End, ConvertData<TOtherPixel>());
        }

        public override DemDataCellPixelIsPoint<TPixel> AsPixelIsPoint()
        {
            return new DemDataCellPixelIsPoint<TPixel>(
                new Coordinates(Start.Latitude + PixelSizeLat / 2, Start.Longitude + PixelSizeLon / 2),
                new Coordinates(End.Latitude - PixelSizeLat / 2, End.Longitude - PixelSizeLon / 2),
                Data);
        }

        public override DemDataCellPixelIsArea<TPixel> AsPixelIsArea()
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
        protected override DemDataCellBase<TPixel> CropExact(Coordinates realStart, Coordinates realEnd)
        {
            var startRelLat = (int)((realStart.Latitude - Start.Latitude) / SizeLat * PointsLat);
            var startRelLon = (int)((realStart.Longitude - Start.Longitude) / SizeLon * PointsLon);
            var endRelLat = (int)Math.Floor(((realEnd.Latitude - Start.Latitude) / SizeLat * PointsLat));
            var endRelLon = (int)Math.Floor(((realEnd.Longitude - Start.Longitude) / SizeLon * PointsLon));

            return new DemDataCellPixelIsArea<TPixel>(realStart, realEnd, CropData(startRelLat, startRelLon, endRelLat - startRelLat, endRelLon - startRelLon));
        }

        public DemDataCellPixelIsArea<TPixel> Downsample(int factor)
        {
            var newPointsLat = PointsLat / factor;
            var newPointsLon = PointsLon / factor;

            var newData = new TPixel[newPointsLat, newPointsLon];
            var samples = new TPixel[factor * factor];

            DownsampleCore(factor, newPointsLat, newPointsLon, newData, samples);

            return new DemDataCellPixelIsArea<TPixel>(Start, End, newData);
        }

        internal override U Accept<U>(IDemDataCellVisitor<U> visitor)
        {
            return PixelFormat.Accept(visitor, this);
        }

        public override IEnumerable<DemDataPoint> GetPointsOnParallel(int lat, int startLon, int count)
        {
            var realStartLat = Start.Latitude + (PixelSizeLat / 2);
            var realStartLon = Start.Longitude + (PixelSizeLon / 2);
            var lonEnd = startLon + count;
            for (var lon = startLon; lon < lonEnd; ++lon)
            {
                yield return new DemDataPoint(
                    new Coordinates(realStartLat + (lat * PixelSizeLat),
                    realStartLon + (lon * PixelSizeLon)),
                    PixelFormat.ToDouble(Data[lat, lon]));
            }
        }

        public override DemDataCellBase<TOtherPixel> ConvertToBase<TOtherPixel>()
        {
            return ConvertTo<TOtherPixel>();
        }
    }
}

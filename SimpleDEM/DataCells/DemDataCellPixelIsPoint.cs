using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDEM.DataCells
{
    public class DemDataCellPixelIsPoint<T>
        : DemDataCellBase<T> where T : unmanaged
    {
        public DemDataCellPixelIsPoint(GeodeticCoordinates start, GeodeticCoordinates end, T[,] data)
            : base(start, end, data)
        {
            PixelSizeLat = SizeLat / (PointsPerCellLat - 1);
            PixelSizeLon = SizeLon / (PointsPerCellLon - 1);
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsPoint;
        public override double PixelSizeLat { get; }
        public override double PixelSizeLon { get; }

        public override T GetRawElevation(GeodeticCoordinates coordinates)
        {
            var relLat = (int)Math.Round((coordinates.Latitude - Start.Latitude) / SizeLat * (PointsPerCellLat - 1));
            var relLon = (int)Math.Round((coordinates.Longitude - Start.Longitude) / SizeLon * (PointsPerCellLon - 1));
            return Data[relLat, relLon];
        }

        public override double GetLocalElevation(GeodeticCoordinates coordinates, IInterpolation interpolation)
        {
            var relLat = (coordinates.Latitude - Start.Latitude) / SizeLat * (PointsPerCellLat - 1);
            var relLon = (coordinates.Longitude - Start.Longitude) / SizeLon * (PointsPerCellLon - 1);

            var relLat0 = (int)Math.Floor(relLat);
            var relLon0 = (int)Math.Floor(relLon);
            var relLat1 = (int)Math.Ceiling(relLat);
            var relLon1 = (int)Math.Ceiling(relLon);

            if (relLat0 < 0 || relLon0 < 0 || relLat1 >= PointsPerCellLat || relLon1 >= PointsPerCellLon)
            {
                return interpolation.Interpolate(coordinates, GetNearbyElevation(coordinates).ToList());
            }

            var f00 = ToDouble(Data[relLat0, relLon0]);
            var f10 = ToDouble(Data[relLat1, relLon0]);
            var f01 = ToDouble(Data[relLat0, relLon1]);
            var f11 = ToDouble(Data[relLat1, relLon1]);

            if (double.IsNaN(f00) || double.IsNaN(f10) || double.IsNaN(f01) || double.IsNaN(f11))
            {
                return interpolation.Interpolate(coordinates, GetNearbyElevation(coordinates).ToList());
            }

            return interpolation.Interpolate(f00, f10, f01, f11, relLat - relLat0, relLon - relLon0);
        }

        public override IEnumerable<DemDataPoint> GetNearbyElevation(GeodeticCoordinates coordinates)
        {
            var relLat = (coordinates.Latitude - Start.Latitude) / SizeLat * (PointsPerCellLat - 1);
            var relLon = (coordinates.Longitude - Start.Longitude) / SizeLon * (PointsPerCellLon - 1);

            var relLat0 = (int)Math.Floor(relLat);
            var relLon0 = (int)Math.Floor(relLon);
            var relLat1 = (int)Math.Ceiling(relLat);
            var relLon1 = (int)Math.Ceiling(relLon);

            if (relLat0 >= 0 && relLon0 >= 0)
            {
                var f00 = ToDouble(Data[relLat0, relLon0]);
                if (!double.IsNaN(f00))
                {
                    yield return new DemDataPoint(new GeodeticCoordinates(Start.Latitude + relLat0 * (PointsPerCellLat - 1), Start.Longitude + relLon0 * (PointsPerCellLon - 1)), f00);
                }
            }

            if (relLat1 < PointsPerCellLat && relLon0 >= 0)
            {
                var f10 = ToDouble(Data[relLat1, relLon0]);
                if (!double.IsNaN(f10))
                {
                    yield return new DemDataPoint(new GeodeticCoordinates(Start.Latitude + relLat1 * (PointsPerCellLat - 1), Start.Longitude + relLon0 * (PointsPerCellLon - 1)), f10);
                }
            }

            if (relLat0 >= 0 && relLon1 < PointsPerCellLon)
            {
                var f01 = ToDouble(Data[relLat0, relLon1]);
                if (!double.IsNaN(f01))
                {
                    yield return new DemDataPoint(new GeodeticCoordinates(Start.Latitude + relLat0 * (PointsPerCellLat - 1), Start.Longitude + relLon1 * (PointsPerCellLon - 1)), f01);
                }
            }

            if (relLat1 < PointsPerCellLat && relLon1 < PointsPerCellLon)
            {
                var f11 = ToDouble(Data[relLat1, relLon1]);
                if (!double.IsNaN(f11))
                {
                    yield return new DemDataPoint(new GeodeticCoordinates(Start.Latitude + relLat1 * (PointsPerCellLat - 1), Start.Longitude + relLon1 * (PointsPerCellLon - 1)), f11);
                }
            }
        }

        public DemDataCellPixelIsPoint<U> ToType<U>() where U : unmanaged
        {
            return new DemDataCellPixelIsPoint<U>(Start, End, ConvertData<U>());
        }

        public DemDataCellPixelIsArea<T> ToPixelIsArea()
        {
            return new DemDataCellPixelIsArea<T>(
                new GeodeticCoordinates(Start.Latitude - PixelSizeLat / 2, Start.Longitude - PixelSizeLon / 2),
                new GeodeticCoordinates(End.Latitude + PixelSizeLat / 2, End.Longitude + PixelSizeLon / 2),
                Data);
        }

        public override bool IsLocal(GeodeticCoordinates coordinates)
        {
            return coordinates.IsInSquare(Start, End);
        }
    }
}

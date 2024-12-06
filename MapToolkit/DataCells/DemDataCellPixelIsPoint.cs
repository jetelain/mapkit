using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pmad.Geometry;

namespace Pmad.Cartography.DataCells
{
    public sealed class DemDataCellPixelIsPoint<TPixel>
        : DemDataCellBase<TPixel> where TPixel : unmanaged
    {
        public DemDataCellPixelIsPoint(Coordinates start, Coordinates end, TPixel[,] data)
            : base(start, end, data)
        {
            PixelSizeLat = SizeLat / (PointsLat - 1);
            PixelSizeLon = SizeLon / (PointsLon - 1);
        }

        public DemDataCellPixelIsPoint(Coordinates start, Vector2D pixelSize, TPixel[,] data) // TODO: TEST !!!
            : this(start, start + (pixelSize * new Vector2D(data.GetLength(0) - 1, data.GetLength(1) - 1)), data)
        {
            Debug.Assert(PixelSizeLat == pixelSize.Y);
            Debug.Assert(PixelSizeLon == pixelSize.X);
        }

        public override DemRasterType RasterType => DemRasterType.PixelIsPoint;
        public override double PixelSizeLat { get; }
        public override double PixelSizeLon { get; }

        public override TPixel GetRawElevation(Coordinates coordinates)
        {
            var relLat = (int)Math.Round((coordinates.Latitude - Start.Latitude) / SizeLat * (PointsLat - 1));
            var relLon = (int)Math.Round((coordinates.Longitude - Start.Longitude) / SizeLon * (PointsLon - 1));
            return Data[relLat, relLon];
        }

        public override double GetLocalElevation(Coordinates coordinates, IInterpolation interpolation)
        {
            var relLat = (coordinates.Latitude - Start.Latitude) / SizeLat * (PointsLat - 1);
            var relLon = (coordinates.Longitude - Start.Longitude) / SizeLon * (PointsLon - 1);

            var relLat0 = (int)Math.Floor(relLat);
            var relLon0 = (int)Math.Floor(relLon);
            var relLat1 = (int)Math.Ceiling(relLat);
            var relLon1 = (int)Math.Ceiling(relLon);

            if (relLat0 < 0 || relLon0 < 0 || relLat1 >= PointsLat || relLon1 >= PointsLon)
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

        public override IEnumerable<DemDataPoint> GetNearbyElevation(Coordinates coordinates)
        {
            var relLat = (coordinates.Latitude - Start.Latitude) / SizeLat * (PointsLat - 1);
            var relLon = (coordinates.Longitude - Start.Longitude) / SizeLon * (PointsLon - 1);

            var relLat0 = (int)Math.Floor(relLat);
            var relLon0 = (int)Math.Floor(relLon);
            var relLat1 = (int)Math.Ceiling(relLat);
            var relLon1 = (int)Math.Ceiling(relLon);

            if (relLat0 >= PointsLat || relLon0 >= PointsLon || relLat1 < 0 || relLon1 < 0)
            {
                // Out of range
                yield break;
            }

            if (relLat0 >= 0 && relLon0 >= 0)
            {
                var f00 = ToDouble(Data[relLat0, relLon0]);
                if (!double.IsNaN(f00))
                {
                    yield return new DemDataPoint(new CoordinatesValue(Start.Latitude + (relLat0 * PixelSizeLat), Start.Longitude + (relLon0 * PixelSizeLon)), f00);
                }
            }

            if (relLat1 < PointsLat && relLon0 >= 0)
            {
                var f10 = ToDouble(Data[relLat1, relLon0]);
                if (!double.IsNaN(f10))
                {
                    yield return new DemDataPoint(new CoordinatesValue(Start.Latitude + (relLat1 * PixelSizeLat), Start.Longitude + (relLon0 * PixelSizeLon)), f10);
                }
            }

            if (relLat0 >= 0 && relLon1 < PointsLon)
            {
                var f01 = ToDouble(Data[relLat0, relLon1]);
                if (!double.IsNaN(f01))
                {
                    yield return new DemDataPoint(new CoordinatesValue(Start.Latitude + (relLat0 * PixelSizeLat), Start.Longitude + (relLon1 * PixelSizeLon)), f01);
                }
            }

            if (relLat1 < PointsLat && relLon1 < PointsLon)
            {
                var f11 = ToDouble(Data[relLat1, relLon1]);
                if (!double.IsNaN(f11))
                {
                    yield return new DemDataPoint(new CoordinatesValue(Start.Latitude + (relLat1 * PixelSizeLat), Start.Longitude + (relLon1 * PixelSizeLon)), f11);
                }
            }
        }

        public DemDataCellPixelIsPoint<TOtherPixel> ConvertTo<TOtherPixel>() where TOtherPixel : unmanaged
        {
            return new DemDataCellPixelIsPoint<TOtherPixel>(Start, End, ConvertData<TOtherPixel>());
        }

        public override DemDataCellPixelIsArea<TPixel> AsPixelIsArea()
        {
            return new DemDataCellPixelIsArea<TPixel>(
                new Coordinates(Start.Latitude - PixelSizeLat / 2, Start.Longitude - PixelSizeLon / 2),
                new Coordinates(End.Latitude + PixelSizeLat / 2, End.Longitude + PixelSizeLon / 2),
                Data);
        }

        public override DemDataCellPixelIsPoint<TPixel> AsPixelIsPoint()
        {
            return this;
        }


        protected override DemDataCellBase<TPixel> CropExact(Coordinates realStart, Coordinates realEnd)
        {
            var startRelLat = (int)Math.Round((realStart.Latitude - Start.Latitude) / SizeLat * (PointsLat - 1));
            var startRelLon = (int)Math.Round((realStart.Longitude - Start.Longitude) / SizeLon * (PointsLon - 1));
            var endRelLat = (int)Math.Round((realEnd.Latitude - Start.Latitude) / SizeLat * (PointsLat - 1));
            var endRelLon = (int)Math.Round((realEnd.Longitude - Start.Longitude) / SizeLon * (PointsLon - 1));

            return new DemDataCellPixelIsPoint<TPixel>(realStart, realEnd, CropData(startRelLat, startRelLon, endRelLat - startRelLat + 1, endRelLon - startRelLon + 1));
        }



        public override bool IsLocal(Coordinates coordinates)
        {
            return coordinates.IsInSquare(Start, End);
        }


        public DemDataCellPixelIsPoint<TPixel> Downsample(int factor, DemDataCellPixelIsPoint<TPixel> north, DemDataCellPixelIsPoint<TPixel> northEast, DemDataCellPixelIsPoint<TPixel> east)
        {
            var newPointsLat = ((PointsLat - 1) / factor) + 1;
            var newPointsLon = ((PointsLon - 1) / factor) + 1;

            var newData = new TPixel[newPointsLat, newPointsLon];
            var samples = new TPixel[factor * factor];
            var northLat = newPointsLat - 1;
            var eastLon = newPointsLon - 1;

            DownsampleCore(factor, northLat, eastLon, newData, samples);

            DownsampleNorthEdge(factor, north, newData, samples, northLat, eastLon);

            DownsampleEastEdge(factor, east, newData, samples, northLat, eastLon);

            DownsampleNorthEastPoint(factor, northEast, newData, samples, northLat, eastLon);

            return new DemDataCellPixelIsPoint<TPixel>(Start, End, newData);
        }

        private void DownsampleNorthEastPoint(int factor, DemDataCellPixelIsPoint<TPixel> northEast, TPixel[,] newData, TPixel[] samples, int northLat, int eastLon)
        {
            if (northEast != null)
            {
                if (northEast.Start.Longitude != End.Longitude || northEast.Start.Latitude != End.Latitude || northEast.SizeLon != SizeLon || northEast.SizeLat != SizeLat)
                {
                    throw new ArgumentException();
                }
                northEast.FillSquareSamples(factor, samples, 0, 0);
                newData[northLat, eastLon] = PixelFormat.Average(samples);
            }
            else
            {
                newData[northLat, eastLon] = Data[PointsLat - 1, PointsLon - 1];
            }
        }

        private void DownsampleEastEdge(int factor, DemDataCellPixelIsPoint<TPixel> east, TPixel[,] newData, TPixel[] samples, int northLat, int eastLon)
        {
            if (east != null)
            {
                if (east.Start.Longitude != End.Longitude || east.Start.Latitude != Start.Latitude || east.PointsLat != PointsLat || east.SizeLon != SizeLon || east.SizeLat != SizeLat)
                {
                    throw new ArgumentException();
                }
                for (var newLat = 0; newLat < northLat; newLat++)
                {
                    east.FillSquareSamples(factor, samples, 0, newLat * factor);
                    newData[newLat, eastLon] = PixelFormat.Average(samples);
                }
            }
            else
            {
                var samplesLine = new TPixel[factor];
                for (var newLat = 0; newLat < northLat; newLat++)
                {
                    FillSamplesLat(samplesLine, newLat * 3, eastLon);
                    newData[newLat, eastLon] = PixelFormat.Average(samplesLine);
                }
            }
        }

        private void DownsampleNorthEdge(int factor, DemDataCellPixelIsPoint<TPixel> north, TPixel[,] newData, TPixel[] samples, int northLat, int eastLon)
        {
            if (north != null)
            {
                if (north.Start.Longitude != Start.Longitude || north.Start.Latitude != End.Latitude || north.PointsLon != PointsLon || north.SizeLon != SizeLon || north.SizeLat != SizeLat)
                {
                    throw new ArgumentException();
                }
                for (var newLon = 0; newLon < eastLon; newLon++)
                {
                    north.FillSquareSamples(factor, samples, 0, newLon * factor);
                    newData[northLat, newLon] = PixelFormat.Average(samples);
                }
            }
            else
            {
                var samplesLine = new TPixel[factor];
                for (var newLon = 0; newLon < eastLon; newLon++)
                {
                    FillSamplesLon(samplesLine, northLat, newLon * 3);
                    newData[northLat, newLon] = PixelFormat.Average(samplesLine);
                }
            }
        }

        private void FillSamplesLat(TPixel[] samples, int startLat, int lon)
        {
            for (var i = 0; i < samples.Length; ++i)
            {
                samples[i] = Data[i + startLat, lon];
            }
        }

        private void FillSamplesLon(TPixel[] samples, int lat, int startLon)
        {
            for (var i = 0; i < samples.Length; ++i)
            {
                samples[i] = Data[lat, startLon + 1];
            }
        }

        internal override U Accept<U>(IDemDataCellVisitor<U> visitor)
        {
            return PixelFormat.Accept(visitor, this);
        }

        public override IEnumerable<DemDataPoint> GetPointsOnParallel(int lat, int startLon, int count)
        {
            if (lat < 0 || lat >= Data.GetLength(0))
            {
                yield break;
            }
            var lonEnd = Math.Min(startLon + count, Data.GetLength(1));
            for (var lon = Math.Max(0, startLon); lon < lonEnd; ++lon)
            {
                yield return new DemDataPoint(
                    new CoordinatesValue(Start.Latitude + (lat * PixelSizeLat),
                    Start.Longitude + (lon * PixelSizeLon)),
                    PixelFormat.ToDouble(Data[lat, lon]));
            }
        }

        public override DemDataCellBase<TOtherPixel> ConvertToBase<TOtherPixel>()
        {
            return ConvertTo<TOtherPixel>();
        }
    }
}

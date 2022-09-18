using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using MapToolkit.DataCells.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.DataCells
{
    public abstract class DemDataCellBase<TPixel> : IDemDataCell
        where TPixel : unmanaged
    {
        internal static DemPixel<TPixel> PixelFormat = DemPixels.Get<TPixel>();

        private protected DemDataCellBase(Coordinates start, Coordinates end, TPixel[,] data)
        {
            Start = start;
            End = end;
            Data = data;

            SizeLat = End.Latitude - Start.Latitude;
            SizeLon = End.Longitude - Start.Longitude;
        }

        public Coordinates Start { get; }

        public Coordinates End { get; }

        public TPixel[,] Data { get; }

        public double SizeLat { get; }
        public double SizeLon { get; }

        public int PointsLat => Data.GetLength(0);

        public int PointsLon => Data.GetLength(1);

        public abstract DemRasterType RasterType { get; }
        public abstract double PixelSizeLat { get; }
        public abstract double PixelSizeLon { get; }

        public int SizeInBytes => Data.Length * Marshal.SizeOf<TPixel>();

        public abstract TPixel GetRawElevation(Coordinates coordinates);
        public abstract IEnumerable<DemDataPoint> GetNearbyElevation(Coordinates coordinates);
        public abstract double GetLocalElevation(Coordinates coordinates, IInterpolation interpolation);
        public abstract DemDataCellPixelIsPoint<TPixel> AsPixelIsPoint();
        public abstract DemDataCellPixelIsArea<TPixel> AsPixelIsArea();

        internal abstract U Accept<U>(IDemDataCellVisitor<U> visitor);

        public abstract IEnumerable<DemDataPoint> GetPointsOnParallel(int lat, int startLon, int count);

        double IDemDataCell.GetRawElevation(Coordinates coordinates)
        {
            return ToDouble(GetRawElevation(coordinates));
        }

        public Coordinates PinToGridCeiling(Coordinates subStart)
        {
            return new Coordinates(
                Start.Latitude + (Math.Ceiling((subStart.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Ceiling((subStart.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon));
        }

        public Coordinates PinToGridFloor(Coordinates subEnd)
        {
            return new Coordinates(
                Start.Latitude + (Math.Floor((subEnd.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Floor((subEnd.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon));
        }

        public DemDataCellBase<TPixel> Crop(Coordinates subStart, Coordinates subEnd)
        {
            return CropExact(PinToGridCeiling(subStart), PinToGridFloor(subEnd));
        }

        protected abstract DemDataCellBase<TPixel> CropExact(Coordinates realStart, Coordinates realEnd);

        protected TPixel[,] CropData(int startLat, int startLon, int countLat, int countLon)
        {
            var subData = new TPixel[countLat, countLon];
            CopyData(startLat, startLon, countLat, countLon, subData, 0, 0);
            return subData;
        }

        internal void CopyData(int startLat, int startLon, int countLat, int countLon, TPixel[,] target, int targetStartLat, int targetStartLon)
        {
            var sizeOfT = Marshal.SizeOf<TPixel>();
            var lineSizeToCopy = countLon * sizeOfT;
            var sourceOffset = (startLat * Data.GetLength(1) + startLon) * sizeOfT;
            var sourceLineJump = Data.GetLength(1) * sizeOfT;
            var targetOffset = (targetStartLat * target.GetLength(1) + targetStartLon) * sizeOfT;
            var targetLineJump = target.GetLength(1) * sizeOfT;

            for (var latOffset = 0; latOffset < countLat; ++latOffset)
            {
                Buffer.BlockCopy(Data, sourceOffset, target, targetOffset, lineSizeToCopy);
                sourceOffset += sourceLineJump;
                targetOffset += targetLineJump;
            }
        }

        protected double ToDouble(TPixel t)
        {
            return PixelFormat.ToDouble(t);
        }

        protected U[,] ConvertData<U>() where U : unmanaged
        {
            var converted = new U[Data.GetLength(0), Data.GetLength(1)];
            for (var localLat = 0; localLat < Data.GetLength(0); localLat++)
            {
                for (var localLon = 0; localLon < Data.GetLength(1); localLon++)
                {
                    converted[localLat, localLon] = (U)Convert.ChangeType(Data[localLat, localLon], typeof(U));
                }
            }
            return converted;
        }

        public byte[] ToBytes()
        {
            var stream = new MemoryStream();
            Save(stream);
            return stream.ToArray();
        }

        public void Save(string target)
        {
            Save(File.Create(target));
        }

        public void Save(Stream target)
        {
            using (var writer = new BinaryWriter(target))
            {
                Save(writer);
            }
        }

        public void Save(BinaryWriter target)
        {
            target.Write(DemDataCell.MagicNumber);

            target.Write((byte)0x01);
            target.Write((byte)0x00);
            target.Write(DemDataCell.GetDataTypeCode(typeof(TPixel)));
            target.Write((byte)RasterType);

            target.Write(Start.Latitude);
            target.Write(Start.Longitude);
            target.Write(End.Latitude);
            target.Write(End.Longitude);

            target.Write(PointsLat);
            target.Write(PointsLon);
            target.Write(0);
            target.Write(0);

            var bytes = new byte[Data.Length * Marshal.SizeOf<TPixel>()];
            target.Write((uint)bytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(Data, 0, bytes, 0, bytes.Length);
            }
            else
            {
                throw new NotImplementedException("BigEndian");
            }
            target.Write(bytes);
        }

        public abstract bool IsLocal(Coordinates coordinates);

        internal void DownsampleCore(int factor, int maxLat, int maxLon, TPixel[,] newData, TPixel[] samples)
        {
            for (var newLat = 0; newLat < maxLat; newLat++)
            {
                for (var newLon = 0; newLon < maxLon; newLon++)
                {
                    FillSquareSamples(factor, samples, newLat * factor, newLon * factor);

                    newData[newLat, newLon] = PixelFormat.Average(samples);
                }
            }
        }

        internal void FillSquareSamples(int factor, TPixel[] samples, int startLat, int startLon)
        {
            for (var i = 0; i < factor; ++i)
            {
                for (var j = 0; j < factor; ++j)
                {
                    samples[(i * factor) + j] = Data[i + startLat, j + startLon];
                }
            }
        }

        IDemDataCell IDemDataCell.Crop(Coordinates subStart, Coordinates subEnd)
        {
            return Crop(subStart, subEnd);
        }

        public DemDataView<TPixel> CreateView(Coordinates start, Coordinates end)
        {
            return new DemDataView<TPixel>(new[] { this }, start, end);
        }

        IDemDataView IDemDataView.CreateView(Coordinates start, Coordinates end)
        {
            return CreateView(start, end);
        }

        public abstract DemDataCellBase<TOtherPixel> ConvertToBase<TOtherPixel>() where TOtherPixel : unmanaged;


#if DEBUG
        internal void SavePreview(string path)
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            for (int y = 0; y < PointsLat; y++)
            {
                for (int x = 0; x < PointsLon; x++)
                {
                    max = Math.Max(PixelFormat.ToDouble(Data[y, x]), max);
                    min = Math.Min(PixelFormat.ToDouble(Data[y, x]), min);
                }
            }

            var legend = new[]
            {
                new { E = min, Color = Color.LightBlue.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.10, Color = Color.DarkGreen.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.15, Color = Color.Green.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.40, Color = Color.Yellow.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.70, Color = Color.Red.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = max, Color = Color.Maroon.ToPixel<Rgb24>().ToScaledVector4() }
            };

            using (var img = new Image<Rgb24>(PointsLon, PointsLat))
            {
                for (int y = 0; y < PointsLat; y++)
                {
                    for (int x = 0; x < PointsLon; x++)
                    {
                        var elevation = PixelFormat.ToDouble(Data[y, x]);
                        var before = legend.Where(e => e.E <= elevation).Last();
                        var after = legend.FirstOrDefault(e => e.E > elevation) ?? legend.Last();
                        var scale = (float)((elevation - before.E) / (after.E - before.E));
                        Rgb24 rgb = new Rgb24();
                        rgb.FromScaledVector4(Vector4.Lerp(before.Color, after.Color, scale));
                        img[x, PointsLat - y - 1] = rgb;
                    }
                }
                img.Save(path);
            }
        }
#endif
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SimpleDEM.DataCells.PixelFormats;

namespace SimpleDEM.DataCells
{
    public abstract class DemDataCellBase<T> : IDemDataCell
        where T : unmanaged
    {
        internal static DemPixel<T> PixelFormat = DemPixels.Get<T>();

        public DemDataCellBase(Coordinates start, Coordinates end, T[,] data)
        {
            Start = start;
            End = end;
            Data = data;

            SizeLat = End.Latitude - Start.Latitude;
            SizeLon = End.Longitude - Start.Longitude;
        }

        public Coordinates Start { get; }

        public Coordinates End { get; }

        public T[,] Data { get; }

        public double SizeLat { get; }
        public double SizeLon { get; }

        public int PointsLat => Data.GetLength(0);

        public int PointsLon => Data.GetLength(1);

        public abstract DemRasterType RasterType { get; }
        public abstract double PixelSizeLat { get; }
        public abstract double PixelSizeLon { get; }

        public int SizeInBytes => Data.Length * Marshal.SizeOf<T>();

        public abstract T GetRawElevation(Coordinates coordinates);
        public abstract IEnumerable<DemDataPoint> GetNearbyElevation(Coordinates coordinates);
        public abstract double GetLocalElevation(Coordinates coordinates, IInterpolation interpolation);
        public abstract DemDataCellPixelIsPoint<T> AsPixelIsPoint();
        public abstract DemDataCellPixelIsArea<T> AsPixelIsArea();

        internal abstract U Accept<U>(IDemDataCellVisitor<U> visitor);

        double IDemDataCell.GetRawElevation(Coordinates coordinates)
        {
            return ToDouble(GetRawElevation(coordinates));
        }

        public DemDataCellBase<T> Crop(Coordinates subStart, Coordinates subEnd)
        {
            var realStart = new Coordinates(
                Start.Latitude + (Math.Ceiling((subStart.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Ceiling((subStart.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon)
                );

            var realEnd = new Coordinates(
                Start.Latitude + (Math.Floor((subEnd.Latitude - Start.Latitude) / PixelSizeLat) * PixelSizeLat),
                Start.Longitude + (Math.Floor((subEnd.Longitude - Start.Longitude) / PixelSizeLon) * PixelSizeLon)
                );

            return CropExact(realStart, realEnd);
        }

        protected abstract DemDataCellBase<T> CropExact(Coordinates realStart, Coordinates realEnd);

        protected T[,] CropData(int startLat, int startLon, int countLat, int countLon)
        {
            var subData = new T[countLat, countLon];
            var sizeOfT = Marshal.SizeOf<T>();
            var sizeOfCountLon = countLon * sizeOfT;
            for (var latTarget = 0; latTarget < countLat; ++latTarget)
            {
                Buffer.BlockCopy(
                    Data, 
                    ((latTarget + startLat) * Data.GetLength(1) + startLon) * sizeOfT, 
                    subData,
                    latTarget * sizeOfCountLon,
                    sizeOfCountLon);
            }
            return subData;
        }

        protected double ToDouble(T t)
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
            target.Write(DemDataCell.GetDataTypeCode(typeof(T)));
            target.Write((byte)0x00);
            target.Write((byte)RasterType);

            target.Write(Start.Latitude);
            target.Write(Start.Longitude);
            target.Write(End.Latitude);
            target.Write(End.Longitude);

            target.Write(PointsLat);
            target.Write(PointsLon);

            var bytes = new byte[Data.Length * Marshal.SizeOf<T>()];
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

        internal void DownsampleCore(int factor, int maxLat, int maxLon, T[,] newData, T[] samples)
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

        internal void FillSquareSamples(int factor, T[] samples, int startLat, int startLon)
        {
            for (var i = 0; i < factor; ++i)
            {
                for (var j = 0; j < factor; ++j)
                {
                    samples[(i * factor) + j] = Data[i + startLat, j + startLon];
                }
            }
        }

    }
}
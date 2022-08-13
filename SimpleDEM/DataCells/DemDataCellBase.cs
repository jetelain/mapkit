using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleDEM.DataCells
{
    public abstract class DemDataCellBase<T> : IDemDataCell
        where T : unmanaged
    {
        public DemDataCellBase(GeodeticCoordinates start, GeodeticCoordinates end, T[,] data)
        {
            Start = start;
            End = end;
            Data = data;

            SizeLat = End.Latitude - Start.Latitude;
            SizeLon = End.Longitude - Start.Longitude;
        }

        public GeodeticCoordinates Start { get; }

        public GeodeticCoordinates End { get; }

        public T[,] Data { get; }

        public double SizeLat { get; }
        public double SizeLon { get; }

        public int PointsPerCellLat => Data.GetLength(0);

        public int PointsPerCellLon => Data.GetLength(1);

        public abstract DemRasterType RasterType { get; }
        public abstract double PixelSizeLat { get; }
        public abstract double PixelSizeLon { get; }

        public int SizeInBytes => Data.Length * Marshal.SizeOf<T>();

        public abstract T GetRawElevation(GeodeticCoordinates coordinates);
        public abstract IEnumerable<DemDataPoint> GetNearbyElevation(GeodeticCoordinates coordinates);
        public abstract double GetLocalElevation(GeodeticCoordinates coordinates, IInterpolation interpolation);

        double IDemDataCell.GetRawElevation(GeodeticCoordinates coordinates)
        {
            return ToDouble(GetRawElevation(coordinates));
        }

        protected double ToDouble(T t)
        {
            var value = Convert.ToDouble(t);
            if (value >= short.MaxValue) // short.MaxValue is used for "No value" (32767)
            {
                return double.NaN;
            }
            return value;
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
            target.Write((byte)RasterType);
            target.Write((byte)0x00);
            target.Write(Start.Latitude);
            target.Write(Start.Longitude);
            target.Write(End.Latitude);
            target.Write(End.Longitude);
            target.Write(PointsPerCellLat);
            target.Write(PointsPerCellLon);

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

        public abstract bool IsLocal(GeodeticCoordinates coordinates);
    }
}
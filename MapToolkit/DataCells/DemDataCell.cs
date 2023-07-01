using System;
using System.Diagnostics;
using System.IO;
using MapToolkit.DataCells.FileFormats;

namespace MapToolkit.DataCells
{
    public static class DemDataCell
    {
        public const string Extension = ".ddc";

        public static DemDataCellBase<TPixel> To<TPixel>(this IDemDataCell cell)
            where TPixel : unmanaged
        {
            return (cell as DemDataCellBase<TPixel>) ?? cell.ConvertToBase<TPixel>();
        }

        public static bool IsDemDataCellFile(string file)
        {
            var ext = CompressionHelper.GetExtension(file);

            return ext == Extension || ext == SRTMHelper.Extension || ext == GeoTiffHelper.Extension || ext == EsriAsciiHelper.Extension;
        }

        public static IDemDataCell Load(string file, IProgress<double>? progress = null)
        {
            var ext = CompressionHelper.GetExtension(file);
            if (ext == SRTMHelper.Extension)
            {
                return SRTMHelper.LoadDataCell(file);
            }
            if (ext == GeoTiffHelper.Extension)
            {
                return GeoTiffHelper.LoadDataCell(file);
            }
            if (ext == EsriAsciiHelper.Extension)
            {
                return EsriAsciiHelper.LoadDataCell(file, progress);
            }
            if (ext == Extension)
            {
                return CompressionHelper.Read(file, Load);
            }
            throw new IOException($"Extension '{ext}' is not supported.");
        }

        public static DemDataCellMetadata LoadMetadata(string file)
        {
            var ext = CompressionHelper.GetExtension(file);
            if (ext == SRTMHelper.Extension)
            {
                return SRTMHelper.LoadDataCellMetadata(file);
            }
            if (ext == GeoTiffHelper.Extension)
            {
                return GeoTiffHelper.LoadDataCellMetadata(file);
            }
            if (ext == EsriAsciiHelper.Extension)
            {
                return EsriAsciiHelper.LoadDataCellMetadata(file);
            }
            if (ext == Extension)
            {
                return CompressionHelper.Read(file, LoadMetadata);
            }
            throw new IOException($"Extension '{ext}' is not supported.");
        }

        public static IDemDataCell Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                return Load(reader);
            }
        }

        public static DemDataCellMetadata LoadMetadata(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                return LoadMetadata(reader);
            }
        }

        internal static DemDataCellMetadata LoadMetadata(BinaryReader reader)
        {
            ReadPrelude(reader);
            reader.ReadByte(); // dataType
            return new DemDataCellMetadata(reader);
        }

        public static IDemDataCell Load(BinaryReader reader)
        {
            ReadPrelude(reader);
            var dataType = reader.ReadByte();
            var metadata = new DemDataCellMetadata(reader);
            reader.ReadInt32(); // Unused
            reader.ReadInt32(); // Unused
            Debug.Assert(reader.BaseStream.Position == 0x38);
            switch (dataType)
            {
                case 0:
                    return ReadData<float>(reader, metadata);
                case 1:
                    return ReadData<short>(reader, metadata);
                case 2:
                    return ReadData<ushort>(reader, metadata);
                case 3:
                    return ReadData<double>(reader, metadata);
                case 4:
                    return ReadData<int>(reader, metadata);
                default:
                    throw new IOException($"{dataType} is not a valid data type.");
            }
        }

        private static byte ReadPrelude(BinaryReader reader)
        {
            if (reader.ReadInt32() != MagicNumber)
            {
                throw new IOException($"Invalid prelude.");
            }
            var version = reader.ReadByte();
            if (version != 1)
            {
                throw new IOException($"Version {version} is not supported.");
            }
            var subversion = reader.ReadByte();
            return subversion;
        }

        private static DemDataCellBase<T> ReadData<T>(BinaryReader reader, DemDataCellMetadata metadata)
            where T : unmanaged
        {
            var dataSize = reader.ReadUInt32();
            var bytes = new byte[dataSize];
            if (reader.BaseStream.Read(bytes, 0, (int)dataSize) != dataSize)
            {
                throw new IOException($"Premature end of file.");
            }

            var data = new T[metadata.PointsLat, metadata.PointsLon];
            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);
            }
            else
            {
                throw new NotImplementedException("BigEndian");
            }
            return Create(metadata.Start, metadata.End, metadata.RasterType, data);
        }

        internal static DemDataCellBase<T> Create<T>(Coordinates start, Coordinates end, DemRasterType type, T[,] data)
            where T : unmanaged
        {
            switch (type)
            {
                default:
                case DemRasterType.Unknown:
                case DemRasterType.PixelIsArea:
                    return new DemDataCellPixelIsArea<T>(start, end, data);

                case DemRasterType.PixelIsPoint:
                    return new DemDataCellPixelIsPoint<T>(start, end, data);
            }
        }

        internal const int MagicNumber = 0x57d15a3c;

        internal static byte GetDataTypeCode(Type t)
        {
            if (t == typeof(float))
            {
                return 0;
            }
            if (t == typeof(short))
            {
                return 1;
            }
            if (t == typeof(ushort))
            {
                return 2;
            }
            if (t == typeof(double))
            {
                return 3;
            }
            if (t == typeof(int))
            {
                return 4;
            }
            throw new ArgumentException();
        }
    }
}

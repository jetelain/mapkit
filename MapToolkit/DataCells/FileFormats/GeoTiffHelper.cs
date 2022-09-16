using System;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using MapToolkit.GeodeticSystems;

namespace MapToolkit.DataCells.FileFormats
{
    internal class GeoTiffHelper
    {
        public const string Extension = ".tif";

        public static IDemDataCell LoadDataCell(string filepath)
        {
            return CompressionHelper.ReadSeekable(filepath, stream => LoadDataCell(filepath, stream));
        }

        public static IDemDataCell LoadDataCell(string name, Stream stream)
        {
            using (var tiff = Tiff.ClientOpen(name, "r", stream, new TiffStream()))
            {
                return LoadDataCell(tiff);
            }
        }

        public static DemDataCellMetadata LoadDataCellMetadata(string filepath)
        {
            return CompressionHelper.ReadSeekable(filepath, stream => LoadDataCellMetadata(filepath, stream));
        }

        public static DemDataCellMetadata LoadDataCellMetadata(string filepath, Stream stream)
        {
            using (var tiff = Tiff.ClientOpen(filepath, "r", stream, new TiffStream()))
            {
                return LoadDataCellMetadata(tiff);
            }
        }

        internal static DemDataCellMetadata LoadDataCellMetadata(Tiff tiff)
        {
            var height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            var width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();

            var modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            var modelPixelScale = new BinaryReader(new MemoryStream(modelPixelScaleTag[1].GetBytes())); // Data is LittleEndian
            var pixelSizeX = modelPixelScale.ReadDouble();
            var pixelSizeY = modelPixelScale.ReadDouble() * -1;

            var modelTiepointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
            var modelTransformation = new BinaryReader(new MemoryStream(modelTiepointTag[1].GetBytes())); // Data is LittleEndian
            modelTransformation.ReadDouble();
            modelTransformation.ReadDouble();
            modelTransformation.ReadDouble();
            var dataStartLon = modelTransformation.ReadDouble();
            var dataStartLat = modelTransformation.ReadDouble();
            var dataEndLon = dataStartLon + width * pixelSizeX;
            var dataEndLat = dataStartLat + height * pixelSizeY;

            var start = new Coordinates(dataEndLat, dataStartLon); // Intentional swap on Latitude
            var end = new Coordinates(dataStartLat, dataEndLon);

            var raster = GetRasterType(tiff);

            return new DemDataCellMetadata(raster, start, end, height, width);
        }

        private static IDemDataCell LoadDataCell(Tiff tiff)
        {
            var metadata = LoadDataCellMetadata(tiff);

            var height = metadata.PointsLat;
            var width = metadata.PointsLon;

            var bitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            var sampleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT).FirstOrDefault().ToString();

            if (sampleFormat == "INT" && bitsPerSample == 16)
            {
                return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, ReadData(tiff, width, height, (reader) => reader.ReadInt16()));
            }
            if (sampleFormat == "UINT" && bitsPerSample == 16)
            {
                return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, ReadData(tiff, width, height, (reader) => reader.ReadUInt16()));
            }
            if (sampleFormat == "IEEEFP" && bitsPerSample == 32)
            {
                return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, ReadData(tiff, width, height, (reader) => reader.ReadSingle()));
            }
            if (sampleFormat == "IEEEFP" && bitsPerSample == 64)
            {
                return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, ReadData(tiff, width, height, (reader) => reader.ReadDouble()));
            }
            throw new IOException($"GeoTIFF Sample format '{sampleFormat}' with {bitsPerSample} bits per sample is not supported.");
        }

        private static DemRasterType GetRasterType(Tiff tiff)
        {
            var geoKey = new BinaryReader(new MemoryStream(tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG)[1].ToByteArray())); // Data is LittleEndian
            geoKey.ReadUInt16(); // keyDirectoryVersion 
            geoKey.ReadUInt16(); // keyRevision 
            geoKey.ReadUInt16(); // minorRevision 
            var count = geoKey.ReadUInt16();
            var raster = DemRasterType.Unknown;
            for (int i = 8; i < 8 + count * 8; i += 8)
            {
                var keyID = geoKey.ReadUInt16();
                geoKey.ReadUInt16();
                geoKey.ReadUInt16();
                var valueOffset = geoKey.ReadUInt16();
                if (keyID == 1025)
                {
                    if (valueOffset == 1)
                    {
                        raster = DemRasterType.PixelIsArea;
                    }
                    else if (valueOffset == 2)
                    {
                        raster = DemRasterType.PixelIsPoint;
                    }
                }
                else if (keyID == 2048)
                {
                    if (valueOffset != WSG84.EPSG)
                    {
                        throw new IOException($"Only CRS/GCS EPSG:4326 (WSG84) is supported. File CRS/GCS is '{valueOffset}'");
                    }
                }
            }

            return raster;
        }

        private static T[,] ReadData<T>(Tiff tiff, int width, int height, Func<BinaryReader, T> read)
            where T : unmanaged
        {
            var data = new T[height, width];
            var buffer = new byte[tiff.ScanlineSize()];
            for (int lat = 0; lat < height; lat++)
            {
                tiff.ReadScanline(buffer, lat);
                var reader = new BinaryReader(new MemoryStream(buffer)); // Data is LittleEndian
                for (int lon = 0; lon < width; lon++)
                {
                    data[height - lat - 1, lon] = read(reader);
                }
            }
            return data;
        }
    }
}

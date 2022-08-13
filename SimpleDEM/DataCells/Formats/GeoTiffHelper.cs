using System;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;

namespace SimpleDEM.DataCells.Formats
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

        private static IDemDataCell LoadDataCell(Tiff tiff)
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

            var start = new GeodeticCoordinates(dataEndLat, dataStartLon); // Intentional swap on Latitude
            var end = new GeodeticCoordinates(dataStartLat, dataEndLon);

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
            }

            var bitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            var sampleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT).FirstOrDefault().ToString();

            if (sampleFormat == "INT" && bitsPerSample == 16)
            {
                return DemDataCell.Create(start, end, raster, ReadData(tiff, width, height, (reader) => reader.ReadInt16()));
            }
            if (sampleFormat == "UINT" && bitsPerSample == 16)
            {
                return DemDataCell.Create(start, end, raster, ReadData(tiff, width, height, (reader) => reader.ReadUInt16()));
            }
            if (sampleFormat == "IEEEFP" && bitsPerSample == 32)
            {
                return DemDataCell.Create(start, end, raster, ReadData(tiff, width, height, (reader) => reader.ReadSingle()));
            }
            if (sampleFormat == "IEEEFP" && bitsPerSample == 64)
            {
                return DemDataCell.Create(start, end, raster, ReadData(tiff, width, height, (reader) => reader.ReadDouble()));
            }
            throw new IOException($"GeoTIFF Sample format '{sampleFormat}' with {bitsPerSample} bits per sample is not supported.");
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

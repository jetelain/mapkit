using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace SimpleDEM.DataCells.Formats
{
    internal class SRTMHelper
    {
        public const string Extension = ".hgt";

        private static readonly Regex FileNameRegex = new Regex("^([NS])([0-9]+)([EW])([0-9]+)\\.");

        public static DemDataCellPixelIsPoint<ushort> LoadDataCell(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }
            return CompressionHelper.Read(filepath, stream => LoadDataCell(filepath, stream));
        }

        public static DemDataCellPixelIsPoint<ushort> LoadDataCell(string filepath, Stream stream)
        {
            var pos = GetCoordinatesFromFileName(filepath);

            var ms = new MemoryStream();

            stream.CopyTo(ms);

            var bytes = ms.ToArray();

            var pointsPerCell = DetectResolution(bytes.Length);

            var data = ConvertData(bytes, pointsPerCell);

            return new DemDataCellPixelIsPoint<ushort>(pos, new GeodeticCoordinates(pos.Latitude + 1, pos.Longitude + 1), data);
        }

        private static ushort[,] ConvertData(byte[] bytes, int pointsPerCell)
        {
            var target = new ushort[pointsPerCell, pointsPerCell];

            for (var localLat = 0; localLat < pointsPerCell; localLat++)
            {
                for (var localLon = 0; localLon < pointsPerCell; localLon++)
                {
                    int bytesPos = (pointsPerCell - localLat - 1) * pointsPerCell * 2 + localLon * 2;
                    if (bytes[bytesPos] == 0x80 && bytes[bytesPos + 1] == 0x00)
                    {
                        target[localLat, localLon] = ushort.MaxValue;
                    }
                    else
                    {
                        // Big-Endian
                        target[localLat, localLon] = (ushort)(bytes[bytesPos] << 8 | bytes[bytesPos + 1]);
                    }
                }
            }
            return target;
        }

        private static int DetectResolution(int length)
        {
            switch (length)
            {
                case 1201 * 1201 * 2: // SRTM-3
                    return 1201;
                case 3601 * 3601 * 2: // SRTM-1
                    return 3601;
                default:
                    throw new ArgumentException();
            }
        }

        private static GeodeticCoordinates GetCoordinatesFromFileName(string filepath)
        {
            var matches = FileNameRegex.Match(Path.GetFileNameWithoutExtension(filepath));
            if (!matches.Success)
            {
                throw new ArgumentException(nameof(filepath));
            }

            var latitude = int.Parse(matches.Groups[2].Value, CultureInfo.InvariantCulture);
            if (string.Equals(matches.Groups[1].Value, "S", StringComparison.OrdinalIgnoreCase))
            {
                latitude *= -1;
            }

            var longitude = int.Parse(matches.Groups[4].Value, CultureInfo.InvariantCulture);
            if (string.Equals(matches.Groups[3].Value, "W", StringComparison.OrdinalIgnoreCase))
            {
                longitude *= -1;
            }

            return new GeodeticCoordinates(latitude, longitude);
        }
    }
}
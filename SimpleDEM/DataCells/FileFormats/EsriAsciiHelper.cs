using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleDEM.DataCells.FileFormats
{
    internal class EsriAsciiHelper
    {
        public const string Extension = ".asc";

        public static DemDataCellMetadata LoadDataCellMetadata(string filepath)
        {
            return CompressionHelper.Read(filepath, stream => LoadDataCellMetadata(new StreamReader(stream, Encoding.UTF8, false, 4096, true), out _));
        }

        public static DemDataCellBase<float> LoadDataCell(string filepath)
        {
            return CompressionHelper.Read(filepath, stream => LoadDataCell(new StreamReader(stream, Encoding.UTF8, false, 4096, true)));
        }

        private static DemDataCellBase<float> LoadDataCell(StreamReader streamReader)
        {
            var metadata = LoadDataCellMetadata(streamReader, out var nodata);
            var data = new float[metadata.PointsLat, metadata.PointsLon];
            var buffer = new StringBuilder(16);
            var lat = metadata.PointsLat - 1; // "Row 1 of the data is at the top of the raster"
            var lon = 0;
            while (lat >= 0)
            {
                var c = streamReader.Read();
                if (c == -1)
                {
                    break;
                }
                if (c == ' ' || c == '\r' || c == '\n')  // "No carriage returns are necessary at the end of each row in the raster. The number of columns in the header determines when a new row begins."
                {
                    if (buffer.Length > 0)
                    {
                        var value = float.Parse(buffer.ToString(), CultureInfo.InvariantCulture);
                        data[lat, lon] = value == nodata ? float.NaN : value;
                        buffer.Length = 0;
                        lon++;
                        if (lon == metadata.PointsLon)
                        {
                            lat--;
                            lon = 0;
                        }
                    }
                }
                else
                {
                    buffer.Append((char)c);
                }
            }
            return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, data);
        }

        private static DemDataCellMetadata LoadDataCellMetadata(StreamReader stream, out float nodata)
        {
            var header = Enumerable.Range(0, 6)
                .Select(_ => stream.ReadLine() ?? string.Empty)
                .Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(l => l[0].ToLowerInvariant(), l => l.Length > 1 ? l[1] : string.Empty);

            if (!header.TryGetValue("ncols", out var ncols))
            {
                throw new IOException("Missing ncols");
            }
            var ncolsNum = int.Parse(ncols, CultureInfo.InvariantCulture);

            if (!header.TryGetValue("nrows", out var nrows))
            {
                throw new IOException("Missing nrows");
            }
            var nrowsNum = int.Parse(nrows, CultureInfo.InvariantCulture);

            if (!header.TryGetValue("cellsize", out var cellsize))
            {
                throw new IOException("Missing cellsize");
            }
            var cellsizeNum = double.Parse(cellsize, CultureInfo.InvariantCulture);

            if (!header.TryGetValue("nodata_value", out var nodata_value))
            {
                throw new IOException("Missing nodata_value");
            }
            nodata = float.Parse(nodata_value, CultureInfo.InvariantCulture);

            Coordinates start;
            DemRasterType type;
            if (header.TryGetValue("xllcenter", out var xllcenter))
            {
                if (!header.TryGetValue("yllcenter", out var yllcenter))
                {
                    throw new IOException("Missing yllcenter");
                }
                start = new Coordinates(double.Parse(yllcenter, CultureInfo.InvariantCulture), double.Parse(xllcenter, CultureInfo.InvariantCulture));
                type = DemRasterType.PixelIsArea;
            }
            else if (header.TryGetValue("xllcorner", out var xllcorner))
            {
                if (!header.TryGetValue("yllcorner", out var yllcorner))
                {
                    throw new IOException("Missing yllcorner");
                }
                start = new Coordinates(double.Parse(yllcorner, CultureInfo.InvariantCulture), double.Parse(xllcorner, CultureInfo.InvariantCulture));
                type = DemRasterType.PixelIsPoint;
            }
            else
            {
                throw new IOException("Missing xllcenter or xllcorner");
            }

            var end = DemDataCellMetadata.EndFromResolution(start, type,  nrowsNum, ncolsNum, cellsizeNum, cellsizeNum);

            return new DemDataCellMetadata(type, start, end, nrowsNum, ncolsNum);
        }
    }
}

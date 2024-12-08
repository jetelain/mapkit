using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Pmad.Cartography.DataCells.FileFormats
{
    public static class EsriAsciiHelper
    {
        public const string Extension = ".asc";

        internal static DemDataCellMetadata LoadDataCellMetadata(string filepath)
        {
            return CompressionHelper.Read(filepath, stream => LoadDataCellMetadata(new StreamReader(stream, Encoding.UTF8, false, 4096, true), out _));
        }

        internal static DemDataCellBase<float> LoadDataCell(string filepath, IProgress<double>? progress = null)
        {
            return CompressionHelper.Read(filepath, stream => LoadDataCell(new StreamReader(stream, Encoding.UTF8, false, 10240, true), progress));
        }

        public static DemDataCellBase<float> LoadDataCell(TextReader streamReader, IProgress<double>? progress = null)
        {
            var metadata = LoadDataCellMetadata(streamReader, out var nodata);
            var data = new float[metadata.PointsLat, metadata.PointsLon];
            var buffer = new StringBuilder(16);
            var lat = metadata.PointsLat - 1; // "Row 1 of the data is at the top of the raster"
            var lon = 0;
            var read = 0;
            var total = metadata.PointsLat;
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
                            if (progress != null)
                            {
                                read++;
                                progress.Report(read * 100.0 / total);
                            }
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

        internal static DemDataCellMetadata LoadDataCellMetadata(TextReader stream, out float nodata)
        {
            var header = Enumerable.Range(0, 6)
                .Select(_ => stream.ReadLine() ?? string.Empty)
                .Where(l => !string.IsNullOrEmpty(l))
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

            var end = DemDataCellMetadata.EndFromResolution(start, type, nrowsNum, ncolsNum, cellsizeNum, cellsizeNum);

            return new DemDataCellMetadata(type, start, end, nrowsNum, ncolsNum);
        }

        public static void SaveDataCell(TextWriter writer, DemDataCellBase<float> dataCell, string nodata = "-9999", IProgress<double>? progress = null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (dataCell == null)
            {
                throw new ArgumentNullException(nameof(dataCell));
            }
            if (string.IsNullOrEmpty(nodata))
            {
                throw new ArgumentNullException(nameof(nodata));
            }
            writer.WriteLine(FormattableString.Invariant($"ncols         {dataCell.PointsLon}"));
            writer.WriteLine(FormattableString.Invariant($"nrows         {dataCell.PointsLat}"));
            if (dataCell.RasterType == DemRasterType.PixelIsArea)
            {
                writer.WriteLine(FormattableString.Invariant($"xllcenter     {dataCell.Start.Longitude}"));
                writer.WriteLine(FormattableString.Invariant($"yllcenter     {dataCell.Start.Latitude}"));
            }
            else 
            {
                writer.WriteLine(FormattableString.Invariant($"xllcorner     {dataCell.Start.Longitude}"));
                writer.WriteLine(FormattableString.Invariant($"yllcorner     {dataCell.Start.Latitude}"));
            }
            writer.WriteLine(FormattableString.Invariant($"cellsize      {dataCell.PixelSizeLat}"));
            writer.WriteLine(FormattableString.Invariant($"NODATA_value  {nodata}"));
            for (int lat = dataCell.PointsLat - 1; lat >= 0; lat--)
            {
                for (int lon = 0; lon < dataCell.PointsLon; lon++)
                {
                    if ( lon > 0 )
                    {
                        writer.Write(" ");
                    }
                    var value = dataCell.Data[lat, lon];
                    if (float.IsNaN(value))
                    {
                        writer.Write(nodata);
                    }
                    else
                    {
                        writer.Write(dataCell.Data[lat, lon].ToString("0.00", CultureInfo.InvariantCulture));
                    }
                }
                writer.WriteLine();
                if (progress != null)
                {
                    progress.Report((dataCell.PointsLat - lat) / dataCell.PointsLat);
                }
            }
        }

    }
}

using MapToolkit.Databases;
using System.Text.Json;
using MapToolkit.DataCells;
using PureHDF;

namespace HdfConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var root = H5File.OpenRead(@"E:\Carto\SRTM15_V2.5.5.nc");

            var latValues = root.Dataset("lat").Read<double>();
            var lonValues = root.Dataset("lon").Read<double>();
            var zValues = root.Dataset("z");

            for (var lat = -90; lat < 90; lat += step)
            {
                for (var lon = -180; lon < 180; lon += step)
                {
                    ExtractBlock(lat, lon, zValues);
                }
            }
        }

        private const int step = 4;
        private const int size = 240 * step;

        private static void ExtractBlock(int lat, int lon, IH5Dataset zValues)
        {
            Console.WriteLine($"{lat} / {lon}");
            var latShift = (lat + 90) * 240;
            var lonShift = (lon + 180) * 240;

            var points = new ulong[size * size, 2];
            for (var latIndex = 0; latIndex < size; ++latIndex)
            {
                for (var lonIndex = 0; lonIndex < size; ++lonIndex)
                {
                    var index = lonIndex + (latIndex * size);
                    points[index, 0] = (ulong)(latIndex + latShift);
                    points[index, 1] = (ulong)(lonIndex + lonShift);
                }
            }

            var zBlock = zValues.Read<float>(new PointSelection(points));
            var data = new float[size, size];
            for (var latIndex = 0; latIndex < size; ++latIndex)
            {
                for (var lonIndex = 0; lonIndex < size; ++lonIndex)
                {
                    var index = lonIndex + (latIndex * size);
                    data[latIndex, lonIndex] = zBlock[index];
                }
            }

            var cell = new DemDataCellPixelIsArea<float>(new MapToolkit.Coordinates(lat, lon), new MapToolkit.Coordinates(lat + step, lon + step), data);
            cell.Save($@"C:\temp\SRTM15Plus\SRTM15_{Lat(lat)}_{Lon(lon)}.ddc");
        }

        private static string Lat(int lat)
        {
            if ( lat < 0 )
            {
                return $"S{Math.Abs(lat):00}";
            }
            return $"N{Math.Abs(lat):00}";
        }
        private static string Lon(int lon)
        {
            if (lon < 0)
            {
                return $"W{Math.Abs(lon):000}";
            }
            return $"E{Math.Abs(lon):000}";
        }
    }
}
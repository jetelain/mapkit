using System;
using System.Linq;
using SimpleDEM.DataCells;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SimpleDEM.Hillshading
{
    public abstract class HillshaderBase
    {
        public Image<L8> GetPixels(IDemDataView cell, IProgress<double>? progress = null)
        {
            return GetPixels(cell, v => new L8((byte)(255 * v)), progress);
        }

        public Image<L8> GetPixelsBelowFlat(IDemDataView cell, IProgress<double>? progress = null)
        {
            var flat = Flat;
            if ( flat == 1)
            {
                return GetPixels(cell, progress);
            }
            var scale = 255 / flat;
            return GetPixels(cell, v => v >= flat ? new L8(255) : new L8((byte)(v * scale)), progress);
        }

        public Image<TPixel> GetPixels<TPixel>(IDemDataView cell, Func<double,TPixel> luminanceToPixel, IProgress<double>? progress = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            double[]? southLine = null;
            double[]? line = null; 
            double[]? northLine = null;
            var pixels = new Image<TPixel>(cell.PointsLon, cell.PointsLat);
            for (int lat = 0; lat < cell.PointsLat; lat++)
            {
                line = northLine ?? cell.GetPointsOnParallel(lat, 0, cell.PointsLon).Select(p => p.Elevation).ToArray();
                if (lat == cell.PointsLat - 1)
                {
                    northLine = line;
                }
                else
                {
                    northLine = cell.GetPointsOnParallel(lat + 1, 0, cell.PointsLon).Select(p => p.Elevation).ToArray();
                }
                var y = cell.PointsLat - lat - 1;
                for (int x = 0; x < line.Length; ++x)
                {
                    pixels[x, y] = luminanceToPixel(GetPixelLuminance(southLine ?? line, line, northLine, x));
                }
                southLine = line;
                progress?.Report((double)lat / (cell.PointsLat - 1) * 100d);
            }
            return pixels;
        }

        /// <summary>
        /// Luminance of a pixel representing flat terrain [0 ; 1]
        /// </summary>
        protected abstract double Flat { get; }

        /// <summary>
        /// Compute luminance of pixel
        /// </summary>
        /// <param name="southLine">Previous parallel at south of current parallel</param>
        /// <param name="line">Current parallel</param>
        /// <param name="northLine">Next parallel at north of current parallel</param>
        /// <param name="x">Position on parallel (Longitude or X depending of CRS)</param>
        /// <returns>Value in [0 ; 1]</returns>
        protected abstract double GetPixelLuminance(double[] southLine, double[] line, double[] northLine, int x);

    }
}

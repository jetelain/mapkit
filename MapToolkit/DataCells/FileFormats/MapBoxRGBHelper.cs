using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.DataCells.FileFormats
{
    public static class MapBoxRGBHelper
    {
        /// <summary>
        /// Create an image from DEM pixels using MapBox RGB encoding 
        /// https://docs.mapbox.com/data/tilesets/reference/mapbox-terrain-dem-v1/
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Image<Rgb24> ToMapBoxRGB(this IDemDataView view)
        {
            return view.ToImage(ToMapBoxRGB);
        }

        internal static Rgb24 ToMapBoxRGB(double elevation)
        {
            // elevation = -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1)
            var encoded = (int)(elevation * 10 + 10000);
            return new Rgb24((byte)(encoded >> 16 & 0xFF), (byte)(encoded >> 8 & 0xFF), (byte)(encoded & 0xFF));
        }

        internal static float FromMapBoxRGB(Rgb24 rgb24)
        {
            return -10000 + ((rgb24.R << 16 + rgb24.G << 8 + rgb24.B) * 0.1f);
        }

        // IEEE floats have up to 7 digits of precision
        // We need 6 digits, it should be enough

        public static float[,] GetData(Image<Rgb24> image)
        {
            var data = new float[image.Height, image.Width];
            for (int lat = 0; lat < image.Height; lat++)
            {
                for (int lon = 0; lon < image.Width; lon++)
                {
                    data[lat, lon] = FromMapBoxRGB(image[lon, lat]);
                }
            }
            return data;
        }

        public static DemDataCellBase<float> GetDemDataCell(Image<Rgb24> image, DemDataCellMetadata metadata)
        {
            if (image.Width != metadata.PointsLon || image.Height != metadata.PointsLat)
            {
                throw new ArgumentException();
            }
            return DemDataCell.Create(metadata.Start, metadata.End, metadata.RasterType, GetData(image));
        }

        // Tiles from MapBox seems to be PixelIsPoint
        // https://github.com/mapbox/mapbox-gl-js/blob/main/src/terrain/elevation.js#L105
    }
}

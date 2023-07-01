using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.DataCells
{
    public static class DemDataViewExtensions
    {
        /// <summary>
        /// Create an image from DEM pixels of a <see cref="IDemDataView"/>
        /// </summary>
        /// <typeparam name="TPixel">Image pixel format</typeparam>
        /// <param name="view">DataSource</param>
        /// <param name="elevationToColor">Pixel color to use for elevation</param>
        /// <returns></returns>
        public static Image<TPixel> ToImage<TPixel>(this IDemDataView view, Func<double,TPixel> elevationToColor) 
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var img = new Image<TPixel>(view.PointsLon, view.PointsLat);
            var isReversed = view.PixelSizeLat < 0;
            Parallel.For(0, view.PointsLat, (int lat) =>
            {
                int lon = 0;
                foreach (var point in view.GetPointsOnParallel(lat, 0, view.PointsLon))
                {
                    if (!double.IsNaN(point.Elevation))
                    {
                        if (isReversed)
                        {
                            img[lon, lat] = elevationToColor(point.Elevation);
                        }
                        else
                        {
                            img[lon, view.PointsLat - lat - 1] = elevationToColor(point.Elevation);
                        }
                    }
                    lon++;
                }
            });
            return img;
        }

        internal static Image<Rgb24> ToImage(this IDemDataView view, DemLegend legend)
        {
            return ToImage(view, legend.ToRgb24);
        }

        public static Image<Rgb24> ToImageWithRelativeLegend(this IDemDataView view)
        {
            return ToImage(view, DemLegend.CreateRelative(view));
        }

        public static void SaveImagePreview(this IDemDataView view, string path)
        {
            using (var img = view.ToImageWithRelativeLegend())
            {
                img.Save(path);
            }
        }
    }
}

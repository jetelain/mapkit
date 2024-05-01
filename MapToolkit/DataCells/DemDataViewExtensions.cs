using System;
using System.Threading.Tasks;
using MapToolkit.DataCells.PixelFormats;
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

        public static Image<Rgb24> ToImage(this IDemDataView view)
        {
            return ToImage(view, DemLegend.CreateAbsolute());
        }

        public static void SaveImagePreview(this IDemDataView view, string path)
        {
            using (var img = view.ToImageWithRelativeLegend())
            {
                img.Save(path);
            }
        }

        public static void SaveImagePreviewAbsolute(this IDemDataView view, string path)
        {
            using (var img = view.ToImage())
            {
                img.Save(path);
            }
        }

        public static DemDataCellPixelIsArea<TPixel> ToPixelIsArea<TPixel>(this IDemDataView view, Vector pixelSize, IInterpolation interpolation)
            where TPixel : unmanaged
        {
            return ToPixelIsArea<TPixel>(view, view.Start, view.End, pixelSize, interpolation);
        }

        public static DemDataCellPixelIsArea<TPixel> ToPixelIsArea<TPixel>(this IDemDataView view, Coordinates start, Coordinates end, Vector pixelSize, IInterpolation interpolation)
            where TPixel : unmanaged
        {
            var tpixel = DemPixels.Get<TPixel>();
            var size = end - start;
            var latCount = (int)Math.Round(size.DeltaLat / pixelSize.DeltaLat);
            var lonCount = (int)Math.Round(size.DeltaLon / pixelSize.DeltaLon);
            var hpixel = pixelSize / 2;
            var data = new TPixel[latCount, lonCount];
            Parallel.For(0, latCount, (int latIndex) =>
            {
                for (var lonIndex = 0; lonIndex < lonCount; lonIndex++)
                {
                    data[latIndex, lonIndex] = tpixel.FromDouble(view.GetLocalElevation(start + hpixel + Vector.FromLatLonDelta(
                        pixelSize.DeltaLat * latIndex,
                        pixelSize.DeltaLon * lonIndex), interpolation));
                }
            });
            return new DemDataCellPixelIsArea<TPixel>(start, pixelSize, data);
        }

        public static DemDataCellPixelIsPoint<TPixel> ToPixelIsPoint<TPixel>(this IDemDataView view, Vector pixelSize, IInterpolation interpolation)
            where TPixel : unmanaged
        {
            return ToPixelIsPoint<TPixel>(view, view.Start, view.End, pixelSize, interpolation);
        }

        public static DemDataCellPixelIsPoint<TPixel> ToPixelIsPoint<TPixel>(this IDemDataView view, Coordinates start, Coordinates end, Vector pixelSize, IInterpolation interpolation)
            where TPixel : unmanaged
        {
            var tpixel = DemPixels.Get<TPixel>();
            var size = end - start;
            var latCount = (int)Math.Round(size.DeltaLat / pixelSize.DeltaLat)+1;
            var lonCount = (int)Math.Round(size.DeltaLon / pixelSize.DeltaLon)+1;
            var data = new TPixel[latCount, lonCount];
            Parallel.For(0, latCount, (int latIndex) =>
            {
                for (var lonIndex = 0; lonIndex < lonCount; lonIndex++)
                {
                    data[latIndex, lonIndex] = tpixel.FromDouble(view.GetLocalElevation(start + Vector.FromLatLonDelta(
                        pixelSize.DeltaLat * latIndex,
                        pixelSize.DeltaLon * lonIndex), interpolation));
                }
            });
            return new DemDataCellPixelIsPoint<TPixel>(start, pixelSize, data);
        }
    }
}

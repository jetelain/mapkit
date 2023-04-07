using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing
{
    /// <summary>
    /// Takes a large image to generate tiles for Leaflet or similar
    /// </summary>
    public static class ImageTiler
    {
        internal static int MaxZoom(Vector size, int maxTileSize = 800)
        {
            var sizeL = Math.Max(size.X, size.Y);
            var zoomLevel = 0;
            while (sizeL / (1 << zoomLevel) > maxTileSize)
            {
                zoomLevel++;
            }
            return zoomLevel;
        }

        internal static int Count(int maxZoom, int minZoom)
        {
            var count = 0;
            while (maxZoom >= minZoom)
            {
                count += (1 << maxZoom) * (1 << maxZoom);
                maxZoom--;
            }
            return count;
        }

        public static TilingInfos DefaultToJpeg(Image fullImage, string targetDirectory)
        {
            return GenerateTiles(fullImage, targetDirectory, ImageExtensions.SaveAsJpeg, "jpg");
        }

        public static TilingInfos DefaultToPng(Image fullImage, string targetDirectory)
        {
            return GenerateTiles(fullImage, targetDirectory, ImageExtensions.SaveAsPng, "png");
        }

        public static TilingInfos DefaultToWebp(Image fullImage, string targetDirectory)
        {
            return GenerateTiles(fullImage, targetDirectory, ImageExtensions.SaveAsWebp, "webp");
        }

        public static TilingInfos GenerateTiles(Image fullImage, string targetDirectory, Action<Image, string> save, string ext)
        {
            var maxZoom = MaxZoom(new Vector(fullImage.Width, fullImage.Height));
            var tileSize = fullImage.Width / (1 << maxZoom);

            var zoomLevel = maxZoom;

            while (fullImage.Width >= tileSize)
            {
                GenerateTilesAtZoomLevel(fullImage, targetDirectory, tileSize, zoomLevel, save, ext);
                fullImage.Mutate(i => i.Resize(fullImage.Width / 2, fullImage.Height / 2));
                zoomLevel--;
            }

            return new TilingInfos()
            {
                MaxZoom = maxZoom,
                TileSize = new Vector(tileSize, tileSize),
                MinZoom = 0,
                TilePattern = "{z}/{x}/{y}." + ext
            };
        }

        private static void GenerateTilesAtZoomLevel(Image fullImage, string targetDirectory, int tileSize, int zoomLevel, Action<Image,string> save, string ext)
        {
            var bounds = fullImage.Bounds();
            for (int x = 0; x < bounds.Width; x += tileSize)
            {
                for (int y = 0; y < bounds.Height; y += tileSize)
                {
                    var tile = fullImage.Clone(i => i.Crop(new Rectangle(x, y, tileSize, tileSize)));
                    var file = Path.Combine(targetDirectory, FormattableString.Invariant($"{zoomLevel}/{x / tileSize}/{y / tileSize}.{ext}"));
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    save(tile,file);
                }
            }
        }
    }
}

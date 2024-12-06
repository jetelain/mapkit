using System.Xml;
using MapToolkit.Drawing.MemoryRender;
using MapToolkit.Drawing.PdfRender;
using MapToolkit.Utils;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing
{
    public static class Render
    {
        public static void ToSvg(string file, Vector size, Action<IDrawSurface> draw, string stylePrefix = "")
        {
            using (var surface = new SvgRender.SvgSurface(XmlWriter.Create(File.CreateText(file), new XmlWriterSettings() { CloseOutput = true }), size, file, stylePrefix))
            {
                draw(surface);
            }
        }

        public static TilingInfos ToSvgTiled(string file, Vector size, SvgFallBackFormats generateWebpFallback, Action<IDrawSurface> drawLod1, Action<IDrawSurface>? drawLod2 = null, Action<IDrawSurface>? drawLod3 = null, IProgress<double>? progress = null)
        {
            var maxZoom = ImageTiler.MaxZoom(size);
            var count = ImageTiler.Count(maxZoom, Math.Max(0, maxZoom - 6));
            var rel = new BasicProgress(progress, count, 2);

            var lod1 = new MemorySurface();
            drawLod1(lod1);

            SvgTileLevel(file, maxZoom, lod1, size, rel, generateWebpFallback);

            if (maxZoom > 0)
            {
                SvgTileLevel(file, maxZoom - 1, lod1.ToScale(0.5, 0.5), size / 2, rel, generateWebpFallback);
            }
            if (maxZoom > 1)
            {
                var lod2 = GetLod(drawLod2, lod1);
                SvgTileLevel(file, maxZoom - 2, lod2.ToScale(0.25, 0.5), size / 4, rel, generateWebpFallback);

                if (maxZoom > 2)
                {
                    SvgTileLevel(file, maxZoom - 3, lod2.ToScale(0.125, 0.5), size / 8, rel, generateWebpFallback);
                }
                if (maxZoom > 3)
                { 
                    SvgTileLevel(file, maxZoom - 4, lod2.ToScale(0.0625, 0.25), size / 16, rel, generateWebpFallback);
                }
                if (maxZoom > 4)
                {
                    var lod3 = GetLod(drawLod3, lod2);
                    SvgTileLevel(file, maxZoom - 5, lod3.ToScale(0.03125, 0.25), size / 32, rel, generateWebpFallback);

                    if (maxZoom > 5)
                    {
                        SvgTileLevel(file, maxZoom - 6, lod3.ToScale(0.015625, 0.25), size / 64, rel, generateWebpFallback);
                    }
                }
            }
            return new TilingInfos()
            {
                MaxZoom = maxZoom,
                MinZoom = Math.Max(0, maxZoom - 6),
                TileSize = size / (1 << maxZoom),
                TilePattern = "{z}/{x}/{y}.svg"
            };
        }

        private static MemorySurface GetLod(Action<IDrawSurface>? drawSimpler, MemorySurface xlod)
        {
            if (drawSimpler != null)
            {
                var lod = new MemoryRender.MemorySurface();
                drawSimpler(lod);
                return lod;
            }
            return xlod;
        }


        private static WebpEncoder WebpEncoder90 = new WebpEncoder() 
        { 
            FileFormat = WebpFileFormatType.Lossy,
            Quality = 90, 
            Method = WebpEncodingMethod.BestQuality,
            TransparentColorMode = WebpTransparentColorMode.Clear 
        };

        private static void SvgTileLevel(string targetDirectory, int zoomLevel, MemorySurface surface, Vector size, BasicProgress rel, SvgFallBackFormats generateWebpFallback)
        {
            var chunks = 1 << zoomLevel;
            var tileSize = size / chunks;

            Parallel.For(0, chunks, x =>
            {
                using var image = generateWebpFallback != SvgFallBackFormats.None ? new Image<Rgba32>((int)tileSize.X, (int)tileSize.Y) : null;

                Directory.CreateDirectory(Path.Combine(targetDirectory, $"{zoomLevel}/{x}"));

                for (int y = 0; y < chunks; ++y)
                {
                    var file = Path.Combine(targetDirectory, $"{zoomLevel}/{x}/{y}.svg");
                    var pos = new Vector(tileSize.X * x, tileSize.Y * y);

                    ToSvg(file, tileSize, t => new MemDrawClipped(surface, t, pos, pos + tileSize).Draw());

                    if (image != null)
                    {
                        image.Mutate(p =>
                        {
                            p.Clear(Color.White);
                            new MemDrawClipped(surface, new ImageRender.ImageSurface(p), pos, pos + tileSize).Draw();
                        });
                        if ((generateWebpFallback & SvgFallBackFormats.Webp) != 0)
                        {
                            image.SaveAsWebp(Path.Combine(targetDirectory, $"{zoomLevel}/{x}/{y}.webp"), WebpEncoder90);
                        }
                        if ((generateWebpFallback & SvgFallBackFormats.Png) != 0)
                        {
                            image.SaveAsPng(Path.Combine(targetDirectory, $"{zoomLevel}/{x}/{y}.png"));
                        }
                    }
                    rel.AddOne();
                }
            });
        }

        public static void ToPng(string file, Vector size, Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255, 255, 255, 255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                image.SaveAsPng(file);
            }
        }

        public static void ToImage(string file, Vector size, Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255, 255, 255, 255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                image.Save(file);
            }
        }

        public static void ToImage(IImageProcessingContext target, Action<IDrawSurface> draw)
        {
            draw(new ImageRender.ImageSurface(target));
        }

        public static TilingInfos ToPngTiled(string targetDirectory, Vector size,  Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255, 255, 255, 255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                return ImageTiler.DefaultToPng(image, targetDirectory);
            }
        }

        public static void ToPdf(string file, Vector sizeInPixels, Action<IDrawSurface> draw)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = sizeInPixels.X * PaperSize.OnePixelAt300Dpi;
            page.Height = sizeInPixels.Y * PaperSize.OnePixelAt300Dpi;
            using (var xgfx = XGraphics.FromPdfPage(page))
            {
                draw(new PdfRender.PdfSurface(xgfx, PaperSize.OnePixelAt300Dpi));
            }

            document.Save(file);
        }

        public static void ToPdfGraphics(XGraphics xgfx, double pixelSize, Action<IDrawSurface> draw)
        {
            draw(new PdfRender.PdfSurface(xgfx, pixelSize));
        }
    }
}

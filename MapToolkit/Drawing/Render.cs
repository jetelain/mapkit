﻿using System;
using System.IO;
using System.Xml;
using MapToolkit.Drawing.MemoryRender;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapToolkit.Drawing
{
    public static class Render
    {
        public static void ToSvg(string file, Vector size, Action<IDrawSurface> draw)
        {
            using (var surface = new SvgRender.SvgSurface(XmlWriter.Create(File.CreateText(file)), size, file))
            {
                draw(surface);
            }
        }

        public static void ToSvgTiled(string file, Vector size, Action<IDrawSurface> draw, Action<IDrawSurface>? drawSimpler = null)
        {
            var surface = new MemoryRender.MemorySurface();
            draw(surface);

            var surface2 = new MemoryRender.MemorySurface();
            if (drawSimpler != null)
            {
                drawSimpler(surface2);
            }
            else
            {
                surface2 = surface;
            }
            SvgTileLevel(file, 4, surface, size);
            SvgTileLevel(file, 3, surface.ToScale(0.5,0.5), size / 2);
            SvgTileLevel(file, 2, surface2.ToScale(0.25,0.5), size / 4);
            SvgTileLevel(file, 1, surface2.ToScale(0.125,0.5), size / 8);
            SvgTileLevel(file, 0, surface2.ToScale(0.0625, 0.25), size / 16);
        }

        private static void SvgTileLevel(string targetDirectory, int zoomLevel, MemorySurface surface, Vector size)
        {
            var chunks = 1 << zoomLevel;

            var tileSize = size / chunks;

            Directory.CreateDirectory(Path.Combine(targetDirectory, $"{zoomLevel}"));

            for (int x = 0; x < chunks; ++x)
            {
                Directory.CreateDirectory(Path.Combine(targetDirectory, $"{zoomLevel}/{x}"));

                for (int y = 0; y < chunks; ++y)
                {
                    var file = Path.Combine(targetDirectory, $"{zoomLevel}/{x}/{y}.svg");
                    var pos = new Vector(tileSize.X * x, tileSize.Y * y);
                    ToSvg(file, tileSize, t => new MemDrawClipped(surface, t, pos, pos + tileSize).Draw());
                }
            }
        }

        public static void ToPng(string file, Vector size, Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255,255,255,255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                image.SaveAsPng(file);
            }
        }

        public static void ToPngTiled(string targetDirectory, int tileSize, Vector size,  Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255, 255, 255, 255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                PngTiles(image, targetDirectory, tileSize);
            }
        }

        private static int GetMaxZoom(int width, int tileSize)
        {
            var zoomLevel = 0;
            while (width >= tileSize)
            {
                width /= 2;
                zoomLevel++;
            }
            return zoomLevel;
        }

        private static void PngTiles(Image fullImage, string targetDirectory, int tileSize)
        {
            var zoomLevel = GetMaxZoom(fullImage.Width, tileSize);

            while (fullImage.Width >= tileSize)
            {
                PngTilesAtZoomLevel(fullImage, targetDirectory, tileSize, zoomLevel);
                fullImage.Mutate(i => i.Resize(fullImage.Width / 2, fullImage.Height / 2));
                zoomLevel--;
            }
        }
        private static void PngTilesAtZoomLevel(Image fullImage, string targetDirectory, int tileSize, int zoomLevel)
        {
            var bounds = fullImage.Bounds();
            for (int x = 0; x < bounds.Width; x += tileSize)
            {
                for (int y = 0; y < bounds.Height; y += tileSize)
                {
                    var tile = fullImage.Clone(i => i.Crop(new Rectangle(x, y, tileSize, tileSize)));
                    var file = Path.Combine(targetDirectory, $"{zoomLevel}/{x / tileSize}/{y / tileSize}.png");
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    tile.SaveAsPng(file);
                }
            }
        }
        public static void ToPdf(string file, Vector size, Action<IDrawSurface> draw)
        {
            // 300 dpi meens that 1px is 0.08466666666666666666666666666667 mm
            // In PDF world 2.8346456692913389 pt = 1 mm
            // So 1px sould be 0,24000000000000002686666666666667 pt to get 300 dpi, rounded to 0.24
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = size.X;
            page.Height = size.Y;
            draw(new PdfRender.PdfSurface(XGraphics.FromPdfPage(page), 0.24));
            document.Save(file);
        }

    }
}

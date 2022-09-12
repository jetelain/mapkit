using System;
using System.IO;
using System.Xml;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SimpleDEM.Drawing
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

        public static void ToPng(string file, Vector size, Action<IDrawSurface> draw)
        {
            using (var image = new Image<Rgba32>((int)size.X, (int)size.Y, new Rgba32(255,255,255,255)))
            {
                image.Mutate(p => draw(new ImageRender.ImageSurface(p)));
                image.SaveAsPng(file);
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

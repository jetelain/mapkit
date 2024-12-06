using Pmad.Cartography.Drawing.PdfRender;
using Pmad.Cartography.Projections;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.Topographic
{
    public static class TopoMapPdfRender
    {
        private const double LegendWidth = LegendRender.LegendWidthPoints;
        private const double LegendHeight = LegendRender.LegendHeightPoints;
        private const double LegendHalfWidth = LegendWidth / 2;
        private const double Margin = 20;

        private const double LegendWidthWithBothMargin = LegendWidth + (2 * Margin);
        private const double LegendWidthWithAllsMargins = LegendWidth + (3 * Margin);
        private const double DoubleLegendWidthWithBothMargin = LegendWidthWithBothMargin * 2;

        public static List<PdfFileInfo> RenderPDF(string targetDirectory, string name, ITopoMapData data, IProgressScope scope, int scale = 25)
        {
            var sizeInMeters = new Vector(
                data.DemDataCell.End.Longitude - data.DemDataCell.Start.Longitude,
                data.DemDataCell.End.Latitude - data.DemDataCell.Start.Latitude);

            var sizeInPoints = new Vector(
                sizeInMeters.X / scale * PaperSize.OneMilimeter,
                sizeInMeters.Y / scale * PaperSize.OneMilimeter);

            var result = new List<PdfFileInfo>();

            if (sizeInPoints.X > PaperSize.ArchEHeight || sizeInPoints.Y > PaperSize.ArchEWidth)
            {
                var paperSize = new Vector(PaperSize.ArchEHeight, PaperSize.ArchEWidth);
                var paperSurface = new Vector(PaperSize.ArchEHeight - (LegendWidth + (Margin * 3)) - 5, PaperSize.ArchEWidth - (Margin * 2));

                var tiles = GetTiles(data.DemDataCell.Start, scale, sizeInMeters, sizeInPoints, paperSurface);

                foreach (var tile in tiles)
                {
                    var subdata = data.Crop(tile.Min, tile.Max, data.Metadata.WithTitle(data.Metadata.Title + " - " + tile.Name));
                    var file = Path.Combine(targetDirectory, $"{name}_{tile.Name}.pdf");
                    result.Add(RenderSinglePdf(subdata, scale, paperSize, file, scope, data, tiles, tile));
                }
            }
            else
            {
                var paperSize = GetPaperSize(sizeInPoints.X, sizeInPoints.Y);
                var file = Path.Combine(targetDirectory, name + ".pdf");
                result.Add(RenderSinglePdf(data, scale, paperSize, file, scope));
            }
            return result;
        }

        public static PdfFileInfo RenderPDFBook(string file, ITopoMapData data, IProgressScope scope, int scale = 25)
        {
            var sizeInMeters = new Vector(
                data.DemDataCell.End.Longitude - data.DemDataCell.Start.Longitude,
                data.DemDataCell.End.Latitude - data.DemDataCell.Start.Latitude);

            var sizeInPoints = new Vector(
                sizeInMeters.X / scale * PaperSize.OneMilimeter,
                sizeInMeters.Y / scale * PaperSize.OneMilimeter);

            var paperSize = new Vector(PaperSize.A3Height, PaperSize.A3Width);

            var paperSurface = new Vector(paperSize.X - (Margin * 2), paperSize.Y - (Margin * 2));

            var tiles = GetTiles(data.DemDataCell.Start, scale, sizeInMeters, sizeInPoints, paperSurface);

            var document = new PdfDocument();
            document.Info.Title = data.Metadata.Title;
            document.Info.Creator = data.Metadata.ExportCreator;
            document.Info.Author = $"Original Map {data.Metadata.Attribution}";

            var page = document.AddPage();
            page.Width = paperSize.X;
            page.Height = paperSize.Y;

            var legendTopCenter = new Vector((paperSize.X - LegendWidth - LegendHeight - Margin) / 2, (paperSize.Y - LegendHeight) / 2);

            ToPdfPage(page, legendTopCenter, w => LegendRender.RenderLegend(w, data.Metadata, scale));
            ToPdfPage(page, legendTopCenter + new Vector(LegendWidth + Margin, 0), w => LegendRender.DrawMiniMap(w, data, tiles, null, LegendRender.LegendHeight));

            var pages = new List<PdfPageInfo>();

            foreach (var tile in tiles.OrderBy(t => t.Name))
            {
                var subdata = data.Crop(tile.Min, tile.Max, data.Metadata.WithTitle(tile.Name));

                var rdata = TopoMapRenderData.Create(subdata, scope);

                pages.Add(RenderPage(rdata, scale, paperSize, data, tiles, tile, document));
            }
            document.Save(file);

            return new PdfFileInfo(true, file, string.Empty, scale, paperSize, pages);
        }

        private static List<TopoMapPdfTile> GetTiles(CoordinatesValue start, int scale, Vector sizeInMeters, Vector sizeInPoints, Vector paperSurface)
        {
            var w = Math.Ceiling(sizeInPoints.X / paperSurface.X);
            var h = Math.Ceiling(sizeInPoints.Y / paperSurface.Y);

            var overlapInMeters = new Vector(
                (w * paperSurface.X - sizeInPoints.X) / w / PaperSize.OneMilimeter * scale,
                (h * paperSurface.Y - sizeInPoints.Y) / h / PaperSize.OneMilimeter * scale);

            var baseSize = new Vector(sizeInMeters.X / w, sizeInMeters.Y / h);

            return GetTiles(start, w, h, overlapInMeters, baseSize);
        }

        private static List<TopoMapPdfTile> GetTiles(CoordinatesValue start, double w, double h, Vector overlapInMeters, Vector baseSize)
        {
            var tiles = new List<TopoMapPdfTile>();
            for (var x = 0; x < w; ++x)
            {
                for (var y = 0; y < h; ++y)
                {
                    var name = $"{(char)('A' + x)}{h - y}";
                    var min = start + new Vector2D(baseSize.X * x, baseSize.Y * y);
                    var max = start + new Vector2D(baseSize.X * (x + 1), baseSize.Y * (y + 1));
                    if (x == 0)
                    {
                        max = max + new Vector2D(overlapInMeters.X, 0);
                    }
                    else if (x == w - 1)
                    {
                        min = min - new Vector2D(overlapInMeters.X, 0);
                    }
                    else
                    {
                        min = min - new Vector2D(overlapInMeters.X / 2, 0);
                        max = max + new Vector2D(overlapInMeters.X / 2, 0);
                    }
                    if (y == 0)
                    {
                        max = max + new Vector2D(0, overlapInMeters.Y);
                    }
                    else if (y == h - 1)
                    {
                        min = min - new Vector2D(0, overlapInMeters.Y);
                    }
                    else
                    {
                        min = min - new Vector2D(0, overlapInMeters.Y / 2);
                        max = max + new Vector2D(0, overlapInMeters.Y / 2);
                    }
                    tiles.Add(new TopoMapPdfTile(name, min, max));
                }
            }

            return tiles;
        }

        private static PdfFileInfo RenderSinglePdf(ITopoMapData data, int scale, Vector paperSize, string file, IProgressScope scope, ITopoMapData? fulldata = null, List<TopoMapPdfTile>? tiles = null, TopoMapPdfTile? current = null)
        {
            var document = new PdfDocument();
            document.Info.Title = data.Metadata.Title;
            document.Info.Creator = data.Metadata.ExportCreator;
            document.Info.Author = $"Original Map {data.Metadata.Attribution}";

            var rdata = TopoMapRenderData.Create(data, scope);

            var page = RenderPage(rdata, scale, paperSize, fulldata, tiles, current, document);
            document.Save(file);

            return new PdfFileInfo(false, file, current?.Name ?? string.Empty, scale, paperSize, [page]);
        }

        private static PdfPageInfo RenderPage(TopoMapRenderData rdata, int scale, Vector paperSizeInPoints, ITopoMapData? fulldata, List<TopoMapPdfTile>? tiles, TopoMapPdfTile? current, PdfDocument document)
        {
            var page = document.AddPage();
            page.Width = paperSizeInPoints.X;
            page.Height = paperSizeInPoints.Y;

            var projSizeInPoints = new Vector(rdata.WidthInMeters / scale * PaperSize.OneMilimeter, rdata.HeightInMeters / scale * PaperSize.OneMilimeter);

            var proj = new NoProjectionArea(rdata.Start, rdata.End, projSizeInPoints / 0.24);

            var dX = paperSizeInPoints.X - projSizeInPoints.X;
            var dY = paperSizeInPoints.Y - projSizeInPoints.Y;

            Vector mapTopLeft;
            Vector legendTopCenter;
            bool miniMap = false;
            bool drawLegend = true;

            if (dX >= LegendWidthWithAllsMargins && dX < DoubleLegendWidthWithBothMargin)
            {
                var delta = (dX - LegendWidthWithAllsMargins) / 2;
                // | Margin | ... Legend ... | Margin | ... Map ... | Margin |
                mapTopLeft = new Vector(delta + (Margin * 2) + LegendWidth, dY / 2);
                legendTopCenter = new Vector(delta + LegendHalfWidth + Margin, dY / 2);
                miniMap = true;
            }
            else
            {
                mapTopLeft = new Vector(dX / 2, dY / 2);
                if (dX < DoubleLegendWidthWithBothMargin)
                {
                    // |              ... Map ...               | 
                    // |              | ... Legend ... | Margin | 
                    legendTopCenter = new Vector(paperSizeInPoints.X - (dX / 2) - LegendHalfWidth - Margin, paperSizeInPoints.Y - (dY / 2) - LegendHeight - Margin);
                    drawLegend = paperSizeInPoints.X > PaperSize.A3Height;
                }
                else
                {
                    // | Margin | ... Legend ... | Margin | ... Map ... | Margin | ... Space for legend ...| Margin |
                    legendTopCenter = new Vector(dX / 4, dY / 2);
                    miniMap = true;
                }
            }

            ToPdfPage(page, mapTopLeft, w => new TopoMapRender(rdata, proj).RenderWithExternGraticule(w));
            if (drawLegend)
            {
                ToPdfPage(page, legendTopCenter - new Vector(LegendHalfWidth, 0), w => LegendRender.RenderLegend(w, rdata.Data.Metadata, scale));
                if (miniMap)
                {
                    ToPdfPage(page, legendTopCenter + new Vector(-LegendHalfWidth, LegendHeight + Margin), w => LegendRender.DrawMiniMap(w, fulldata ?? rdata.Data, tiles, current));
                }
            }
            else
            {
                ToPdfPage(page, mapTopLeft, w =>
                {
                    var style = w.AllocateTextStyle(new[] { "Calibri" }, SixLabors.Fonts.FontStyle.Regular, 30, new SolidColorBrush(Color.Black), null, false, TextAnchor.CenterLeft);
                    w.DrawText(new Vector(0, -30), rdata.Data.Metadata.Title, style);
                    w.DrawText(new Vector(0, proj.Size.Y + 30), rdata.Data.Metadata.Title, style);

                    style = w.AllocateTextStyle(new[] { "Calibri" }, SixLabors.Fonts.FontStyle.Regular, 30, new SolidColorBrush(Color.Black), null, false, TextAnchor.CenterRight);
                    w.DrawText(new Vector(proj.Size.X, -30), rdata.Data.Metadata.Title, style);
                    w.DrawText(new Vector(proj.Size.X, proj.Size.Y + 30), rdata.Data.Metadata.Title, style);
                });
            }

            return new PdfPageInfo(rdata.Start, rdata.End, mapTopLeft);
        }



        private static Vector GetPaperSize(double widthInPoints, double heightInPoints)
        {
            if (heightInPoints > PaperSize.A0Width)
            {
                // Arch E / Maxmimum size
                return new Vector(PaperSize.ArchEHeight, PaperSize.ArchEWidth);
            }
            if (widthInPoints > PaperSize.A1Height || heightInPoints > PaperSize.A1Width)
            {
                // A0
                return new Vector(PaperSize.A0Height, PaperSize.A0Width);
            }
            if (widthInPoints > PaperSize.A2Height || heightInPoints > PaperSize.A2Width)
            {
                // A1
                return new Vector(PaperSize.A1Height, PaperSize.A1Width);
            }
            if (widthInPoints > PaperSize.A3Height || heightInPoints > PaperSize.A3Width)
            {
                // A2
                return new Vector(PaperSize.A2Height, PaperSize.A2Width);
            }
            // A3
            return new Vector(PaperSize.A3Height, PaperSize.A3Width);
        }


        public static void ToPdfPage(PdfPage page, Vector shiftInPoints, Action<IDrawSurface> draw)
        {
            using (var xgfx = XGraphics.FromPdfPage(page))
            {
                xgfx.TranslateTransform(shiftInPoints.X, shiftInPoints.Y);
                Drawing.Render.ToPdfGraphics(xgfx, PaperSize.OnePixelAt300Dpi, draw);
            }
        }
    }
}

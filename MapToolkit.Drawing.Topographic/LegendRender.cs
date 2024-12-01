using MapToolkit.Drawing;
using MapToolkit.Drawing.PdfRender;
using MapToolkit.Projections;
using Pmad.Geometry.Collections;
using Pmad.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    internal class LegendRender
    {
        public const double LegendWidthPoints = LegendWidth * PaperSize.OnePixelAt300Dpi;
        public const double LegendHeightPoints = LegendHeight * PaperSize.OnePixelAt300Dpi;

        // Pixels

        public const double LegendWidth = 1300;
        public const double LegendHeight = 2000;
        private const double InsideMargin = 40;

        private const double LegendHalfWidth = LegendWidth / 2;

        private const double LegendContentStart = InsideMargin;
        private const double LegendContentCenter = LegendHalfWidth;
        private const double LegendContentEnd = LegendWidth - InsideMargin;
        private const double LegendContentWidth = LegendContentEnd - InsideMargin;

        private const double LegendContentP1 = LegendContentCenter;
        private const double LegendContentW = (LegendContentEnd - LegendContentCenter - InsideMargin * 2) / 3;
        private const double LegendContentP2 = LegendContentP1 + LegendContentW;
        private const double LegendContentP3 = LegendContentP2 + InsideMargin;
        private const double LegendContentP4 = LegendContentP3 + LegendContentW;
        private const double LegendContentP5 = LegendContentP4 + InsideMargin;
        private const double LegendContentP6 = LegendContentEnd;

        public static void RenderLegend(IDrawSurface w, TopoMapMetadata data, int scale)
        {
            var style = TopoMapStyle.CreateFull(w, ColorPalette.Default, true);

            var titleFontSize = 160f;
            var titleSize = TextMeasurer.MeasureSize(data.Title, new TextOptions(SystemFonts.Collection.Get("Calibri").CreateFont(titleFontSize)));
            if ( titleSize.Width > LegendContentWidth)
            {
                titleFontSize = (float)(titleFontSize * LegendContentWidth / titleSize.Width);
            }

            var titleText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, titleFontSize, new SolidColorBrush(Color.Black), null, false, TextAnchor.TopCenter);
            var subTitleText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 60, new SolidColorBrush(Color.Black), null, false, TextAnchor.TopCenter);
            var normalText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 32, new SolidColorBrush(Color.Black), null, false, TextAnchor.TopCenter);
            var smallText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 25, new SolidColorBrush(Color.Black), null, false, TextAnchor.TopCenter);
            var normalTextLeft = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 32, new SolidColorBrush(Color.Black), null, false, TextAnchor.CenterLeft);

            w.DrawRectangle(new Vector(0, 0), new Vector(LegendWidth, LegendHeight), w.AllocateStyle(Color.White, Color.Black, 2));

            w.DrawText(new Vector(LegendContentCenter, 20), data.UpperTitle, subTitleText);
            w.DrawText(new Vector(LegendContentCenter, 90), data.Title, titleText);
            if (scale == -1)
            {
                w.DrawText(new Vector(LegendContentCenter, 300), $"Dynamic scale : see bottom left of map", normalText);
            }
            else
            {
                w.DrawText(new Vector(LegendContentCenter, 300), $"Scale 1 : {scale} 000 - 1 cm = {scale * 10} m", normalText);
            }
            w.DrawText(new Vector(LegendContentCenter, 345), $"Version {DateTime.Now:yyyy-MM-dd}", smallText);

            RoadLegend(w, style, normalTextLeft, 500, TopoMapPathType.Main, "Main road");
            RoadLegend(w, style, normalTextLeft, 560, TopoMapPathType.Secondary, "Secondary road");
            RoadLegend(w, style, normalTextLeft, 620, TopoMapPathType.Road, "City road, Other road");
            RoadLegend(w, style, normalTextLeft, 680, TopoMapPathType.Track, "Vehicle track");
            RoadLegend(w, style, normalTextLeft, 740, TopoMapPathType.Trail, "Foot trail");

            w.DrawText(new Vector(LegendContentStart, 800), "Bridges Main, Secondary, Road", normalTextLeft);
            RenderBridge(w, style, TopoMapPathType.Main, new Vector(LegendContentP1, 800), new Vector(LegendContentP2, 800));
            RenderBridge(w, style, TopoMapPathType.Secondary, new Vector(LegendContentP3, 800), new Vector(LegendContentP4, 800));
            RenderBridge(w, style, TopoMapPathType.Road, new Vector(LegendContentP5, 800), new Vector(LegendContentP6, 800));

            w.DrawText(new Vector(LegendContentStart, 860), "Forest, Rocks, Water", normalTextLeft);
            w.DrawRectangle(new Vector(LegendContentP1, 840), new Vector(LegendContentP2, 880), style.forest);
            w.DrawRectangle(new Vector(LegendContentP3, 840), new Vector(LegendContentP4, 880), style.rocks);
            w.DrawRectangle(new Vector(LegendContentP5, 840), new Vector(LegendContentP6, 880), style.water);

            w.DrawText(new Vector(LegendContentStart, 920), "Wind turbine, Water tower, Transmitter", normalTextLeft);
            w.DrawIcon(new Vector((LegendContentP1 + LegendContentP2) / 2, 920), style.GetIcon(TopoIconType.WindPowerPlant)!);
            w.DrawIcon(new Vector((LegendContentP3 + LegendContentP4) / 2, 920), style.GetIcon(TopoIconType.WaterTower)!);
            w.DrawIcon(new Vector((LegendContentP5 + LegendContentP6) / 2, 920), style.GetIcon(TopoIconType.Transmitter)!);
            
            w.DrawText(new Vector(LegendContentStart, 980), "Hospital, Church", normalTextLeft);
            w.DrawIcon(new Vector((LegendContentP1 + LegendContentP2) / 2, 980), style.GetIcon(TopoIconType.Hospital)!);
            w.DrawIcon(new Vector((LegendContentP3 + LegendContentP4) / 2, 980), style.GetIcon(TopoIconType.Church)!);

            w.DrawText(new Vector(LegendContentStart, 1040), "Railway, Pylon, Powerline", normalTextLeft);
            if (style.Railway != null)
            {
                TopoMapRender.DrawRailway(w, style.Railway, [new Vector(LegendContentP1, 1040), new Vector(LegendContentP2, 1040)]);
            }
            w.DrawIcon(new Vector((LegendContentP3 + LegendContentP4) / 2, 1040), style.GetIcon(TopoIconType.ElectricityPylon)!);
            if (style.Powerline != null)
            {
                w.DrawPolyline([new Vector(LegendContentP5, 1040), new Vector(LegendContentP6, 1040)], style.Powerline);
            }


            w.DrawText(new Vector(LegendContentCenter, 1810), $"Original Map {data.Attribution}", smallText);
            w.DrawText(new Vector(LegendContentCenter, 1890), data.ExportCreator, smallText);
            if (!string.IsNullOrEmpty(data.LicenseNotice))
            {
                w.DrawText(new Vector(LegendContentCenter, 1930), data.LicenseNotice, smallText);
            }
        }

        private static void RenderBridge(IDrawSurface w, TopoMapStyle style, TopoMapPathType type, Vector begin, Vector end)
        {
            var bg = style.roadBackground[(int)type];
            var fg = style.roadForeground[(int)type];
            var bbg = style.bridgeBackground[(int)type];
            var bfg = style.bridgeForeground[(int)type];
            if (bg != null)
            {
                w.DrawPolyline(new[] { begin, end }, bg);
            }
            if (fg != null)
            {
                w.DrawPolyline(new[] { begin, end }, fg);
            }

            var bbegin = (begin * 2 + end) / 3;
            var bend = (begin + end * 2) / 3;

            if (bbg != null)
            {
                w.DrawPolyline(new[] { bbegin, bend }, bbg);

                TopoMapRender.BridgeLimit(w, bbegin, bend, style.BridgeLimit);
                TopoMapRender.BridgeLimit(w, bend, bbegin, style.BridgeLimit);
            }
            if (bfg != null)
            {
                w.DrawPolyline(new[] { bbegin, bend }, bfg);
            }
        }

        private static void RoadLegend(IDrawSurface w, TopoMapStyle style, IDrawTextStyle textStyle, double top, TopoMapPathType type, string label)
        {
            w.DrawText(new Vector(LegendContentStart, top), label, textStyle);

            var bg = style.roadBackground[(int)type];
            var fg = style.roadForeground[(int)type];
            if (bg != null)
            {
                w.DrawPolyline(new[] { new Vector(LegendContentP1, top), new Vector(LegendContentP6, top) }, bg);
            }
            if (fg != null)
            {
                w.DrawPolyline(new[] { new Vector(LegendContentP1, top), new Vector(LegendContentP6, top) }, fg);
            }
        }

        public static void DrawMiniMap(IDrawSurface w, ITopoMapData data, List<TopoMapPdfTile>? tiles, TopoMapPdfTile? current, double size = LegendWidth)
        {
            var projection = new NoProjectionArea(data.DemDataCell.Start, data.DemDataCell.End, new Vector(size, size));

            var tileNameText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 12 / 0.24, new SolidColorBrush(Color.Black), null, false, TextAnchor.BottomCenter);
            var cityNameText = w.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Regular, 8 / 0.24, new SolidColorBrush(Color.Black), null, false, TextAnchor.CenterLeft);

            var cityCircle = w.AllocatePenStyle(Color.Black, 1);
            var tileLimit = w.AllocatePenStyle(Color.Gray, 2);
            var currentTile = w.AllocateStyle(new SolidColorBrush(Color.ParseHex("FFFF0080")), new Pen(Color.Gray, 4));

            if (data.ForestPolygons != null)
            {
                TopoMapRender.DrawPolygonsSimplified(w, projection, data.ForestPolygons, w.AllocateBrushStyle("D1FEB9"));
            }
            if (data.RockPolygons != null)
            {
                TopoMapRender.DrawPolygonsSimplified(w, projection, data.RockPolygons, w.AllocateBrushStyle("C0C0C080"));
            }
            if (data.WaterPolygons != null)
            {
                TopoMapRender.DrawPolygonsSimplified(w, projection, data.WaterPolygons, w.AllocateBrushStyle("B3D9FE"));
            }

            var mains = data.Roads?[TopoMapPathType.Main];
            if (mains != null)
            {
                var mr = w.AllocatePenStyle("FE002C", 2);
                foreach (var road in mains)
                {
                    var simpler = LevelOfDetailHelper.SimplifyAnglesAndDistances(projection.Project(road.Points.AsSpan<Vector2D, CoordinatesS>()).ToList(), 1);
                    if (simpler.Count > 0)
                    {
                        w.DrawPolyline(simpler, mr);
                    }
                }
            }
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    if (tile != current)
                    {
                        w.DrawPolyline(GetTile(projection, tile), tileLimit);
                    }
                }
            }
            if (current != null)
            {
                w.DrawPolygon(GetTile(projection, current), currentTile);
            }

            if (data.Names != null)
            {
                foreach (var city in data.Names)
                {
                    if (city.Type == TopoLocationType.City || city.Type == TopoLocationType.Local || city.Type == TopoLocationType.Village)
                    {
                        var pc = projection.Project(city.Position);
                        w.DrawCircle(pc, 1.5f, cityCircle);
                        w.DrawText(pc + new Vector(15, 0), city.Name, cityNameText);
                    }
                }
            }

            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    w.DrawText((projection.Project(tile.Min) + projection.Project(tile.Max)) / 2, tile.Name, tileNameText);
                }
            }

            w.DrawRectangle(new Vector(0, 0), new Vector(size, size), w.AllocatePenStyle(Color.Black, 2));
        }

        private static Vector[] GetTile(NoProjectionArea projection, TopoMapPdfTile tile)
        {
            return new[]{
                        projection.Project(tile.Min),
                        projection.Project(new CoordinatesS(tile.Min.Latitude, tile.Max.Longitude)),
                        projection.Project(tile.Max),
                        projection.Project(new CoordinatesS(tile.Max.Latitude, tile.Min.Longitude)),
                        projection.Project(tile.Min)};
        }
    }
}

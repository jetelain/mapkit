using System.Globalization;
using GeoJSON.Text.Geometry;
using MapToolkit.Drawing;
using MapToolkit.Drawing.Contours;
using MapToolkit.Projections;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    public static class TopoMapRender
    {
        public static void Render(IDrawSurface writer, TopoMapRenderData renderData, NoProjectionArea proj)
        {
            var style = TopoMapStyle.CreateFull(writer, ColorPalette.Default);

            RenderAny(writer, renderData, proj, style);

            NaiveGraticule(writer, proj, style);
        }

        public static void RenderWithExternGraticule(IDrawSurface writer, TopoMapRenderData renderData, NoProjectionArea proj)
        {
            var style = TopoMapStyle.CreateFull(writer, ColorPalette.Default, true);

            RenderAny(writer, renderData, proj, style);

            NaiveGraticule(writer, proj, style);
        }

        public static void RenderLod2(IDrawSurface writer, TopoMapRenderData renderData, NoProjectionArea proj)
        {
            var style = TopoMapStyle.CreateLod2(writer, ColorPalette.Default);

            RenderAny(writer, renderData, proj, style, true);

            NaiveGraticule(writer, proj, style);
        }

        public static void RenderLod3(IDrawSurface writer, TopoMapRenderData renderData, NoProjectionArea proj)
        {
            var style = TopoMapStyle.CreateLod3(writer, ColorPalette.Default);
            var data = renderData.Data;

            if (data.ForestPolygons != null)
            {
                DrawPolygons(writer, proj, data.ForestPolygons, style.forest);
            }

            if (data.RockPolygons != null)
            {
                DrawPolygons(writer, proj, data.RockPolygons, style.rocks);
            }

            if (renderData.Img != null)
            {
                writer.DrawImage(renderData.Img, Vector.Zero, proj.Size, 0.5);
            }

            if (data.WaterPolygons != null)
            {
                DrawPolygons(writer, proj, data.WaterPolygons, style.water);
            }

            if (data.Roads != null)
            {
                RenderRoads(writer, data.Roads, proj, style.roadForeground, style.roadBackground);
            }

            if (data.Bridges != null)
            {
                RenderBridgesRoads(writer, data.Bridges, proj, style.bridgeForeground, style.bridgeBackground, style.BridgeLimit);
            }

            RenderNames(writer, data, proj, style);

            NaiveGraticule(writer, proj, style, 10000);
        }

        private static void RenderAny(IDrawSurface writer, TopoMapRenderData renderData, NoProjectionArea proj, TopoMapStyle style, bool simpler = false)
        {
            var data = renderData.Data;

            if (data.ForestPolygons != null)
            {
                DrawPolygons(writer, proj, data.ForestPolygons, style.forest);
            }
            if (data.RockPolygons != null)
            {
                DrawPolygons(writer, proj, data.RockPolygons, style.rocks);
            }

            var render = new ContourRender(writer, style.contourStyle);
            if (simpler)
            {
                render.RenderSimpler(renderData.Contour, proj, renderData.Img);
            }
            else
            {
                render.Render(renderData.Contour, proj, renderData.Img);
            }

            if (data.WaterPolygons != null)
            {
                DrawPolygons(writer, proj, data.WaterPolygons, style.water);
            }

            if (data.BuildingPolygons != null && style.buildings != null)
            {
                DrawPolygons(writer, proj, data.BuildingPolygons, style.buildings);
            }

            if (data.Roads != null)
            {
                RenderRoads(writer, data.Roads, proj, style.roadForeground, style.roadBackground);
            }

            if (data.FortPolygons != null && style.Forts != null)
            {
                DrawPolygons(writer, proj, data.FortPolygons, style.Forts);
            }

            if (data.Bridges != null)
            {
                RenderBridgesRoads(writer, data.Bridges, proj, style.bridgeForeground, style.bridgeBackground, style.BridgeLimit);
            }

            if (data.Powerlines != null && style.Powerline != null)
            {
                foreach (var pl in data.Powerlines.Coordinates)
                {
                    writer.DrawPolyline(pl.Coordinates.Select(p => proj.Project(p)), style.Powerline);
                }
            }

            if (renderData.PlottedPoints != null && style.plotted != null && style.plottedCircle != null)
            {
                RenderPlotted(writer, proj, renderData.PlottedPoints, style.plotted, style.plottedCircle);
            }

            if (data.Icons != null)
            {
                foreach (var icon in data.Icons)
                {
                    var i = GetIcon(style, icon.MapType);
                    if (i != null)
                    {
                        writer.DrawIcon(proj.Project(icon.Coordinates), i);
                    }
                }
            }

            RenderNames(writer, data, proj, style);
        }

        private static void RenderNames(IDrawSurface writer, ITopoMapData data, NoProjectionArea proj, TopoMapStyle style)
        {
            if (data.Names != null)
            {
                foreach (var entry in data.Names)
                {
                    var pos = proj.Project(entry.Position);
                    var nstyle = style.name;
                    if (entry.Type == TopoLocationType.City)
                    {
                        nstyle = style.city;
                    }
                    else if (entry.Type == TopoLocationType.Local)
                    {
                        nstyle = style.local;
                    }
                    if (nstyle != null)
                    {
                        writer.DrawText(pos, entry.Name, nstyle);
                    }
                }
            }
        }

        private static IDrawIcon? GetIcon(TopoMapStyle style, TopoIconType mapType)
        {
            switch (mapType)
            {
                case TopoIconType.WindPowerPlant:
                    return style.WindPowerPlant;

                case TopoIconType.WaterTower:
                    return style.WaterTower;

                case TopoIconType.Transmitter:
                    return style.Transmitter;

                case TopoIconType.ElectricityPylon:
                    return style.Dot;

                case TopoIconType.Hospital:
                    return style.Hospital;
            }
            return null;
        }

        private static void RenderPlotted(IDrawSurface writer, NoProjectionArea proj, List<DemDataPoint> plottedPoints, IDrawTextStyle plotted, IDrawStyle plottedCircle)
        {
            foreach (var point in plottedPoints)
            {
                var projected = proj.Project(point.Coordinates);
                var text = Math.Round(point.Elevation).ToString(CultureInfo.InvariantCulture);

                writer.DrawCircle(projected, 1.5f, plottedCircle);
                writer.DrawText(projected + new Vector(3, 0), text, plotted);

            }
        }

        private static void NaiveGraticule(IDrawSurface writer, NoProjectionArea proj, TopoMapStyle style, int step = 1000)
        {
            var format = "00";
            if (step == 10000)
            {
                format = "0";
            }
            for (int x = 1; x < 100; ++x)
            {
                var projected = proj.Project(new Coordinates(0, x * step));
                if (projected.X >= 0 && projected.X <= proj.Size.X)
                {
                    writer.DrawPolyline(new[] {
                        new Vector(projected.X, 0),
                        new Vector(projected.X, proj.Size.Y),
                    }, x % 10 == 0 ? style.gl2 : style.gl1);

                    writer.DrawText(new Vector(projected.X, 0), x.ToString(format), style.gltT);
                    writer.DrawText(new Vector(projected.X, proj.Size.Y), x.ToString(format), style.gltB);
                }
            }
            for (int y = 1; y < 100; ++y)
            {
                var projected = proj.Project(new Coordinates(y * step, 0));
                if (projected.Y >= 0 && projected.Y <= proj.Size.Y)
                {
                    writer.DrawPolyline(new[] {
                        new Vector(0, projected.Y),
                        new Vector(proj.Size.X, projected.Y),
                    }, y % 10 == 0 ? style.gl2 : style.gl1);

                    writer.DrawText(new Vector(0, projected.Y), y.ToString(format), style.gltL);
                    writer.DrawText(new Vector(proj.Size.X, projected.Y), y.ToString(format), style.gltR);
                }
            }
        }

        private static void RenderRoads(IDrawSurface writer, Dictionary<TopoMapPathType, MultiLineString> data, NoProjectionArea proj, IDrawStyle?[] foreground, IDrawStyle?[] background)
        {
            foreach (var roads in data)
            {
                var style = background[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value.Coordinates)
                    {
                        writer.DrawPolyline(road.Coordinates.Select(p => proj.Project(p)), style);
                    }
                }
            }
            foreach (var roads in data.OrderByDescending(t => t.Key))
            {
                var style = foreground[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value.Coordinates)
                    {
                        writer.DrawPolyline(road.Coordinates.Select(p => proj.Project(p)), style);
                    }
                }
            }
        }

        private static void RenderBridgesRoads(IDrawSurface writer, Dictionary<TopoMapPathType, MultiLineString> data, NoProjectionArea proj, IDrawStyle?[] foreground, IDrawStyle?[] background, IDrawStyle limitLine)
        {
            foreach (var roads in data)
            {
                var style = background[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value.Coordinates)
                    {
                        writer.DrawPolyline(road.Coordinates.Select(p => proj.Project(p)), style);

                        BridgeLimit(writer, proj.Project(road.Coordinates[0]), proj.Project(road.Coordinates[1]), limitLine);
                        BridgeLimit(writer, proj.Project(road.Coordinates[road.Coordinates.Count - 1]), proj.Project(road.Coordinates[road.Coordinates.Count - 2]), limitLine);
                    }
                }
            }
            foreach (var roads in data.OrderByDescending(t => t.Key))
            {
                var style = foreground[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value.Coordinates)
                    {
                        writer.DrawPolyline(road.Coordinates.Select(p => proj.Project(p)), style);
                    }
                }
            }
        }

        internal static void BridgeLimit(IDrawSurface writer, Vector begin, Vector inside, IDrawStyle limitLine)
        {
            var delta = System.Numerics.Vector2.Normalize((inside - begin).ToFloat());
            var d8 = delta * 10;
            var d4 = delta * 4.5f;
            var x1 = System.Numerics.Vector2.Transform(d8, System.Numerics.Matrix3x2.CreateRotation((float)(3 * Math.PI / 4)));
            var x2 = System.Numerics.Vector2.Transform(d8, System.Numerics.Matrix3x2.CreateRotation((float)(-3 * Math.PI / 4)));
            writer.DrawPolyline(new[] { begin + new Vector(d4), begin + new Vector(x1 + d4) }, limitLine);
            writer.DrawPolyline(new[] { begin + new Vector(d4), begin + new Vector(x2 + d4) }, limitLine);
        }

        private static void DrawPolygons(IDrawSurface writer, NoProjectionArea proj, MultiPolygon surface, IDrawStyle styleToUse)
        {
            foreach (var poly in surface.Coordinates)
            {
                writer.DrawPolygon(
                    poly.Coordinates[0].Coordinates.Cast<Coordinates>().Select(proj.Project),
                    poly.Coordinates.Skip(1).Select(l => l.Coordinates.Cast<Coordinates>().Select(proj.Project)),
                    styleToUse);
            }
        }

        internal static void DrawPolygonsSimplified(IDrawSurface writer, NoProjectionArea proj, MultiPolygon surface, IDrawStyle styleToUse)
        {
            foreach (var poly in surface.Coordinates)
            {
                var contour = LevelOfDetailHelper.SimplifyAnglesAndDistancesClosed(poly.Coordinates[0].Coordinates.Cast<Coordinates>().Select(proj.Project), 1);

                if (contour.Count > 0)
                {
                    writer.DrawPolygon(
                        contour,
                        LevelOfDetailHelper.SimplifyAnglesAndDistancesClosed(poly.Coordinates.Skip(1).Select(l => l.Coordinates.Cast<Coordinates>().Select(proj.Project)), 1) ?? Enumerable.Empty<IEnumerable<Vector>>(),
                        styleToUse);
                }
            }
        }
    }
}

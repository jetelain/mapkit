using System.Globalization;
using System.Numerics;
using GeoJSON.Text.Geometry;
using MapToolkit.Drawing.Contours;
using MapToolkit.Projections;
using Pmad.Geometry;
using Pmad.Geometry.Algorithms;
using Pmad.Geometry.Collections;
using Pmad.Geometry.Shapes;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    public class TopoMapRender
    {
        private readonly TopoMapRenderData renderData;
        private readonly ITopoMapData data;
        private readonly NoProjectionArea proj;

        public TopoMapRender(TopoMapRenderData renderData, NoProjectionArea proj)
        {
            this.renderData = renderData;
            this.data = renderData.Data;
            this.proj = proj;
        }

        public void Render(IDrawSurface writer)
        {
            var style = TopoMapStyle.CreateFull(writer, ColorPalette.Default);

            RenderAny(writer, style);

            NaiveGraticule(writer, style);
        }

        public void RenderWithExternGraticule(IDrawSurface writer)
        {
            var style = TopoMapStyle.CreateFull(writer, ColorPalette.Default, true);

            RenderAny(writer, style);

            NaiveGraticule(writer, style);
        }

        public void RenderLod2(IDrawSurface writer)
        {
            var style = TopoMapStyle.CreateLod2(writer, ColorPalette.Default);

            RenderAny(writer, style, true);

            NaiveGraticule(writer, style);
        }

        public void RenderLod3(IDrawSurface writer)
        {
            var style = TopoMapStyle.CreateLod3(writer, ColorPalette.Default);
            var data = renderData.Data;

            if (data.ForestPolygons != null)
            {
                DrawPolygons(writer, data.ForestPolygons, style.forest);
            }

            if (data.RockPolygons != null)
            {
                DrawPolygons(writer, data.RockPolygons, style.rocks);
            }

            if (renderData.Img != null)
            {
                writer.DrawImage(renderData.Img, Vector.Zero, proj.Size, 0.5);
            }

            if (data.WaterPolygons != null)
            {
                DrawPolygons(writer, data.WaterPolygons, style.water);
            }

            if (data.Roads != null)
            {
                RenderRoads(writer, data.Roads, style.roadForeground, style.roadBackground);
            }

            if (data.Bridges != null)
            {
                RenderBridgesRoads(writer, data.Bridges, proj, style.bridgeForeground, style.bridgeBackground, style.BridgeLimit);
            }

            RenderNames(writer, style);

            NaiveGraticule(writer, style, 10000);
        }

        private void RenderAny(IDrawSurface writer, TopoMapStyle style, bool simpler = false)
        {
            var data = renderData.Data;

            if (data.ForestPolygons != null)
            {
                DrawPolygons(writer, data.ForestPolygons, style.forest);
            }
            if (data.RockPolygons != null)
            {
                DrawPolygons(writer, data.RockPolygons, style.rocks);
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
                DrawPolygons(writer, data.WaterPolygons, style.water);
            }

            if (data.BuildingPolygons != null && style.buildings != null)
            {
                DrawPolygons(writer, data.BuildingPolygons, style.buildings);
            }

            if (data.Roads != null)
            {
                RenderRoads(writer, data.Roads, style.roadForeground, style.roadBackground);
            }

            if (data.FortPolygons != null && style.Forts != null)
            {
                DrawPolygons(writer, data.FortPolygons, style.Forts);
            }

            if (data.Bridges != null)
            {
                RenderBridgesRoads(writer, data.Bridges, proj, style.bridgeForeground, style.bridgeBackground, style.BridgeLimit);
            }

            if (data.Powerlines != null && style.Powerline != null)
            {
                foreach (var pl in data.Powerlines)
                {
                    writer.DrawPolyline(proj.Project(pl.Points.AsSpan<Vector2D, CoordinatesValue>()), style.Powerline);
                }
            }
            if (data.Railways != null && style.Railway != null)
            {
                foreach (var pl in data.Railways)
                {
                    DrawRailway(writer, style.Railway, proj.Project(pl.Points.AsSpan<Vector2D, CoordinatesValue>()));
                }
            }
            if (renderData.PlottedPoints != null && style.plotted != null && style.plottedCircle != null)
            {
                RenderPlotted(writer, renderData.PlottedPoints, style.plotted, style.plottedCircle);
            }

            if (data.Icons != null)
            {
                foreach (var icon in data.Icons)
                {
                    var i = style.GetIcon(icon.MapType);
                    if (i != null)
                    {
                        writer.DrawIcon(proj.Project(icon.Coordinates), i);
                    }
                }
            }

            RenderNames(writer, style);
        }

        internal static void DrawRailway(IDrawSurface writer, IDrawStyle railway, IEnumerable<Vector> points)
        {
            writer.DrawPolyline(points, railway);
            var x = new PathFollower<double,Vector2D>(points.Select(p => p.Vector2D));

            if (x.Move(5))
            {
                var normal = Vector2D.Normalize(x.Delta);
                writer.DrawPolyline([new(x.Current + (normal.Rotate90() * 2.5)), new(x.Current + (normal.RotateM90() * 2.5))], railway);

                while (x.Move(75) && !x.IsLast)
                {
                    normal = Vector2D.Normalize(x.Delta);
                    writer.DrawPolyline([new (x.Current + (normal.Rotate90() * 2.5)), new(x.Current + (normal.RotateM90() * 2.5))], railway);
                }
            }
        }

        private void RenderNames(IDrawSurface writer, TopoMapStyle style)
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

        private void RenderPlotted(IDrawSurface writer, List<DemDataPoint> plottedPoints, IDrawTextStyle plotted, IDrawStyle plottedCircle)
        {
            foreach (var point in plottedPoints)
            {
                var projected = proj.Project(point.CoordinatesS);
                var text = Math.Round(point.Elevation).ToString(CultureInfo.InvariantCulture);

                writer.DrawCircle(projected, 1.5f, plottedCircle);
                writer.DrawText(projected + new Vector(3, 0), text, plotted);

            }
        }

        private void NaiveGraticule(IDrawSurface writer, TopoMapStyle style, int step = 1000)
        {
            var format = "00";
            if (step == 10000)
            {
                format = "0";
            }
            for (int x = 1; x < 100; ++x)
            {
                var projected = proj.Project(new CoordinatesValue(0, x * step));
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
                var projected = proj.Project(new CoordinatesValue(y * step, 0));
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

        private void RenderRoads(IDrawSurface writer, Dictionary<TopoMapPathType, MultiPath<double,Vector2D>> data, IDrawStyle?[] foreground, IDrawStyle?[] background)
        {
            foreach (var roads in data)
            {
                var style = background[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value)
                    {
                        writer.DrawPolyline(proj.Project(road.Points.AsSpan<Vector2D, CoordinatesValue>()), style);
                    }
                }
            }
            foreach (var roads in data.OrderByDescending(t => t.Key))
            {
                var style = foreground[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value)
                    {
                        writer.DrawPolyline(proj.Project(road.Points.AsSpan<Vector2D, CoordinatesValue>()), style);
                    }
                }
            }
        }

        private void RenderBridgesRoads(IDrawSurface writer, Dictionary<TopoMapPathType, MultiPath<double,Vector2D>> data, NoProjectionArea proj, IDrawStyle?[] foreground, IDrawStyle?[] background, IDrawStyle limitLine)
        {
            foreach (var roads in data)
            {
                var style = background[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value)
                    {
                        writer.DrawPolyline(proj.Project(road.Points.AsSpan<Vector2D, CoordinatesValue>()), style);

                        BridgeLimit(writer, proj.Project(new CoordinatesValue(road.Points[0])), proj.Project(new CoordinatesValue(road.Points[1])), limitLine);
                        BridgeLimit(writer, proj.Project(new CoordinatesValue(road.Points[road.Points.Count - 1])), proj.Project(new CoordinatesValue(road.Points[road.Points.Count - 2])), limitLine);
                    }
                }
            }
            foreach (var roads in data.OrderByDescending(t => t.Key))
            {
                var style = foreground[(int)roads.Key];
                if (style != null)
                {
                    foreach (var road in roads.Value)
                    {
                        writer.DrawPolyline(proj.Project(road.Points.AsSpan<Vector2D, CoordinatesValue>()), style);
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

        private void DrawPolygons(IDrawSurface writer, MultiPolygon<double, Vector2D> surface, IDrawStyle styleToUse)
        {
            foreach (var poly in surface)
            {
                writer.DrawPolygon(
                    proj.Project(poly.Shell.AsSpan<Vector2D, CoordinatesValue>()),
                    poly.Holes.Select(l => proj.Project(l.AsSpan<Vector2D, CoordinatesValue>())),
                    styleToUse);
            }
        }

        internal static void DrawPolygonsSimplified(IDrawSurface writer, NoProjectionArea proj, MultiPolygon<double,Vector2D> surface, IDrawStyle styleToUse)
        {
            foreach (var poly in surface)
            {
                var contour = LevelOfDetailHelper.SimplifyAnglesAndDistancesClosed(proj.Project(poly.Shell.AsSpan<Vector2D,CoordinatesValue>()), 1);

                if (contour.Count > 0)
                {
                    writer.DrawPolygon(
                        contour.ToArray(),
                        LevelOfDetailHelper.SimplifyAnglesAndDistancesClosed(poly.Holes.Select(l => proj.Project(l.AsSpan<Vector2D, CoordinatesValue>()).ToArray()), 1),
                        styleToUse);
                }
            }
        }
    }
}

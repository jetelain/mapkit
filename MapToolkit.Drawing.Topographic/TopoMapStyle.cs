using MapToolkit.Drawing;
using MapToolkit.Drawing.Contours;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    internal class TopoMapStyle
    {
        public IDrawIcon? WindPowerPlant { get; private set; }
        public required IDrawStyle rocks { get; internal set; }
        public required IDrawStyle forest { get; internal set; }
        public required IDrawStyle water { get; internal set; }
        public IDrawStyle? buildings { get; private set; }
        public required ContourRenderStyle contourStyle { get; internal set; }
        public required IDrawStyle?[] roadForeground { get; internal set; }
        public required IDrawStyle?[] roadBackground { get; internal set; }
        public required IDrawStyle?[] bridgeForeground { get; internal set; }
        public required IDrawStyle?[] bridgeBackground { get; internal set; }
        public required IDrawStyle gl1 { get; internal set; }
        public required IDrawStyle gl2 { get; internal set; }
        public required IDrawTextStyle gltT { get; internal set; }
        public required IDrawTextStyle gltB { get; internal set; }
        public required IDrawTextStyle gltL { get; internal set; }
        public required IDrawTextStyle gltR { get; internal set; }
        public required IDrawTextStyle city { get; internal set; }
        public IDrawTextStyle? local { get; private set; }
        public IDrawTextStyle? name { get; private set; }
        public IDrawTextStyle? plotted { get; private set; }
        public IDrawStyle? plottedCircle { get; private set; }
        public IDrawIcon? WaterTower { get; private set; }
        public IDrawIcon? TechnicalTower { get; internal set; }
        public IDrawIcon? Transmitter { get; private set; }
        public IDrawStyle? Powerline { get; private set; }
        public IDrawIcon? Dot { get; private set; }
        public IDrawIcon? Hospital { get; private set; }
        public required IDrawStyle BridgeLimit { get; internal set; }
        public IDrawStyle? Forts { get; internal set; }

        public static TopoMapStyle CreateFull(IDrawSurface writer, ColorPalette palette, bool externGraticule = false)
        {
            var rocksFill = writer.AllocateBrushStyle(palette.RocksSymbol);
            var rocks = writer.AllocateStyle(new VectorBrush(writer, 20, 20, d =>
            {
                d.DrawCircle(new Vector(5, 5), 3, rocksFill);
                d.DrawCircle(new Vector(15, 15), 3, rocksFill);
            }), new Pen(new SolidColorBrush(palette.RocksBorder)));

            var forest = writer.AllocateStyle(palette.ForestFill, palette.ForestBorder);
            var water = writer.AllocateStyle(palette.WaterFill, palette.WaterBorder);
            var buildings = writer.AllocateStyle(palette.BuildingFill, palette.BuildingBorder);

            var contourStyle = new ContourRenderStyle(writer);

            var r1 = writer.AllocatePenStyle(palette.MainRoad, 5);
            var b1 = writer.AllocatePenStyle(palette.RoadBorder, 6);
            var bx1 = writer.AllocatePenStyle(palette.RoadBorder, 10);
            var r2 = writer.AllocatePenStyle(palette.SecondaryRoad, 4);
            var b2 = writer.AllocatePenStyle(palette.RoadBorder, 5);
            var bx2 = writer.AllocatePenStyle(palette.RoadBorder, 9);
            var r3 = writer.AllocatePenStyle(palette.Road, 4);
            var b4 = writer.AllocatePenStyle(palette.RoadBorder, 5, new[] { 10.0, 6.0 });
            var r7 = writer.AllocatePenStyle(palette.Trail, 5);

            var gl1 = writer.AllocatePenStyle(palette.Graticule, 1);
            var gl2 = writer.AllocatePenStyle(palette.Graticule, 2);
            var gltT = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.TopCenter);
            var gltB = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.BottomCenter);
            var gltL = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterLeft);
            var gltR = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterRight);

            var city = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Italic, 32, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);
            var local = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.BoldItalic, 26, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);
            var name = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 22, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);

            return new TopoMapStyle()
            {
                rocks = rocks,
                forest = forest,
                buildings = buildings,
                water = water,
                contourStyle = contourStyle,
                roadForeground = new[] { r1, r2, r3, r3, r7 },
                roadBackground = new[] { b1, b2, b2, b4, null },
                bridgeForeground = new[] { r1, r2, r3, r3, r7 },
                bridgeBackground = new[] { bx1, bx2, bx2, bx2, null },
                gl1 = gl1,
                gl2 = gl2,
                gltT = externGraticule ? gltB : gltT,
                gltB = externGraticule ? gltT : gltB,
                gltL = externGraticule ? gltR : gltL,
                gltR = externGraticule ? gltL : gltR,
                city = city,
                local = local,
                name = name,
                plotted = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Italic, 18, new SolidColorBrush(Color.Black), null, false, TextAnchor.CenterLeft),
                plottedCircle = writer.AllocateBrushStyle(Color.Black),
                WindPowerPlant = IconsRender.WindTurbine(writer),
                WaterTower = IconsRender.WaterTower(writer),
                //TechnicalTower = IconsRender.TechnicalTower(writer),
                Transmitter = IconsRender.Transmitter(writer),
                Powerline = writer.AllocatePenStyle(Color.Black),
                Dot = IconsRender.Dot(writer),
                Hospital = IconsRender.Hospital(writer),
                BridgeLimit = writer.AllocatePenStyle(palette.RoadBorder, 3),
                Forts = writer.AllocateStyle(Color.White, Color.Black)
            };
        }


        public static TopoMapStyle CreateLod2(IDrawSurface writer, ColorPalette palette)
        {
            var rocks = writer.AllocateBrushStyle(palette.RocksFill);
            var forest = writer.AllocateBrushStyle(palette.ForestFill);
            var water = writer.AllocateBrushStyle(palette.WaterFill);

            var contourStyle = new ContourRenderStyle(writer);

            var r1 = writer.AllocatePenStyle(palette.MainRoad, 5);
            var b1 = writer.AllocatePenStyle(Color.Black, 6);
            var bx1 = writer.AllocatePenStyle(Color.Black, 10);
            var r2 = writer.AllocatePenStyle(palette.SecondaryRoad, 4);
            var b2 = writer.AllocatePenStyle(Color.Black, 5);
            var bx2 = writer.AllocatePenStyle(Color.Black, 9);
            var r3 = writer.AllocatePenStyle(Color.White, 4);

            var gl1 = writer.AllocatePenStyle(palette.Graticule, 1);
            var gl2 = writer.AllocatePenStyle(palette.Graticule, 2);
            var gltT = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 4, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.TopCenter);
            var gltB = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 4, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.BottomCenter);
            var gltL = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 4, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterLeft);
            var gltR = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 4, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterRight);

            var city = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Italic, 32 * 4, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);
            var local = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.BoldItalic, 26 * 4, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);


            return new TopoMapStyle()
            {
                rocks = rocks,
                forest = forest,
                buildings = null,
                water = water,
                contourStyle = contourStyle,
                roadForeground = new[] { r1, r2, r3, null, null },
                roadBackground = new[] { b1, b2, b2, null, null },
                bridgeForeground = new[] { r1, r2, r3, null, null },
                bridgeBackground = new[] { bx1, bx2, bx2, null, null },
                gl1 = gl1,
                gl2 = gl2,
                gltT = gltT,
                gltB = gltB,
                gltL = gltL,
                gltR = gltR,
                city = city,
                local = local,
                name = null,
                BridgeLimit = writer.AllocatePenStyle(palette.RoadBorder, 3)
            };
        }

        public static TopoMapStyle CreateLod3(IDrawSurface writer, ColorPalette palette)
        {
            var rocks = writer.AllocateBrushStyle(palette.RocksFill);
            var forest = writer.AllocateBrushStyle(palette.ForestFill);
            var water = writer.AllocateBrushStyle(palette.WaterFill);

            var contourStyle = new ContourRenderStyle(writer);

            var r1 = writer.AllocatePenStyle(palette.MainRoad, 5);
            var b1 = writer.AllocatePenStyle(Color.Black, 6);
            var bx1 = writer.AllocatePenStyle(Color.Black, 10);
            var r2 = writer.AllocatePenStyle(palette.SecondaryRoad, 4);
            var b2 = writer.AllocatePenStyle(Color.Black, 5);
            var bx2 = writer.AllocatePenStyle(Color.Black, 9);

            var gl1 = writer.AllocatePenStyle(palette.Graticule, 1);
            var gl2 = writer.AllocatePenStyle(palette.Graticule, 2);
            var gltT = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 16, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.TopCenter);
            var gltB = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 16, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.BottomCenter);
            var gltL = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 16, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterLeft);
            var gltR = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Bold, 28 * 16, new SolidColorBrush(palette.Graticule), new Pen(new SolidColorBrush(palette.TextBorder), 3), true, TextAnchor.CenterRight);

            var city = writer.AllocateTextStyle(new[] { "Calibri", "sans-serif" }, SixLabors.Fonts.FontStyle.Italic, 32 * 16, new SolidColorBrush(Color.Black), new Pen(new SolidColorBrush(palette.TextBorder), 3), true);

            return new TopoMapStyle()
            {
                rocks = rocks,
                forest = forest,
                buildings = null,
                water = water,
                contourStyle = contourStyle,
                roadForeground = new[] { r1, r2, null, null, null },
                roadBackground = new[] { b1, b2, null, null, null },
                bridgeForeground = new[] { r1, r2, null, null, null },
                bridgeBackground = new[] { bx1, bx2, null, null, null },
                gl1 = gl1,
                gl2 = gl2,
                gltT = gltT,
                gltB = gltB,
                gltL = gltL,
                gltR = gltR,
                city = city,
                local = null,
                name = null,
                BridgeLimit = writer.AllocatePenStyle(palette.RoadBorder, 3)
            };
        }
    }
}

// DemoApp – exercises all README examples to verify they compile and run.

using Pmad.Cartography;
using Pmad.Cartography.Contours;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;
using Pmad.Cartography.Drawing.Contours;
using Pmad.Cartography.Drawing.Topographic;
using Pmad.Cartography.Hillshading;
using Pmad.Cartography.Projections;
using Pmad.Drawing;
using Pmad.Drawing.PdfRender;
using Pmad.Geometry;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;

// ─────────────────────────────────────────────────────────────────────────────
// Pmad.Cartography README examples
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Pmad.Cartography ===");

// Query elevation from a well-known database
// Uses SRTM1 data hosted on cdn.dem.pmad.net (downloaded on demand and cached locally)
var database = WellKnownDatabases.GetSRTM1();

// Single point – bilinear interpolation
var elevation = await database.GetElevationAsync(
    new Coordinates(51.509865, -0.118092),
    DefaultInterpolation.Instance);
Console.WriteLine($"Elevation at London: {elevation:F1} m");

// Load an area into memory as a float raster
var area = await database.CreateView<float>(
    new Coordinates(51, -1),
    new Coordinates(52, 0));
Console.WriteLine($"Area loaded: {area.PointsLon}x{area.PointsLat} px");

// Contour lines
var contour = new ContourGraph();
// 10-metre interval starting at 10 m
contour.Add(area, new ContourLevelGenerator(10, 10));

foreach (var line in contour.Lines.Take(3))
{
    Console.WriteLine($"Level {line.Level}: {line.Points.Count} points");
}

// Hillshading – each pixel represents a 10x10-metre cell
var hillshader = new HillshaderFast(new Vector2D(10, 10));

// Returns an image where alpha encodes shadow intensity
var hillshadeImg = hillshader.GetPixelsAlphaBelowFlat(area);
Console.WriteLine($"Hillshade image: {hillshadeImg.Width}x{hillshadeImg.Height}");

// Custom cache path example (not actually downloaded here)
var _ = WellKnownDatabases.GetSRTM1(localCache: Path.Combine(Path.GetTempPath(), "dem-cache"));

// ─────────────────────────────────────────────────────────────────────────────
// Pmad.Drawing README examples
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("\n=== Pmad.Drawing ===");

var myPoints = new Vector2D[] { new(10, 10), new(200, 10), new(200, 200), new(10, 200) };
var myLine   = new Vector2D[] { new(50, 50), new(400, 300), new(700, 100) };

// Render to SVG
Render.ToSvg("output.svg", new Vector2D(800, 600), surface =>
{
    var fill   = new SolidColorBrush(Color.LightBlue);
    var stroke = new Pen(new SolidColorBrush(Color.DarkBlue), 2);
    var style  = surface.AllocateStyle(fill, stroke);

    surface.DrawPolygon(new[] { myPoints }, style);
    surface.DrawPolyline(myLine, style);
});
Console.WriteLine("SVG written: output.svg");

// Render to PNG
Render.ToPng("output.png", new Vector2D(800, 600), surface =>
{
    // same drawing calls as SVG
    var fill  = new SolidColorBrush(Color.LightBlue);
    var stroke = new Pen(new SolidColorBrush(Color.DarkBlue), 2);
    var style = surface.AllocateStyle(fill, stroke);
    surface.DrawPolygon(new[] { myPoints }, style);
    surface.DrawPolyline(myLine, style);
});
Console.WriteLine("PNG written: output.png");

// Render to PDF (sizeInPixels, not a PaperSize enum)
Render.ToPdf("output.pdf", new Vector2D(800, 600), surface =>
{
    // same drawing calls
    var fill  = new SolidColorBrush(Color.LightBlue);
    var stroke = new Pen(new SolidColorBrush(Color.DarkBlue), 2);
    var style = surface.AllocateStyle(fill, stroke);
    surface.DrawPolygon(new[] { myPoints }, style);
});
Console.WriteLine("PDF written: output.pdf");

// Generate map tiles (Leaflet-compatible)
TilingInfos info = Render.ToSvgTiled(
    "tiles/map.svg",
    new Vector2D(1024, 1024),
    SvgFallBackFormats.Webp,
    drawLod1: surface =>
    {
        var style = surface.AllocateStyle(new SolidColorBrush(Color.LightGreen), null);
        surface.DrawPolygon(new[] { myPoints }, style);
    },
    drawLod2: surface =>
    {
        var style = surface.AllocateStyle(new SolidColorBrush(Color.Green), null);
        surface.DrawPolygon(new[] { myPoints }, style);
    });
Console.WriteLine($"Tiles written – zoom {info.MinZoom}–{info.MaxZoom}");

// Tile an existing Image directly
using var fullImage = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(512, 512);
TilingInfos info2 = ImageTiler.DefaultToWebp(fullImage, "tiles-img/");
Console.WriteLine($"Image tiles written – zoom {info2.MinZoom}–{info2.MaxZoom}");

// ─────────────────────────────────────────────────────────────────────────────
// Pmad.Cartography.Drawing README examples
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("\n=== Pmad.Cartography.Drawing ===");

// Projection area covering the loaded DEM view
var proj = new NoProjectionArea(
    new Coordinates(51, -1),
    new Coordinates(52, 0),
    new Vector2D(1024, 1024));

// Render contour lines
Render.ToSvg("contours.svg", proj.Size, surface =>
{
    var renderer = new ContourRender(surface);
    renderer.Render(contour, proj, hillshadeImg);
});
Console.WriteLine("Contour SVG written: contours.svg");

// Topographic map rendering
using var scope = new NoProgress();

// Build a minimal ITopoMapData implementation using the DEM view we already loaded
var topoData = new DemoTopoMapData(area);

var renderData = TopoMapRenderData.Create(topoData, scope);

Render.ToSvgTiled("topo/map.svg", proj.Size, SvgFallBackFormats.Webp,
    drawLod1: surface => new TopoMapRender(renderData, proj).Render(surface),
    drawLod2: surface => new TopoMapRender(renderData, proj).RenderLod2(surface),
    drawLod3: surface => new TopoMapRender(renderData, proj).RenderLod3(surface));
Console.WriteLine("Topographic tiled SVG written: topo/map.svg");

// PDF export
// Note: TopoMapPdfRender is designed for metric units, but the demo data is in degrees, so rescale to have something useful
topoData = new DemoTopoMapData(new DemDataCellPixelIsPoint<float>(new Coordinates(0,0), new Coordinates(10240, 10240), area.ToDataCell().Data));

System.IO.Directory.CreateDirectory("topo-pdf");
var pdfFiles = TopoMapPdfRender.RenderPDF("topo-pdf", "demo", topoData, scope);
Console.WriteLine($"PDF files written: {pdfFiles.Count}");

// Map metadata example
var metadata = new TopoMapMetadata(
    attribution:   "Map data (c) OpenStreetMap contributors",
    title:         "My Topographic Map",
    licenseNotice: "CC BY-SA 4.0",
    exportCreator: "DemoApp 1.0",
    upperTitle:    "Sheet 1");
Console.WriteLine($"Metadata title: {metadata.Title}");

Console.WriteLine("\nAll examples completed successfully.");

// ─────────────────────────────────────────────────────────────────────────────
// Minimal ITopoMapData implementation used by the demo
// ─────────────────────────────────────────────────────────────────────────────

internal sealed class DemoTopoMapData : ITopoMapData
{
    public DemoTopoMapData(Pmad.Cartography.DataCells.IDemDataView dem)
    {
        DemDataCell = dem;
    }

    public TopoMapMetadata Metadata => TopoMapMetadata.None;
    public Pmad.Cartography.DataCells.IDemDataView DemDataCell { get; }
    public Dictionary<TopoMapPathType, Pmad.Geometry.Shapes.MultiPath<double, Vector2D>>? Roads => null;
    public Dictionary<TopoMapPathType, Pmad.Geometry.Shapes.MultiPath<double, Vector2D>>? Bridges => null;
    public Pmad.Geometry.Shapes.MultiPolygon<double, Vector2D>? ForestPolygons => null;
    public Pmad.Geometry.Shapes.MultiPolygon<double, Vector2D>? RockPolygons => null;
    public Pmad.Geometry.Shapes.MultiPolygon<double, Vector2D>? BuildingPolygons => null;
    public Pmad.Geometry.Shapes.MultiPolygon<double, Vector2D>? WaterPolygons => null;
    public Pmad.Geometry.Shapes.MultiPolygon<double, Vector2D>? FortPolygons => null;
    public List<TopoLocation>? Names => null;
    public List<TopoIcon>? Icons => null;
    public Pmad.Geometry.Shapes.MultiPath<double, Vector2D>? Powerlines => null;
    public Pmad.Geometry.Shapes.MultiPath<double, Vector2D>? Railways => null;
    public List<Pmad.Cartography.DemDataPoint>? PlottedPoints => null;
}

# Pmad.Cartography.Drawing

[![NuGet](https://img.shields.io/nuget/v/Pmad.Cartography.Drawing)](https://www.nuget.org/packages/Pmad.Cartography.Drawing)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE)

A .NET library for rendering topographic maps and contour overlays. It combines `Pmad.Cartography` (DEM/contour data) with `Pmad.Drawing` (vector output) to produce publication-quality maps in SVG, PNG/WebP and PDF.

## Installation

```
dotnet add package Pmad.Cartography.Drawing
```

## Getting started

### Render contour lines

```csharp
// Build contours from a DEM view
var contour = new ContourGraph();
contour.Add(area, new ContourLevelGenerator(10, 10));

// Render to any IDrawSurface
Render.ToSvg("contours.svg", new Vector2D(1024, 1024), surface =>
{
    var renderer = new ContourRender(surface);
    renderer.Render(contour, projectionArea, hillshadeImage);
});
```

### Render a full topographic map

```csharp
// Populate map data (roads, forests, water, buildings, etc.)
var data = new TopoMapRenderData
{
    Data = myTopoMapData,   // implements ITopoMapData
    Img  = hillshadeImage   // optional pre-computed hillshade
};

var proj = new NoProjectionArea(origin, new Vector2D(width, height), scale);

Render.ToSvgTiled("map.svg", proj.Size, SvgFallBackFormats.Webp,
    lod1: surface => new TopoMapRender(data, proj).Render(surface),
    lod2: surface => new TopoMapRender(data, proj).RenderLod2(surface),
    lod3: surface => new TopoMapRender(data, proj).RenderLod3(surface));
```

### Export as PDF

```csharp
var pdfRender = new TopoMapPdfRender(data, proj);
pdfRender.Render("map.pdf");
```

## Map data model

`ITopoMapData` / `TopoMapData` carries all vector layers that the renderer understands:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `DemDataCell` | `IDemDataView` | Elevation raster (required) |
| `Roads` | `Dictionary<TopoMapPathType, MultiPath>` | Road network by category |
| `Bridges` | `Dictionary<TopoMapPathType, MultiPath>` | Bridge segments |
| `Railways` | `MultiPath` | Rail lines |
| `Powerlines` | `MultiPath` | Power line routes |
| `ForestPolygons` | `MultiPolygon` | Forested areas |
| `RockPolygons` | `MultiPolygon` | Rock/cliff areas |
| `WaterPolygons` | `MultiPolygon` | Water bodies |
| `BuildingPolygons` | `MultiPolygon` | Building footprints |
| `FortPolygons` | `MultiPolygon` | Fortification areas |
| `Names` | `List<TopoLocation>` | Named locations with type and position |
| `Icons` | `List<TopoIcon>` | Point-of-interest icons |
| `PlottedPoints` | `List<DemDataPoint>` | Labelled elevation spot heights |

### Map metadata

```csharp
var metadata = new TopoMapMetadata(
    attribution:   "Map data (c) OpenStreetMap contributors",
    title:         "My Topographic Map",
    licenseNotice: "CC BY-SA 4.0",
    exportCreator: "MyApp 1.0",
    upperTitle:    "Sheet 1");
```

## Level-of-detail rendering

`TopoMapRender` exposes three LOD methods that progressively simplify the map for smaller zoom levels:

| Method | Detail level | Typical use |
| ------ | ------------ | ----------- |
| `Render` | Full detail | Maximum zoom |
| `RenderLod2` | Reduced | Medium zoom |
| `RenderLod3` | Minimal | Overview zoom |

## Key types

| Type | Description |
| ---- | ----------- |
| `ContourRender` | Draws a `ContourGraph` onto an `IDrawSurface`, with optional hillshade underlay |
| `ContourRenderStyle` | Default contour styling (thin lines every 10 m, bold every 50 m) |
| `TopoMapRender` | Full topographic map renderer |
| `TopoMapPdfRender` | PDF-specific renderer with page layout and legend |
| `TopoMapStyle` | Colour palette and style factory for all map layers |
| `ColorPalette` | Default colour values used by `TopoMapStyle` |
| `LegendRender` | Draws the map legend |
| `NoProjectionArea` | Simple pixel-to-coordinate projection for flat/local maps |

## Dependencies

- [Pmad.Cartography](https://www.nuget.org/packages/Pmad.Cartography)
- [Pmad.Drawing](https://www.nuget.org/packages/Pmad.Drawing)
- [Pmad.ProgressTracking](https://www.nuget.org/packages/Pmad.ProgressTracking)

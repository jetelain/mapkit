# Pmad.Drawing

[![NuGet](https://img.shields.io/nuget/v/Pmad.Drawing)](https://www.nuget.org/packages/Pmad.Drawing)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE)

A .NET vector drawing library that exposes a single `IDrawSurface` API and supports multiple output backends: **SVG**, **PNG/WebP** (via ImageSharp) and **PDF** (via PdfSharpCore). Tiled output for web maps (Leaflet-compatible) is also supported.

## Installation

```
dotnet add package Pmad.Drawing
```

## Getting started

### Render to SVG

```csharp
Render.ToSvg("output.svg", new Vector2D(800, 600), surface =>
{
    var fill   = new SolidColorBrush(Color.LightBlue);
    var stroke = new Pen(new SolidColorBrush(Color.DarkBlue), 2);
    var style  = surface.AllocateStyle(fill, stroke);

    surface.DrawPolygon(new[] { myPoints }, style);
    surface.DrawPolyline(myLine, style);
});
```

### Render to PNG

```csharp
Render.ToPng("output.png", new Vector2D(800, 600), surface =>
{
    // same drawing calls as SVG
});
```

### Render to PDF

```csharp
Render.ToPdf("output.pdf", PaperSize.A4Landscape, surface =>
{
    // same drawing calls
});
```

### Generate map tiles (Leaflet-compatible)

```csharp
TilingInfos info = Render.ToSvgTiled(
    "tiles/map.svg",
    new Vector2D(4096, 4096),
    SvgFallBackFormats.Webp,
    drawLod1: surface => { /* full-detail drawing */ },
    drawLod2: surface => { /* reduced-detail drawing */ });
```

You can also tile an existing `Image` directly:

```csharp
TilingInfos info = ImageTiler.DefaultToWebp(fullImage, "tiles/");
```

## Drawing API overview

All output backends implement `IDrawSurface`:

| Method | Description |
| ------ | ----------- |
| `AllocateStyle(fill, pen)` | Create a reusable fill/stroke style |
| `AllocateTextStyle(...)` | Create a reusable text style (font, size, colour, anchor) |
| `AllocateIcon(size, draw)` | Create a reusable icon defined by a drawing callback |
| `DrawPolygon(paths, style)` | Filled/stroked polygon (with holes via multiple paths) |
| `DrawPolyline(points, style)` | Open polyline |
| `DrawCircle(center, radius, style)` | Circle |
| `DrawArc(center, radius, start, sweep, style)` | Arc |
| `DrawTextPath(points, text, style)` | Text flowing along a path |
| `DrawText(point, text, style)` | Text at a fixed position |
| `DrawIcon(center, icon)` | Pre-allocated icon |
| `DrawImage(image, pos, size, alpha)` | Raster image |
| `DrawRoundedRectangle(tl, br, style, radius)` | Rounded rectangle |

### Brushes

| Type | Description |
| ---- | ----------- |
| `SolidColorBrush` | Uniform RGBA colour |
| `VectorBrush` | Pattern brush defined by a drawing callback |

### Memory surface and scaling

`MemorySurface` records all drawing operations so they can be replayed at a different scale or into a different backend – useful for generating multiple zoom levels from a single drawing pass.

## Output backends

| Backend | Class | Notes |
| ------- | ----- | ----- |
| SVG | `SvgSurface` | Inline CSS styles, external image references |
| PNG / WebP / JPEG | `ImageSurface` | Rasterised via ImageSharp |
| PDF | `PdfSurface` | Single-page PDF via PdfSharpCore |
| Memory | `MemorySurface` | Captures operations for later replay or scaling |

## Dependencies

- [Pmad.Geometry](https://www.nuget.org/packages/Pmad.Geometry)
- [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp)
- [SixLabors.ImageSharp.Drawing](https://www.nuget.org/packages/SixLabors.ImageSharp.Drawing)
- [PdfSharpCore](https://www.nuget.org/packages/PdfSharpCore)
- [Pmad.ProgressTracking](https://www.nuget.org/packages/Pmad.ProgressTracking)

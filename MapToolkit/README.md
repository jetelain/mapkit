# Pmad.Cartography

[![NuGet](https://img.shields.io/nuget/v/Pmad.Cartography)](https://www.nuget.org/packages/Pmad.Cartography)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE)

A .NET library for Digital Elevation Model (DEM) processing. It provides elevation queries, contour line generation, hillshading and support for multiple data formats and on-demand HTTP data sources.

## Installation

```
dotnet add package Pmad.Cartography
```

## Getting started

### Query elevation from a well-known database

```csharp
// Uses SRTM1 data hosted on cdn.dem.pmad.net (downloaded on demand and cached locally)
var database = WellKnownDatabases.GetSRTM1();

// Single point - bilinear interpolation
var elevation = await database.GetElevationAsync(
    new Coordinates(51.509865, -0.118092),
    DefaultInterpolation.Instance);

// Load an area into memory as a float raster
var area = await database.CreateView<float>(
    new Coordinates(51, -1),
    new Coordinates(52, 0));
```

### Elevation contours

```csharp
var contour = new ContourGraph();
// 10-metre interval starting at 10 m
contour.Add(area, new ContourLevelGenerator(10, 10));

foreach (var line in contour.Lines)
{
    Console.WriteLine($"Level {line.Level}: {line.Points.Count} points");
}
```

### Hillshading

```csharp
// Each pixel of 'area' represents a 10x10-metre cell
var hillshader = new HillshaderFast(new Vector2D(10, 10));

// Returns an RGBA image where alpha encodes shadow intensity
Image<Rgba32> img = hillshader.GetPixelsAlphaBelowFlat(area);
```

## Data sources

The following open datasets are pre-configured in `WellKnownDatabases`:

| Source   | Resolution     | License        | Credits |
| -------- | -------------- | -------------- | ------- |
| SRTM1    | 1 arc second   | Public Domain  | NASA |
| SRTM15+  | 15 arc seconds | Public Domain  | Tozer et al. |
| AW3D30   | 1 arc second   | [See terms](https://cdn.dem.pmad.net/README.txt) | (c) JAXA |

Data is fetched over HTTP from `cdn.dem.pmad.net` and cached locally. You can supply a custom cache path:

```csharp
var database = WellKnownDatabases.GetSRTM1(localCache: @"C:\dem-cache");
```

## File formats

### Supported raster formats

| Format     | Read | Write | Notes |
| ---------- | :--: | :---: | ----- |
| ESRI ASCII | Yes  | Yes   | `float` only |
| DDC        | Yes  | Yes   | Native format for this library |
| GeoTIFF    | Yes  |       | WGS84 projection only |
| SRTM HGT   | Yes  |       | 1 and 3 arc-second |

### Supported compression wrappers

| Format | Read | Write | Notes |
| ------ | :--: | :---: | ----- |
| ZSTD   | Yes  | Yes   | Best storage/CPU trade-off (default) |
| GZip   | Yes  | Yes   | Lowest CPU cost |
| Brotli | Yes  | Yes   | Best compression ratio |
| Zip    | Yes  |       | Archive must contain a single file |

## Key types

| Type | Description |
| ---- | ----------- |
| `DemDatabase` | Main entry point - wraps a storage and provides async elevation/view queries |
| `WellKnownDatabases` | Factory helpers for the pre-configured CDN datasets |
| `DemDataCell<T>` | In-memory raster tile with typed pixel values |
| `ContourGraph` | Builds a set of contour lines from a raster view |
| `ContourLevelGenerator` | Defines the interval and starting elevation for contour generation |
| `HillshaderFast` | Fast hillshading using the Zevenbergen-Thorne gradient algorithm |
| `HillshaderClassic` | Reference hillshader (slower, same visual output) |
| `HillshaderIgor` | Alternative hillshader with a different lighting model |
| `DefaultInterpolation` | Bilinear elevation interpolation between raster pixels |
| `Coordinates` | WGS84 latitude/longitude coordinate pair |
| `DemFileSystemStorage` | Reads a DEM database from a local directory |
| `DemHttpStorage` | Downloads DEM tiles on demand from an HTTP server with local caching |

## Dependencies

- [Pmad.Geometry](https://www.nuget.org/packages/Pmad.Geometry)
- [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp)
- [BitMiracle.LibTiff.NET](https://www.nuget.org/packages/BitMiracle.LibTiff.NET)
- [ZstdSharp.Port](https://www.nuget.org/packages/ZstdSharp.Port)
- [GeoJSON.Text](https://www.nuget.org/packages/GeoJSON.Text)

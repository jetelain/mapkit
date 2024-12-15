# Pmad.Cartography

## Pmad.Cartography
A simple and read-to-use Digital Elevation Model for everyone based on Open Data with minimalist credits

### Digital Elevation Model sources

| Source | Resolution    | License       | URL | Credits |
| ------ | ------------- | ------------- | --- | --- |
| SRTM1  | 1 arc second  | Public Domain | https://cdn.dem.pmad.net/SRTM1/ | NASA |
| SRTM15+| 15 arc second | Public Domain | https://cdn.dem.pmad.net/SRTM15Plus/ | Tozer, B. , D. T. Sandwell, W. H. F. Smith, C. Olson, J. R. Beale, and P. Wessel |
| AW3D30 | 1 arc second  | [See terms](https://cdn.dem.pmad.net/README.txt) | https://cdn.dem.pmad.net/AW3D30/ | � JAXA |

```csharp
var database = WellKnownDatabases.GetSRTM1();

// Singe point
var elevation = await database.GetElevationAsync(new Coordinates(51.509865, -0.118092), DefaultInterpolation.Instance);

// Area
var area = await demDatabase.CreateView<float>(new Coordinates(51, -1), new Coordinates(52, 0));
```

### Elevation contours

```csharp
var contour = new ContourGraph();
contour.Add(area, new ContourLevelGenerator(10, 10)); // 10 meters elevation interval from 10
```

### Hillshading

```csharp
var img = new HillshaderFast(new Vector2D(10, 10)) // Assume each pixel of area is 10x10 meters
			.GetPixelsAlphaBelowFlat(area);
```

### File formats

#### Supported data formats

| Format     | Read | Write | Remarks                             |
| ---------- | ---- | ----- | ----------------------------------- |
| ESRI ASCII | Yes  | Yes   | float only                          |
| DDC        | Yes  | Yes   | Format specific to Pmad.Cartography |
| GeoTIFF    | Yes  | No    | WSG84 projection Only               |
| SRTM       | Yes  | No    | 3 and 1 arc second                  |

#### Supported compression formats

Most DEM files requires a lot of disk space. To reduce the size of the files, the following compression formats are supported:

| Format     | Read | Write | Remarks                             |
| ---------- | ---- | ----- | ----------------------------------- |
| ZSTD       | Yes  | Yes   | Best compromise storage/CPU cost    |
| GZIP       | Yes  | Yes   | Lowest CPU cost                     |
| Brotli     | Yes  | Yes   | Best compression                    |
| Zip        | Yes  | No    | Zip must contains only one file     |

## Pmad.Cartography.Drawing

A simple topographic map rendering toolkit.

Drawing API is still in development, it may change in the future.

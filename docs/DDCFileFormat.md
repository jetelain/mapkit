# DemDataCell (DDC) File Format 1.0

This file format is intend to be as simple as possible to store DEM data :
  - Simple read/write in C#
  - Trivial read in JavaScript with a modern browser
  - Lightweight
  
GeoTIFF is too complex to read. A JSON format would be too massive.

This file format is not intend for interchange, it's an internal format.
  
Data is little endian
  
## Header
  
Header size is 56 bytes (0x38)
  
```
+---0---+---1---+---2---+---3---+---4---+---5---+---6---+---7---+
| Magic Number                  | Ver   | SVer  | DType | RType |
+---8---+---9---+---A---+---B---+---C---+---D---+---E---+---F---+
| Start Latitude / Y1                                           |
+--10---+-------+-------+-------+-------+-------+-------+-------+
| Start Longitude / X1                                          |
+--18---+-------+-------+-------+-------+-------+-------+-------+
| End Latitude / Y2                                             |
+--20---+-------+-------+-------+-------+-------+-------+-------+
| End Longitude / X2                                            |
+--28---+-------+-------+-------+--2C---+-------+-------+-------+
| Height                        | Width                         |
+--30---+-------+-------+-------+-------+-------+-------+-------+
| Unused                                                        |
+--38---+-------+-------+-------+-------+-------+-------+-------+
```

| Offset | Bytes | Description                                 | Value      |
|--------|-------|---------------------------------------------|------------|
|   0x00 |     4 | Magic Number (32 bits)                      | 0x57d15a3c |
|   0x04 |     1 | Major Version (8 bits)                      | 1          |
|   0x05 |     1 | Minor Version (8 bits)                      | 0          |
|   0x06 |     1 | DataType (8 bits)                           | 0=float, 1=int16, 2=uint16, 3=double     |
|   0x07 |     1 | RasterType (8 bits)                         | 0=Unknown, 1=PixelIsArea, 2=PixelIsPoint |
|   0x08 |     8 | Start Latitude, or Y1 (double, 64 bits)     |            |
|   0x10 |     8 | Start Longitude, or X1 (double, 64 bits)    |            |
|   0x18 |     8 | End Latitude, or Y2 (double, 64 bits)       |            |
|   0x20 |     8 | End Longitude, or X2 (double, 64 bits)      |            |
|   0x28 |     4 | Height (unsigned 32 bits)                   |            |
|   0x2C |     4 | Width (unsigned 32 bits)                    |            |
|   0x30 |     8 | Unused (8 bytes)                            | 0          |

- DataType
  - float : Single-precision floating-point (32 bits), 4 bytes per pixel
  - int16 : Signed short integer, 2 bytes per pixel
  - uint16 : Unsigned short integer, 2 bytes per pixel
  - double : Double-precision floating-point (64 bits), 8 bytes per pixel

## Data

| Offset | Bytes | Description                                 | Value      |
|--------|-------|---------------------------------------------|------------|
|   0x38 |     4 | Data size in bytes (unsigned 32 bits)       | `Height * Width * sizeof(DataType)` |
|   0x3C |  Size | Data line by line                           |            |

Mapping entries to coordinates depends on RasterType :
 - If PixelIsArea, then `dX=(X2-X1)/Width` and `dY=(Y2-Y1)/Height` 
 - If PixelIsPoint, then `dX=(X2-X1)/(Width-1)` and `dY=(Y2-Y1)/(Height-1)` 

Coordinates `(X,Y)` correspond to entry `((Y-Y1)/dY) * Width + ((X-X1)/dX)`

That means that data starts by line zero (with coordinates Y1) :
 - Entry #0 has coordinates `(X1,Y1)`
 - Entry #1 has coordinates `(X1+1*dX,Y1)`
 - Entry #2 has coordinates `(X1+2*dX,Y1)`
 - ...
 - Last entry has coordinates `(X2,Y2)`



# DemDataCell (DDC) File Format 1.0

This file format is intend to be as simple as possible to store DEM data :
  - Simple read/write in C#
  - Trivial read in JavaScript with a modern browser
  - Lightweight
  
GeoTIFF is too complex to read. A JSON format would be too massive.

This file format is not intend for interchange, it's an internal format.
  
Everything is little endian
  
## Header
  
  
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

- Magic Number (32 bits) : 0x57d15a3c
- Major Version (8 bits) : 1 (breaking change)
- Minor Version (8 bits) : 0 (no breaking change)
- DataType (8 bits) :
  - 0 : float (32 bits) - 4 bytes per pixel
  - 1 : int16 - 2 bytes per pixel
  - 2 : unsigned int16 - 2 bytes per pixel
  - 3 : double (64 bits) - 8 bytes per pixel
- Unused (8 bits) : 0
- RasterType (8 bits) :
  - 0 Unknown
  - 1 PixelIsArea
  - 2 PixelIsPoint
- Start Latitude, or Y1 (double, 64 bits)
- Start Longitude, or X1 (double, 64 bits)
- End Latitude, or Y2 (double, 64 bits)
- End Longitude, or X2 (double, 64 bits)
- Height (unsigned 32 bits)
- Width (unsigned 32 bits)
- Unused (8 bytes) (might be used for infos on coordinate system/EPSG codes)


## Data

- Data size in bytes (unsigned 32 bits) : Must be equal to Height x Width x bytes per pixel
- Data line by line

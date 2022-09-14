using System.Collections.Generic;
using System.IO;

namespace SimpleDEM.DataCells
{
    public interface IDemDataView : IDemDataCellMetadata
    {
        double PixelSizeLat { get; }

        double PixelSizeLon { get; }

        IEnumerable<DemDataPoint> GetPointsOnParallel(int lat, int lon, int count);

        IDemDataView CreateView(Coordinates start, Coordinates end);
    }
}
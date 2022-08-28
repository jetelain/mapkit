using System.Collections.Generic;
using System.IO;

namespace SimpleDEM.DataCells
{
    public interface IDemDataCell : IDemDataCellMetadata
    {
        void Save(Stream target);

        double GetRawElevation(Coordinates coordinates);

        double GetLocalElevation(Coordinates coordinates, IInterpolation interpolation);

        IEnumerable<DemDataPoint> GetNearbyElevation(Coordinates coordinates);

        /// <summary>
        /// Determines if cell has all data to compute elevation for specified coordinates.
        /// 
        /// It means that cell contains 4 data points for requested coordinates. No other cell data is required for computation.
        /// 
        /// If true, you can use safely <see cref="GetLocalElevation"/>
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        bool IsLocal(Coordinates coordinates);

        int SizeInBytes { get; }

        IEnumerable<DemDataPoint> GetScanLine(int lat);

        IDemDataCell Crop(Coordinates subStart, Coordinates subEnd);
    }
}
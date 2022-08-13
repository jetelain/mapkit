using System.Collections.Generic;
using System.IO;

namespace SimpleDEM.DataCells
{
    public interface IDemDataCell : IDemDataCellMetadata
    {
        void Save(Stream target);

        double GetRawElevation(GeodeticCoordinates coordinates);

        double GetLocalElevation(GeodeticCoordinates coordinates, IInterpolation interpolation);

        IEnumerable<DemDataPoint> GetNearbyElevation(GeodeticCoordinates coordinates);

        /// <summary>
        /// Determines if cell has all data to compute elevation for specified coordinates.
        /// 
        /// It means that cell contains 4 data points for requested coordinates. No other cell data is required for computation.
        /// 
        /// If true, you can use safely <see cref="GetLocalElevation"/>
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        bool IsLocal(GeodeticCoordinates coordinates);

        int SizeInBytes { get; }
    }
}
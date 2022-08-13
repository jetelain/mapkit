namespace SimpleDEM.DataCells
{
    public interface IDemDataCellMetadata
    {
        GeodeticCoordinates Start { get; }

        GeodeticCoordinates End { get; }

        DemRasterType RasterType { get; }

        int PointsPerCellLat { get; }

        int PointsPerCellLon { get; }
    }
}
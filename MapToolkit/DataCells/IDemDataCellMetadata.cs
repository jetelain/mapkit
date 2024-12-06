namespace Pmad.Cartography.DataCells
{
    public interface IDemDataCellMetadata
    {
        Coordinates Start { get; }

        Coordinates End { get; }

        DemRasterType RasterType { get; }

        int PointsLat { get; }

        int PointsLon { get; }
    }
}
namespace Pmad.Cartography.DataCells
{
    /// <summary>
    /// Represents the metadata of a DEM data cell.
    /// </summary>
    public interface IDemDataCellMetadata
    {
        /// <summary>
        /// The start and end coordinates of the data cell.
        /// </summary>
        Coordinates Start { get; }

        /// <summary>
        /// The start and end coordinates of the data cell.
        /// </summary>
        Coordinates End { get; }

        /// <summary>
        /// The raster type of the data cell.
        /// </summary>
        DemRasterType RasterType { get; }

        /// <summary>
        /// The number of points in the latitude direction (Y).
        /// </summary>
        int PointsLat { get; }

        /// <summary>
        /// The number of points in the longitude direction (X).
        /// </summary>
        int PointsLon { get; }
    }
}
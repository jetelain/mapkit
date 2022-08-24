using System.IO;
using System.Text.Json.Serialization;

namespace SimpleDEM.DataCells
{
    public class DemDataCellMetadata : IDemDataCellMetadata
    {
        internal DemDataCellMetadata(BinaryReader reader)
        {
            RasterType = (DemRasterType)reader.ReadByte();
            Start = new GeodeticCoordinates(reader.ReadDouble(), reader.ReadDouble());
            End = new GeodeticCoordinates(reader.ReadDouble(), reader.ReadDouble());
            PointsPerCellLat = reader.ReadInt32();
            PointsPerCellLon = reader.ReadInt32();
        }

        public DemDataCellMetadata(IDemDataCellMetadata other)
        {
            RasterType = other.RasterType;
            Start = other.Start;
            End = other.End;
            PointsPerCellLat = other.PointsPerCellLat;
            PointsPerCellLon = other.PointsPerCellLon;
        }

        [JsonConstructor]
        public DemDataCellMetadata(DemRasterType rasterType, GeodeticCoordinates start, GeodeticCoordinates end, int pointsPerCellLat, int pointsPerCellLon)
        {
            RasterType = rasterType;
            Start = start;
            End = end;
            PointsPerCellLat = pointsPerCellLat;
            PointsPerCellLon = pointsPerCellLon;
        }

        public DemRasterType RasterType { get; }

        public GeodeticCoordinates Start { get; }

        public GeodeticCoordinates End { get; }

        public int PointsPerCellLat { get; }

        public int PointsPerCellLon { get; }
    }
}

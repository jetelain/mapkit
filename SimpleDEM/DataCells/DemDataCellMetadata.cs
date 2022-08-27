using System.IO;
using System.Text.Json.Serialization;

namespace SimpleDEM.DataCells
{
    public class DemDataCellMetadata : IDemDataCellMetadata
    {
        internal DemDataCellMetadata(BinaryReader reader)
        {
            RasterType = (DemRasterType)reader.ReadByte();
            Start = new Coordinates(reader.ReadDouble(), reader.ReadDouble());
            End = new Coordinates(reader.ReadDouble(), reader.ReadDouble());
            PointsLat = reader.ReadInt32();
            PointsLon = reader.ReadInt32();
        }

        public DemDataCellMetadata(IDemDataCellMetadata other)
        {
            RasterType = other.RasterType;
            Start = other.Start;
            End = other.End;
            PointsLat = other.PointsLat;
            PointsLon = other.PointsLon;
        }

        [JsonConstructor]
        public DemDataCellMetadata(DemRasterType rasterType, Coordinates start, Coordinates end, int pointsPerCellLat, int pointsPerCellLon)
        {
            RasterType = rasterType;
            Start = start;
            End = end;
            PointsLat = pointsPerCellLat;
            PointsLon = pointsPerCellLon;
        }

        public DemRasterType RasterType { get; }

        public Coordinates Start { get; }

        public Coordinates End { get; }

        public int PointsLat { get; }

        public int PointsLon { get; }
    }
}

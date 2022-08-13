using System.IO;

namespace SimpleDEM.DataCells
{
    public class DemDataCellMetadata : IDemDataCellMetadata
    {
        internal DemDataCellMetadata(BinaryReader reader)
        {
            RasterType = (DemRasterType)reader.ReadByte();
            reader.ReadByte(); // unused
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

        public DemRasterType RasterType { get; }

        public GeodeticCoordinates Start { get; }

        public GeodeticCoordinates End { get; }

        public int PointsPerCellLat { get; }

        public int PointsPerCellLon { get; }
    }
}

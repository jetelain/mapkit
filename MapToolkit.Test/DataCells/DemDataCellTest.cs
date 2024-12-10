using System;
using System.IO;
using Xunit;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.DataCells
{
    public class DemDataCellTest
    {
        [Fact]
        public void IsDemDataCellFile_ValidExtensions_ReturnsTrue()
        {
            Assert.True(DemDataCell.IsDemDataCellFile("file.ddc"));
            Assert.True(DemDataCell.IsDemDataCellFile("file.hgt"));
            Assert.True(DemDataCell.IsDemDataCellFile("file.tif"));
            Assert.True(DemDataCell.IsDemDataCellFile("file.asc"));
        }

        [Fact]
        public void IsDemDataCellFile_InvalidExtension_ReturnsFalse()
        {
            Assert.False(DemDataCell.IsDemDataCellFile("file.txt"));
            Assert.False(DemDataCell.IsDemDataCellFile("file.jpg"));
            Assert.False(DemDataCell.IsDemDataCellFile("file.png"));
        }

        [Fact]
        public void Load_UnsupportedExtension_ThrowsIOException()
        {
            var ex = Assert.Throws<IOException>(() => DemDataCell.Load("file.txt"));
            Assert.Equal("Extension '.txt' is not supported.", ex.Message);
        }

        [Fact]
        public void LoadMetadata_UnsupportedExtension_ThrowsIOException()
        {
            var ex = Assert.Throws<IOException>(() => DemDataCell.LoadMetadata("file.txt"));
            Assert.Equal("Extension '.txt' is not supported.", ex.Message);
        }

        [Fact]
        public void LoadMetadata_ValidStream_ReturnsDemDataCell()
        {
            using var stream = CreateDemDataCell();

            var result = DemDataCell.LoadMetadata(stream);
            Assert.Equal(DemRasterType.PixelIsPoint, result.RasterType);
            Assert.Equal(0, result.Start.Latitude);
            Assert.Equal(1, result.Start.Longitude);
            Assert.Equal(2, result.End.Latitude);
            Assert.Equal(3, result.End.Longitude);
            Assert.Equal(100, result.PointsLat);
            Assert.Equal(200, result.PointsLon);
        }

        [Fact]
        public void GetDataTypeCode_ValidTypes_ReturnsCorrectCode()
        {
            Assert.Equal(0, DemDataCell.GetDataTypeCode(typeof(float)));
            Assert.Equal(1, DemDataCell.GetDataTypeCode(typeof(short)));
            Assert.Equal(2, DemDataCell.GetDataTypeCode(typeof(ushort)));
            Assert.Equal(3, DemDataCell.GetDataTypeCode(typeof(double)));
            Assert.Equal(4, DemDataCell.GetDataTypeCode(typeof(int)));
        }

        [Fact]
        public void GetDataTypeCode_InvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => DemDataCell.GetDataTypeCode(typeof(string)));
        }

        [Fact]
        public void Load_ValidStream_ReturnsDemDataCell()
        {
            using var stream = CreateDemDataCell();

            var result = DemDataCell.Load(stream);

            Assert.Equal(DemRasterType.PixelIsPoint, result.RasterType);
            Assert.Equal(0, result.Start.Latitude);
            Assert.Equal(1, result.Start.Longitude);
            Assert.Equal(2, result.End.Latitude);
            Assert.Equal(3, result.End.Longitude);
            Assert.Equal(100, result.PointsLat);
            Assert.Equal(200, result.PointsLon);
        }

        private static MemoryStream CreateDemDataCell()
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                writer.Write(DemDataCell.MagicNumber);
                writer.Write((byte)1); // version
                writer.Write((byte)0); // subversion
                writer.Write((byte)0); // dataType => float
                writer.Write((byte)2); // DemRasterType
                writer.Write((double)0); //Start.Latitude
                writer.Write((double)1); //Start.Longitude
                writer.Write((double)2); //End.Latitude
                writer.Write((double)3); //End.Longitude
                writer.Write((int)100); // PointsLat
                writer.Write((int)200); // PointsLon
                writer.Write((int)0); // Unused
                writer.Write((int)0); // Unused
                writer.Write((uint)0); // dataSize
            }
            stream.Position = 0;
            return stream;
        }
    }
}

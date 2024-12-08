using System;
using System.IO;
using System.Text;
using Xunit;
using Pmad.Cartography.DataCells.FileFormats;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.DataCells.FileFormats
{
    public class EsriAsciiHelperTest
    {
        [Fact]
        public void LoadDataCellMetadata_ValidFile_ReturnsMetadata()
        {
            var content = "ncols 4\nnrows 3\nxllcorner 0.0\nyllcorner 0.0\ncellsize 1.0\nNODATA_value -9999\n";
            using var reader = new StringReader(content);
            var metadata = EsriAsciiHelper.LoadDataCellMetadata(reader, out var nodata);

            Assert.Equal(new Coordinates(0, 0), metadata.Start);
            Assert.Equal(new Coordinates(2, 3), metadata.End);
            Assert.Equal(DemRasterType.PixelIsPoint, metadata.RasterType);
            Assert.Equal(4, metadata.PointsLon);
            Assert.Equal(3, metadata.PointsLat);
            Assert.Equal(-9999, nodata);
            Assert.Equal(0.0, metadata.Start.Latitude);
            Assert.Equal(0.0, metadata.Start.Longitude);
        }

        [Fact]
        public void LoadDataCellMetadata_ValidFile_PixelIsArea_ReturnsMetadata()
        {
            var content = "ncols 4\nnrows 3\nxllcenter 0.0\nyllcenter 0.0\ncellsize 1.0\nNODATA_value -9999\n";
            using var reader = new StringReader(content);
            var metadata = EsriAsciiHelper.LoadDataCellMetadata(reader, out var nodata);

            Assert.Equal(new Coordinates(0, 0), metadata.Start);
            Assert.Equal(new Coordinates(3, 4), metadata.End);
            Assert.Equal(DemRasterType.PixelIsArea, metadata.RasterType);
            Assert.Equal(4, metadata.PointsLon);
            Assert.Equal(3, metadata.PointsLat);
            Assert.Equal(-9999, nodata);
            Assert.Equal(0.0, metadata.Start.Latitude);
            Assert.Equal(0.0, metadata.Start.Longitude);
        }

        [Fact]
        public void LoadDataCell_ValidFile_ReturnsDataCell()
        {
            var content = "ncols 2\nnrows 2\nxllcorner 0.0\nyllcorner 0.0\ncellsize 1.0\nNODATA_value -9999\n1 2\n3 4\n";
            using var reader = new StringReader(content);
            var dataCell = EsriAsciiHelper.LoadDataCell(reader);

            Assert.Equal(new Coordinates(0, 0), dataCell.Start);
            Assert.Equal(new Coordinates(1, 1), dataCell.End);
            Assert.Equal(DemRasterType.PixelIsPoint, dataCell.RasterType);
            Assert.Equal(2, dataCell.PointsLon);
            Assert.Equal(2, dataCell.PointsLat);
            Assert.Equal(1.0f, dataCell.Data[1, 0]);
            Assert.Equal(2.0f, dataCell.Data[1, 1]);
            Assert.Equal(3.0f, dataCell.Data[0, 0]);
            Assert.Equal(4.0f, dataCell.Data[0, 1]);
        }

        [Fact]
        public void SaveDataCell_PixelIsPoint()
        {
            var dataCell = new DemDataCellPixelIsPoint<float>(new Coordinates(0, 0), new Coordinates(1, 1), new float[,] { { 1, 2 }, { 3, 4 } });

            using var writer = new StringWriter();
            EsriAsciiHelper.SaveDataCell(writer, dataCell, "-9999");

            var expectedContent = "ncols         2\nnrows         2\nxllcorner     0\n" +
                                  "yllcorner     0\ncellsize      1\nNODATA_value  -9999\n" +
                                  "3.00 4.00\n1.00 2.00\n";
            Assert.Equal(expectedContent.ReplaceLineEndings(), writer.ToString());
        }

        [Fact]
        public void SaveDataCell_NoData()
        {
            var dataCell = new DemDataCellPixelIsPoint<float>(new Coordinates(0, 0), new Coordinates(1, 1), new float[,] { { 1, 2 }, { 3, float.NaN } });

            using var writer = new StringWriter();
            EsriAsciiHelper.SaveDataCell(writer, dataCell, "-9999");

            var expectedContent = "ncols         2\nnrows         2\nxllcorner     0\n" +
                                  "yllcorner     0\ncellsize      1\nNODATA_value  -9999\n" +
                                  "3.00 -9999\n1.00 2.00\n";
            Assert.Equal(expectedContent.ReplaceLineEndings(), writer.ToString());
        }

        [Fact]
        public void SaveDataCell_PixelIsArea()
        {
            var dataCell = new DemDataCellPixelIsArea<float>(new Coordinates(0, 0), new Coordinates(1, 1), new float[,] { { 1, 2 }, { 3, 4 } });

            using var writer = new StringWriter();
            EsriAsciiHelper.SaveDataCell(writer, dataCell, "-9999");

            var expectedContent = "ncols         2\nnrows         2\nxllcenter     0\n" +
                                  "yllcenter     0\ncellsize      0.5\nNODATA_value  -9999\n" +
                                  "3.00 4.00\n1.00 2.00\n";
            Assert.Equal(expectedContent.ReplaceLineEndings(), writer.ToString());
        }

        [Fact]
        public void LoadDataCellMetadata_MissingNcols_ThrowsIOException()
        {
            var content = "nrows 3\nxllcorner 0.0\nyllcorner 0.0\ncellsize 1.0\nNODATA_value -9999";
            using var reader = new StringReader(content);

            Assert.Throws<IOException>(() => EsriAsciiHelper.LoadDataCellMetadata(reader, out _));
        }

        [Fact]
        public void LoadDataCellMetadata_MissingNrows_ThrowsIOException()
        {
            var content = "ncols 4\nxllcorner 0.0\nyllcorner 0.0\ncellsize 1.0\nNODATA_value -9999";
            using var reader = new StringReader(content);

            Assert.Throws<IOException>(() => EsriAsciiHelper.LoadDataCellMetadata(reader, out _));
        }

        [Fact]
        public void LoadDataCellMetadata_MissingCellsize_ThrowsIOException()
        {
            var content = "ncols 4\nnrows 3\nxllcorner 0.0\nyllcorner 0.0\nNODATA_value -9999";
            using var reader = new StringReader(content);

            Assert.Throws<IOException>(() => EsriAsciiHelper.LoadDataCellMetadata(reader, out _));
        }

        [Fact]
        public void LoadDataCellMetadata_MissingNodataValue_ThrowsIOException()
        {
            var content = "ncols 4\nnrows 3\nxllcorner 0.0\nyllcorner 0.0\ncellsize 1.0";
            using var reader = new StringReader(content);

            Assert.Throws<IOException>(() => EsriAsciiHelper.LoadDataCellMetadata(reader, out _));
        }
    }
}

using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.Databases
{
    public class DemDatabaseEntryTest
    {
        [Fact]
        public void DemDatabaseEntry_Contains()
        {
            var metadata = new Mock<IDemDataCellMetadata>();
            metadata.Setup(m => m.Start).Returns(new Coordinates(1, 1));
            metadata.Setup(m => m.End).Returns(new Coordinates(2, 2));
            var entry = new DemDatabaseEntry("test", metadata.Object);

            Assert.True(entry.Contains(new Coordinates(1.5, 1.5)));
            Assert.False(entry.Contains(new Coordinates(0.5, 0.5)));
        }

        [Fact]
        public void DemDatabaseEntry_Overlaps()
        {
            var metadata = new Mock<IDemDataCellMetadata>();
            metadata.Setup(m => m.Start).Returns(new Coordinates(1, 1));
            metadata.Setup(m => m.End).Returns(new Coordinates(2, 2));
            var entry = new DemDatabaseEntry("test", metadata.Object);

            Assert.True(entry.Overlaps(new Coordinates(0.5, 0.5), new Coordinates(1.5, 1.5)));
            Assert.False(entry.Overlaps(new Coordinates(2.5, 2.5), new Coordinates(3.5, 3.5)));
        }

        [Fact]
        public async Task DemDatabaseEntry_Load()
        {
            var metadata = new Mock<IDemDataCellMetadata>();
            var storage = new Mock<IDemStorage>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var dataCell = new Mock<IDemDataCell>();
            dataCell.Setup(d => d.SizeInBytes).Returns(100);
            storage.Setup(s => s.Load(It.IsAny<string>())).ReturnsAsync(dataCell.Object);

            var entry = new DemDatabaseEntry("test", metadata.Object);
            var result = await entry.Load(storage.Object, cache);

            Assert.NotNull(result);
            Assert.Equal(dataCell.Object, result);
        }

        [Fact]
        public void DemDatabaseEntry_PickData()
        {
            var metadata = new Mock<IDemDataCellMetadata>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var dataCell = new Mock<IDemDataCell>();

            var entry = new DemDatabaseEntry("test", metadata.Object);
            cache.Set(entry, dataCell.Object);

            var result = entry.PickData(cache);

            Assert.NotNull(result);
            Assert.Equal(dataCell.Object, result);
        }

        [Fact]
        public void DemDatabaseEntry_UnLoad()
        {
            var metadata = new Mock<IDemDataCellMetadata>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var dataCell = new Mock<IDemDataCell>();

            var entry = new DemDatabaseEntry("test", metadata.Object);
            cache.Set(entry, dataCell.Object);

            entry.UnLoad(cache);

            var result = entry.PickData(cache);

            Assert.Null(result);
        }

        [Fact]
        public void DemDatabaseEntry_GetCoverageSurface()
        {
            var entry = new DemDatabaseEntry("test", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(1, 1), new Coordinates(2, 2), 100, 100));
            Assert.Equal(0, entry.GetCoverageSurface(new Coordinates(0, 0), new Coordinates(1, 1)));
            Assert.Equal(1, entry.GetCoverageSurface(new Coordinates(1, 1), new Coordinates(2, 2)));
            Assert.Equal(0, entry.GetCoverageSurface(new Coordinates(2, 2), new Coordinates(3, 3)));
            Assert.Equal(0.5, entry.GetCoverageSurface(new Coordinates(1.5, 1), new Coordinates(2.5, 2)));
            Assert.Equal(0.25, entry.GetCoverageSurface(new Coordinates(1.5, 1.5), new Coordinates(2.5, 2.5)));
        }
    }
}

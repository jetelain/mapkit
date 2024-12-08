using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.Databases
{
    public class DemDatabaseTest
    {

        [Fact]
        public async Task LoadIndexAsync_ShouldLoadIndex()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var index = new DemDatabaseIndex(new List<DemDatabaseFileInfos>());
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(index);

            // Act
            await demDatabase.LoadIndexAsync();

            // Assert
            mockStorage.Verify(s => s.ReadIndex(), Times.Once);
        }

        [Fact]
        public async Task GetDataCellsAsync_ShouldReturnDataCells()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(1, 1);
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            var dataCell = new Mock<IDemDataCell>().Object;
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));
            mockStorage.Setup(s => s.Load(It.IsAny<string>())).ReturnsAsync(dataCell);

            // Act
            var result = await demDatabase.GetDataCellsAsync(start, end);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task CreateView_ShouldReturnDemDataView()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(1, 1);
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            var dataCell = new DemDataCellPixelIsPoint<float>(new Coordinates(0, 0), new Coordinates(1, 1), new float[100, 100]);
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));
            mockStorage.Setup(s => s.Load(It.IsAny<string>())).ReturnsAsync(dataCell);

            // Act
            var result = await demDatabase.CreateView<float>(start, end);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task HasData_ShouldReturnTrueIfDataExists()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(1, 1);
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));

            // Act
            var result = await demDatabase.HasData(start, end);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasFullData_ShouldReturnTrueIfFullDataExists()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(1, 1);
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));

            // Act
            var result = await demDatabase.HasFullData(start, end);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetElevation_ShouldReturnElevation()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var coordinates = new Coordinates(0, 0);
            var interpolation = new Mock<IInterpolation>().Object;
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            var dataCell = new Mock<IDemDataCell>().Object;
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));
            mockStorage.Setup(s => s.Load(It.IsAny<string>())).ReturnsAsync(dataCell);

            // Act
            var result = demDatabase.GetElevation(coordinates, interpolation);

            // Assert
            Assert.NotEqual(double.NaN, result);
        }

        [Fact]
        public async Task GetElevationAsync_ShouldReturnElevation()
        {
            // Arrange
            var mockStorage = new Mock<IDemStorage>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 });
            var demDatabase = new DemDatabase(mockStorage.Object, memoryCache);
            var coordinates = new Coordinates(0, 0);
            var interpolation = new Mock<IInterpolation>().Object;
            var entry = new DemDatabaseFileInfos("path", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(0, 0), new Coordinates(1, 1), 100, 100));
            var dataCell = new DemDataCellPixelIsPoint<float>(new Coordinates(0, 0), new Coordinates(1, 1), new float[100, 100]);
            mockStorage.Setup(s => s.ReadIndex()).ReturnsAsync(new DemDatabaseIndex(new List<DemDatabaseFileInfos> { entry }));
            mockStorage.Setup(s => s.Load(It.IsAny<string>())).ReturnsAsync(dataCell);

            // Act
            var result = await demDatabase.GetElevationAsync(coordinates, interpolation);

            // Assert
            Assert.NotEqual(double.NaN, result);
        }
    }
}

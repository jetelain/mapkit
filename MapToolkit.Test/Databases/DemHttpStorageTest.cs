using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;
using System.Linq;

namespace Pmad.Cartography.Test.Databases
{
    public class DemHttpStorageTest
    {
        private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0";
        private const string baseAddress = "https://cdn.dem.pmad.net/SRTM1/";
        private const string samplePath = "N00E006.SRTMGL1.hgt.zst";

        [Fact]
        public async Task Load_ShouldDownloadAndCacheFile()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var cacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", samplePath);
            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            // Act
            var dataCell = await storage.Load(samplePath);

            // Assert
            Assert.NotNull(dataCell);
            Assert.True(File.Exists(cacheFile));
        }

        [Fact]
        public async Task ReadIndex_ShouldDownloadAndDeserializeIndex()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent); 
            var storage = new DemHttpStorage(localCache, httpClient);

            // Act
            var index = await storage.ReadIndex();

            // Assert
            Assert.NotNull(index);
            Assert.NotEmpty(index.Cells); 

            var cell = index.Cells.FirstOrDefault(c => c.Path == "N00E006.SRTMGL1.hgt.zst");
            Assert.NotNull(cell);
            Assert.Equal(new(0,6),cell.Metadata.Start);
            Assert.Equal(new(1,7),cell.Metadata.End);
        }
    }
}

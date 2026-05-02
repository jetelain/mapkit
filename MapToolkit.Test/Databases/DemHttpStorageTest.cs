using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;
using System.Linq;

namespace Pmad.Cartography.Test.Databases
{
    public class DemHttpStorageTest
    {
        private const string userAgent = "Mozilla/5.0 (Pmad-Cartography; UnitTests)";
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
        public async Task Load_ShouldUseCachedFile_OnSecondCall()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var cacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", samplePath);

            // Ensure file is already cached
            await storage.Load(samplePath);
            var firstWriteTime = File.GetLastWriteTimeUtc(cacheFile);

            // Act
            await storage.Load(samplePath);
            var secondWriteTime = File.GetLastWriteTimeUtc(cacheFile);

            // Assert: cached file was not re-written
            Assert.Equal(firstWriteTime, secondWriteTime);
        }

        [Fact]
        public async Task Load_ShouldDeleteAndRedownload_WhenChecksumMismatch()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_checksum_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var cacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", samplePath);

            // Pre-create a corrupted cache file
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
            File.WriteAllBytes(cacheFile, new byte[] { 0x00, 0x01 });

            // Act: loading with a wrong checksum should re-download, fail validation, delete the file and throw
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                storage.LoadAsync(samplePath, "0000000000000000000000000000000000000000000000000000000000000000"));

            // The file should have been deleted after the checksum mismatch
            Assert.False(File.Exists(cacheFile));
        }

        [Fact]
        public async Task LoadAsync_ShouldNotThrow_WhenChecksumMatches()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_valid_checksum_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var cacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", samplePath);
            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            // Download once without checksum to get the real file
            await storage.Load(samplePath);

            // Compute the real checksum
            var realHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(cacheFile))).ToLowerInvariant();

            // Act: second load with correct checksum should succeed
            var dataCell = await storage.LoadAsync(samplePath, realHash);

            // Assert
            Assert.NotNull(dataCell);
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

        [Fact]
        public async Task ReadIndex_ShouldUseCachedIndex_WhenFresh()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_index_cache");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var indexCacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", "index.json");

            // Ensure a fresh cached index exists
            await storage.ReadIndex();
            var firstWriteTime = File.GetLastWriteTimeUtc(indexCacheFile);

            // Act: second call should use the cache
            await storage.ReadIndex();
            var secondWriteTime = File.GetLastWriteTimeUtc(indexCacheFile);

            // Assert: the cache file was not re-written
            Assert.Equal(firstWriteTime, secondWriteTime);
        }

        [Fact]
        public async Task ReadIndex_ShouldRefreshCache_WhenExpired()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_index_expired");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var indexCacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", "index.json");

            // Create a stale cache file
            Directory.CreateDirectory(Path.GetDirectoryName(indexCacheFile)!);
            File.WriteAllText(indexCacheFile, "{\"cells\":[]}");
            File.SetLastWriteTimeUtc(indexCacheFile, DateTime.UtcNow - DemHttpStorage.IndexCacheDuration - TimeSpan.FromSeconds(1));

            // Act: should re-download because the cache is expired
            var index = await storage.ReadIndex();
            var writeTime = File.GetLastWriteTimeUtc(indexCacheFile);

            // Assert: file was refreshed
            Assert.True(writeTime > DateTime.UtcNow - TimeSpan.FromMinutes(1));
            Assert.NotEmpty(index.Cells);
        }

        [Fact]
        public async Task GetSha256Async_ShouldDownloadFileAndReturnHash()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_sha256_download");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemHttpStorage(localCache, httpClient);
            var cacheFile = Path.Combine(localCache, "cdn.dem.pmad.net", "SRTM1", samplePath);
            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            // Act
            var hash = await storage.GetSha256Async(samplePath);

            // Assert
            Assert.NotNull(hash);
            Assert.Equal(64, hash.Length); // SHA-256 hex is 64 chars
            using var cacheFileStream = File.OpenRead(cacheFile);
            var expectedHash = Convert.ToHexString(SHA256.HashData(cacheFileStream)).ToLowerInvariant();
            Assert.Equal(expectedHash, hash);
        }

        [Fact]
        public async Task GetSha256Async_ShouldReturnNull_WhenFileNotFound()
        {
            // Arrange
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_sha256_missing");
            var handler = new NotFoundHttpMessageHandler();
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
            var storage = new DemHttpStorage(localCache, httpClient);

            // Act
            var hash = await storage.GetSha256Async("nonexistent_file.hgt.zst");

            // Assert
            Assert.Null(hash);
        }

        private sealed class NotFoundHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            }
        }
    }
}

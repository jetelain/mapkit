using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    public class DemHttpStorage : IDemStorage
    {
        /// <summary>
        /// How long a cached index.json is considered fresh before being re-downloaded.
        /// </summary>
        public static TimeSpan IndexCacheDuration { get; set; } = TimeSpan.FromHours(24);

        const int MaxDownloadAttempts = 3;

        private readonly string localCache;
        private readonly HttpClient client;

        public DemHttpStorage(string? localCache, HttpClient client)
        {
            this.localCache = localCache ?? DefaultCacheLocation;
            this.client = client;
        }

        public DemHttpStorage(string? localCache, Uri baseAddress)
            : this(localCache, new HttpClient() { BaseAddress = baseAddress })
        {

        }

        public DemHttpStorage(Uri baseAddress)
            : this(null, baseAddress)
        {

        }

        public static string DefaultCacheLocation => Path.Combine(Path.GetTempPath(), "dem");

        public static void ClearDefaultCache()
        {
            var cacheDir = DefaultCacheLocation;
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, recursive: true);
            }
        }

        private string GetCacheFile(string path)
        {
            var uri = new Uri(client.BaseAddress!, path);
            return Path.Combine(localCache, uri.DnsSafeHost, uri.AbsolutePath.Substring(1).Replace('/', Path.DirectorySeparatorChar));
        }

        public async Task<IDemDataCell> LoadAsync(string path, string? expectedSha256 = null, CancellationToken cancellationToken = default)
        {
            var cacheFile = GetCacheFile(path);
            if (File.Exists(cacheFile))
            {
                if (expectedSha256 != null && !await Sha256Helper.VerifyAsync(cacheFile, expectedSha256, cancellationToken).ConfigureAwait(false))
                {
                    File.Delete(cacheFile);
                }
            }

            if (!File.Exists(cacheFile))
            {
                await DownloadFile(path, cacheFile, cancellationToken).ConfigureAwait(false);
                if (expectedSha256 != null && !await Sha256Helper.VerifyAsync(cacheFile, expectedSha256, cancellationToken).ConfigureAwait(false))
                {
                    File.Delete(cacheFile);
                    throw new InvalidDataException($"SHA-256 checksum mismatch for '{path}'. The downloaded file may be corrupted.");
                }
            }

            return DemDataCell.Load(cacheFile);
        }

        private async Task DownloadFile(string path, string cacheFile, CancellationToken cancellationToken = default)
        {
            var cacheDirectory = Path.GetDirectoryName(cacheFile)!;

            Directory.CreateDirectory(cacheDirectory);

            var tempFile = Path.Combine(cacheDirectory, Path.GetRandomFileName());

            for (int attempt = 1; attempt <= MaxDownloadAttempts; attempt++)
            {
                try
                {
                    using (var input = await client.GetStreamAsync(path, cancellationToken).ConfigureAwait(false))
                    {
                        using var cache = File.Create(tempFile);
                        await input.CopyToAsync(cache, cancellationToken).ConfigureAwait(false);
                    }
                    File.Move(tempFile, cacheFile, true);
                    return;
                }
                catch (HttpRequestException httpException) when (httpException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
                catch (Exception) when (attempt < MaxDownloadAttempts && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Random.Shared.Next(500, 5000), cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
        }

        public Task<IDemDataCell> Load(string path) => LoadAsync(path, null);

        public async Task<DemDatabaseIndex> ReadIndex()
        {
            var cacheFile = GetCacheFile("index.json");

            if (!File.Exists(cacheFile) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFile)) > IndexCacheDuration)
            {
                await DownloadFile("index.json", cacheFile).ConfigureAwait(false);
            }

            using (var stream = File.OpenRead(cacheFile))
            {
                return (await JsonSerializer.DeserializeAsync<DemDatabaseIndex>(stream, DemDatabaseIndexContext.Default.DemDatabaseIndex).ConfigureAwait(false))!;
            }
        }

        public async Task<string?> GetSha256Async(string path, CancellationToken cancellationToken = default)
        {
            var cacheFile = GetCacheFile(path);
            try
            {
                await DownloadFile(path, cacheFile, cancellationToken).ConfigureAwait(false);
            }
            catch(HttpRequestException httpException) when (httpException.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the file doesn't exist on the server, we won't be able to get its SHA-256.
                return null;
            }
            return await Sha256Helper.ComputeHexAsync(cacheFile, cancellationToken).ConfigureAwait(false);
        }
    }
}

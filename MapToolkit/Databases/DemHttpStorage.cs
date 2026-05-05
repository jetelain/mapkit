using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    /// <summary>
    /// An <see cref="IDemStorage"/> implementation that downloads DEM tiles on demand from an HTTP
    /// server and caches them locally on disk.
    /// </summary>
    /// <remarks>
    /// Downloaded tiles are stored under <see cref="DefaultCacheLocation"/> (or a custom path supplied
    /// to the constructor) and reused on subsequent requests, so network access only occurs when a
    /// tile is not yet present in the cache.
    /// </remarks>
    public class DemHttpStorage : IDemStorage
    {
        /// <summary>
        /// Gets or sets how long a cached <c>index.json</c> is considered fresh before being
        /// re-downloaded. Defaults to 24 hours.
        /// </summary>
        public static TimeSpan IndexCacheDuration { get; set; } = TimeSpan.FromHours(24);

        const int MaxDownloadAttempts = 3;

        private readonly string localCache;
        private readonly HttpClient client;

        /// <summary>
        /// Initialises a new instance of <see cref="DemHttpStorage"/> with an already-configured
        /// <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="localCache">
        /// Directory used to cache downloaded tiles. Pass <see langword="null"/> to use
        /// <see cref="DefaultCacheLocation"/>.
        /// </param>
        /// <param name="client">
        /// An <see cref="HttpClient"/> whose <see cref="HttpClient.BaseAddress"/> points to the DEM
        /// server root.
        /// </param>
        public DemHttpStorage(string? localCache, HttpClient client)
        {
            this.localCache = localCache ?? DefaultCacheLocation;
            this.client = client;
        }

        /// <summary>
        /// Initialises a new instance of <see cref="DemHttpStorage"/> that creates its own
        /// <see cref="HttpClient"/> for the given base address.
        /// </summary>
        /// <param name="localCache">
        /// Directory used to cache downloaded tiles. Pass <see langword="null"/> to use
        /// <see cref="DefaultCacheLocation"/>.
        /// </param>
        /// <param name="baseAddress">Base URI of the DEM HTTP server.</param>
        public DemHttpStorage(string? localCache, Uri baseAddress)
            : this(localCache, HttpClientHelper.CreateClient(baseAddress))
        {

        }

        /// <summary>
        /// Initialises a new instance of <see cref="DemHttpStorage"/> using
        /// <see cref="DefaultCacheLocation"/> as the local cache directory.
        /// </summary>
        /// <param name="baseAddress">Base URI of the DEM HTTP server.</param>
        public DemHttpStorage(Uri baseAddress)
            : this(null, baseAddress)
        {

        }

        /// <summary>
        /// Gets the default directory used to cache downloaded tiles when no explicit path is
        /// provided. Resolves to a <c>dem</c> sub-directory inside the system temporary folder.
        /// </summary>
        public static string DefaultCacheLocation => Path.Combine(Path.GetTempPath(), "dem");

        /// <summary>
        /// Deletes all files in <see cref="DefaultCacheLocation"/>, freeing disk space occupied by
        /// previously downloaded tiles. Has no effect when the directory does not exist.
        /// </summary>
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

        /// <summary>
        /// Loads a DEM tile from the cache, downloading it first if necessary.
        /// </summary>
        /// <param name="path">Server-relative path of the tile (e.g. <c>srtm1/N51W001.ddc.zst</c>).</param>
        /// <param name="expectedSha256">
        /// Optional expected SHA-256 hex digest. When provided the cached file is verified after
        /// download; a mismatch causes the file to be deleted and an
        /// <see cref="InvalidDataException"/> to be thrown.
        /// </param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The loaded <see cref="IDemDataCell"/>.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown when <paramref name="expectedSha256"/> is set and the downloaded file does not
        /// match the expected digest.
        /// </exception>
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

        /// <inheritdoc cref="LoadAsync(string, string?, CancellationToken)"/>
        public Task<IDemDataCell> Load(string path) => LoadAsync(path, null);

        /// <summary>
        /// Downloads (if required) and deserialises the server's <c>index.json</c> file.
        /// The cached copy is reused as long as it is younger than <see cref="IndexCacheDuration"/>.
        /// </summary>
        /// <returns>The deserialized <see cref="DemDatabaseIndex"/>.</returns>
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

        /// <summary>
        /// Downloads the file at <paramref name="path"/> and returns its SHA-256 hex digest, or
        /// <see langword="null"/> if the file does not exist on the server.
        /// </summary>
        /// <param name="path">Server-relative path of the file to hash.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A lowercase hex string representing the SHA-256 digest, or <see langword="null"/> when
        /// the server returns HTTP 404.
        /// </returns>
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

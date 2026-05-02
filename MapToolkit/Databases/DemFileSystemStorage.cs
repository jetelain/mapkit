using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    public class DemFileSystemStorage : IDemStorage
    {
        private readonly string basePath;

        public DemFileSystemStorage(string basePath)
        {
            this.basePath = basePath;
        }

        public async Task<DemDatabaseIndex> ReadIndex()
        {
            var indexFile = Path.Combine(basePath, "index.json");
            if (File.Exists(indexFile))
            {
                using (var input = File.OpenRead(indexFile))
                {
                    return (await JsonSerializer.DeserializeAsync<DemDatabaseIndex>(input, DemDatabaseIndexContext.Default.DemDatabaseIndex).ConfigureAwait(false))!;
                }
            }
            return await BuildIndexAsync().ConfigureAwait(false);
        }

        [Obsolete]
        public DemDatabaseIndex BuildIndex()
        {
            return BuildIndexAsync().GetAwaiter().GetResult();
        }

        public async Task<DemDatabaseIndex> BuildIndexAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {   
            var entries = new List<DemDatabaseFileInfos>();
            var files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            var done = 0;
            foreach (var file in files)
            {
                if (DemDataCell.IsDemDataCellFile(file))
                {
                    entries.Add(new DemDatabaseFileInfos(GetRelative(file), DemDataCell.LoadMetadata(file), await Sha256Helper.ComputeHexAsync(file, cancellationToken).ConfigureAwait(false)));
                }
                done++;
                progress?.Report((double)done / files.Length);
            }
            return new DemDatabaseIndex(entries);
        }

        private string GetRelative(string file)
        {
            return file.Substring(basePath.Length).TrimStart('/', '\\');
        }

        public Task<IDemDataCell> Load(string path)
        {
            return LoadAsync(path, null);
        }

        public async Task<IDemDataCell> LoadAsync(string path, string? expectedSha256, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(basePath, path);
            if (expectedSha256 != null && !await Sha256Helper.VerifyAsync(fullPath, expectedSha256, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidDataException($"SHA-256 checksum mismatch for '{path}'.");
            }
            return DemDataCell.Load(fullPath);
        }

        public async Task<string?> GetSha256Async(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(basePath, path);
            if (File.Exists(fullPath))
            {
                return await Sha256Helper.ComputeHexAsync(fullPath).ConfigureAwait(false);
            }
            return null;
        }
    }
}

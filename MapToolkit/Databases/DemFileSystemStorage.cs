using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MapToolkit.DataCells;

namespace MapToolkit.Databases
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
                    return (await JsonSerializer.DeserializeAsync<DemDatabaseIndex>(input).ConfigureAwait(false))!;
                }
            }
            return BuildIndex();
        }

        public DemDatabaseIndex BuildIndex()
        {
            var entries = new List<DemDatabaseFileInfos>();
            var files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (DemDataCell.IsDemDataCellFile(file))
                {
                    entries.Add(new DemDatabaseFileInfos(GetRelative(file), DemDataCell.LoadMetadata(file)));
                }
            }
            return new DemDatabaseIndex(entries);
        }

        private string GetRelative(string file)
        {
            return file.Substring(basePath.Length).TrimStart('/', '\\');
        }

        public Task<IDemDataCell> Load(string path)
        {
            return Task.FromResult(DemDataCell.Load(Path.Combine(basePath, path)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    internal class FileSystemStorage : IDemStorage
    {
        private readonly string basePath;

        internal FileSystemStorage(string basePath)
        {
            this.basePath = basePath;
        }

        public List<DemDatabaseEntry> ReadIndex()
        {
            var entries = new List<DemDatabaseEntry>();
            var files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (DemDataCell.IsDemDataCellFile(file))
                {
                    entries.Add(new DemDatabaseEntry(file, DemDataCell.LoadMetadata(file)));
                }
            }
            return entries;
        }

        public IDemDataCell Load(DemDatabaseEntry entry)
        {
            return DemDataCell.Load(entry.Path);
        }
    }
}

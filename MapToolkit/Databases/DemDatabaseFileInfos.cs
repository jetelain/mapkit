using System;
using System.Collections.Generic;
using System.Text;
using Pmad.Cartography.DataCells;
using System.Text.Json.Serialization;

namespace Pmad.Cartography.Databases
{
    public class DemDatabaseFileInfos
    {
        [JsonConstructor]
        public DemDatabaseFileInfos(string path, DemDataCellMetadata metadata, string? sha256 = null)
        {
            Path = path; 
            Metadata = metadata;
            Sha256 = sha256;
        }

        public DemDatabaseFileInfos(string path, IDemDataCellMetadata metadata, string? sha256 = null)
        {
            Path = path;
            Metadata = metadata as DemDataCellMetadata ?? new DemDataCellMetadata(metadata);
            Sha256 = sha256;
        }

        public string Path { get; }

        public DemDataCellMetadata Metadata { get; }

        /// <summary>
        /// Optional SHA-256 hex digest of the cell file, used to verify integrity after download.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Sha256 { get; }
    }
}

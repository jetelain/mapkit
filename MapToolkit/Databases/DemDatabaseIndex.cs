using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MapToolkit.Databases
{
    public class DemDatabaseIndex
    {
        [JsonConstructor]
        public DemDatabaseIndex(List<DemDatabaseFileInfos> cells)
        {
            Cells = cells;
        }

        public List<DemDatabaseFileInfos> Cells { get; }
    }
}

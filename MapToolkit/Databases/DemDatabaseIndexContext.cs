using System.Text.Json.Serialization;

namespace MapToolkit.Databases
{
    [JsonSerializable(typeof(DemDatabaseIndex))]
    internal partial class DemDatabaseIndexContext : JsonSerializerContext
    {
    }
}

using System.Text.Json.Serialization;

namespace SimpleDEM.Contours
{
    public class ContourFeatureProperties
    {
        [JsonConstructor]
        public ContourFeatureProperties(double elevation)
        {
            Elevation = elevation;
        }

        public double Elevation { get; }
    }
}

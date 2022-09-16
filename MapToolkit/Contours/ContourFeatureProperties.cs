using System.Text.Json.Serialization;

namespace MapToolkit.Contours
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

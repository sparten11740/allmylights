using AllMyLights.Models.Transformations;

namespace AllMyLights.Models
{
    public abstract class ConnectorOptions
    {
        public string Type { get; set; }
        public TransformationOptions[] Transformations { get; set; }
    }
}

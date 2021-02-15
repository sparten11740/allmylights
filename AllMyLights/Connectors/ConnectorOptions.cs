using AllMyLights.Transformations;

namespace AllMyLights.Connectors
{
    public abstract class ConnectorOptions
    {
        public string Type { get; set; }
        public TransformationOptions[] Transformations { get; set; }
    }
}

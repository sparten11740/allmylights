using System.ComponentModel.DataAnnotations;
using AllMyLights.Transformations;

namespace AllMyLights.Connectors
{
    public abstract class ConnectorOptions
    {
        public string Id { get; set; }
        [Required]
        public string Type { get; set; }
        public TransformationOptions[] Transformations { get; set; }
    }
}

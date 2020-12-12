using System.Runtime.Serialization;
using AllMyLights.JsonConverters;
using AllMyLights.Models.OpenRGB;
using Newtonsoft.Json;

namespace AllMyLights.Models
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SinkOptions))]
    [KnownType(typeof(OpenRGBSinkOptions))]
    public abstract class SinkOptions : ConnectorOptions
    {
        public string Foo { get; set; }
    }
}

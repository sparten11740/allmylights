using System.Runtime.Serialization;
using AllMyLights.Connectors.Sources.Mqtt;
using AllMyLights.Connectors.Sources.OpenRGB;
using AllMyLights.JsonConverters;
using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sources
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SourceOptions))]
    [
        KnownType(typeof(MqttSourceOptions)),
        KnownType(typeof(OpenRGBSourceOptions))
    ]
    public abstract class SourceOptions: ConnectorOptions
    {
    }
}

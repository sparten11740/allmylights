using System.Runtime.Serialization;
using AllMyLights.Connectors.Sources.Mqtt;
using AllMyLights.JsonConverters;
using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sources
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SourceOptions))]
    [KnownType(typeof(MqttSourceOptions))]
    public abstract class SourceOptions: ConnectorOptions
    {
    }
}

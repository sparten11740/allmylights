using System.Runtime.Serialization;
using AllMyLights.JsonConverters;
using AllMyLights.Models.Mqtt;
using Newtonsoft.Json;

namespace AllMyLights.Models
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SourceOptions))]
    [KnownType(typeof(MqttSourceOptions))]
    public abstract class SourceOptions: ConnectorOptions
    {
    }
}

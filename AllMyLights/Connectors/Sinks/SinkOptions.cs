using System.Runtime.Serialization;
using AllMyLights.Connectors.Sinks.OpenRGB;
using AllMyLights.JsonConverters;
using AllMyLights.Models.OpenRGB;
using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SinkOptions))]
    [
        KnownType(typeof(OpenRGBSinkOptions)),
        KnownType(typeof(WallpaperSinkOptions)),
    ]
    public abstract class SinkOptions : ConnectorOptions
    {
    }
}

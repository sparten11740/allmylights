using System.Runtime.Serialization;
using AllMyLights.Connectors.Sinks.Chroma;
using AllMyLights.Connectors.Sinks.OpenRGB;
using AllMyLights.Connectors.Sinks.Wallpaper;
using AllMyLights.JsonConverters;
using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks
{
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(SinkOptions))]
    [
        KnownType(typeof(OpenRGBSinkOptions)),
        KnownType(typeof(WallpaperSinkOptions)),
        KnownType(typeof(ChromaSinkOptions)),
        KnownType(typeof(MqttSinkOptions)),
    ]
    public abstract class SinkOptions : ConnectorOptions
    {
    }
}

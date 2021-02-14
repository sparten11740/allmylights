using System.Runtime.Serialization;
using AllMyLights.JsonConverters;
using AllMyLights.Models.OpenRGB;
using Newtonsoft.Json;

namespace AllMyLights.Models
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

using System.Collections.Generic;

namespace AllMyLights.Models.OpenRGB
{
    public class OpenRGBSinkOptions: SinkOptions
    {
        public string Server { get; set; }
        public int? Port { get; set; }

        public Dictionary<string, DeviceOverride> Overrides { get; set; }
    }
}



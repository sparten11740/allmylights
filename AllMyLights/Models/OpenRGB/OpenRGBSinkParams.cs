using System.Collections.Generic;

namespace AllMyLights.Models.OpenRGB
{
    public class OpenRGBSinkParams
    {
        public int? Port { get; set; }
        public string Server { get; set; }

        public Dictionary<string, DeviceOverride> Overrides { get; set; }
    }
}



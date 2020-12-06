using System.Collections.Generic;

namespace AllMyLights.Models.OpenRGB
{
    public class DeviceOverride
    {
        public string ChannelLayout { get; set; }
        public bool Ignore { get; set; }

        public Dictionary<string, ZoneOverride> Zones { get; set; }
    }
}



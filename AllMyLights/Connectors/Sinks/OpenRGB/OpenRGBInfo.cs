using System.Collections.Generic;
using OpenRGB.NET.Models;

namespace AllMyLights.Connectors.Sinks.OpenRGB
{
    public struct OpenRGBInfo
    {
        public OpenRGBInfo(IEnumerable<Device> devices, IEnumerable<string> profiles)
        {
            Devices = devices;
            Profiles = profiles;
        }

        public IEnumerable<Device> Devices { get; }
        public IEnumerable<string> Profiles { get; }
    }
}

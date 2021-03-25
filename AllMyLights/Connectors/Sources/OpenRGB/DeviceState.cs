using System.Collections.Generic;

namespace AllMyLights.Connectors.Sources.OpenRGB
{
    public struct DeviceState
    {
        public IEnumerable<string> Colors;

        public DeviceState(IEnumerable<string> colors)
        {
            Colors = colors;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;

namespace AllMyLights.Connectors.Sources.OpenRGB
{
    public struct DeviceState
    {
        public IEnumerable<Color> Colors;

        public DeviceState(IEnumerable<Color> colors)
        {
            Colors = colors;
        }
    }
}

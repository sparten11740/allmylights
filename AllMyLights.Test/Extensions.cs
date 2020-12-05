using System.Linq;
using OpenRGB.NET.Models;

namespace AllMyLights.Test
{
    public static class Extensions
    {

        public static Device SetColors(this Device device, params Color[] colors)
        {
            var type = typeof(Device);
            type.GetProperty("Colors").SetValue(device, colors);

            return device;
        }

        public static Device SetName(this Device device, string name)
        {
            var type = typeof(Device);
            type.GetProperty("Name").SetValue(device, name);

            return device;
        }

        public static Device SetZones(this Device device, params Zone[] zones)
        {
            var type = typeof(Device);
            type.GetProperty(nameof(Device.Zones)).SetValue(device, zones);

            return device;
        }


        public static Zone Set(
            this Zone zone,
            string name,
            uint ledCount
        )
        {
            var type = typeof(Zone);
            type.GetProperty(nameof(Zone.Name)).SetValue(zone, name);
            type.GetProperty(nameof(Zone.LedCount)).SetValue(zone, ledCount);

            return zone;
        }

        public static bool ContainsExactly(this Color[] colors, params Color[] otherColors)
        {
            var matches = true;
            var size = colors.Count();
            var otherSize = otherColors.Count();

            if (size != otherSize) return false;

            for (int i = 0; i < size; i++)
            {
                if (!colors[i].Equals(otherColors[i]))
                {
                    matches = false;
                    break;
                }
            }

            return matches;
        }
    }
}

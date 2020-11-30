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

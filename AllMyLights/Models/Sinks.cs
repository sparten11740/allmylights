using AllMyLights.Models.OpenRGB;
using System.Collections.Generic;

namespace AllMyLights.Models
{
    public class Sinks
    {
        public IEnumerable<OpenRGBSinkParams> OpenRgb { get; set; }
    }
}

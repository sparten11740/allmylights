using System.Collections.Generic;

namespace AllMyLights.Connectors.Sources
{
    public class RouteOptions
    {
        public string From { get; set; }
        public IEnumerable<string> To { get; set; }
    }
}

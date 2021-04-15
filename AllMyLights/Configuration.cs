using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using System.Collections.Generic;

namespace AllMyLights
{
    public class Configuration
    {
        public IEnumerable<SourceOptions> Sources { get; set; }
        public IEnumerable<SinkOptions> Sinks { get; set; }

        public IEnumerable<RouteOptions> Routes { get; set; } = new List<RouteOptions>();
    }
}



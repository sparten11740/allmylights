using System.Collections.Generic;

namespace AllMyLights.Models
{
    public class Configuration
    {
        public IEnumerable<SourceOptions> Sources { get; set; }
        public IEnumerable<SinkOptions> Sinks { get; set; }
    }
}



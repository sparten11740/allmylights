using System.Collections.Generic;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class ChromaSinkOptions : SinkOptions
    {
        public IEnumerable<string> SupportedDevices { get; set; }
    }
}

using Newtonsoft.Json;
using System.Collections.Generic;

namespace AllMyLights.Connectors.Sinks.Chroma
{

    public class ChromaApp
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("author")]
        public ChromaAuthor Author { get; set; }

        [JsonProperty("device_supported")]
        public IEnumerable<string> SupportedDevices { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = "application";

        public class ChromaAuthor
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("contact")]
            public string Contact { get; set; }
        }
    }
}
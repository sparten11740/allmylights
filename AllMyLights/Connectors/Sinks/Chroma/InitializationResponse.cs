using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class InitializationResponse
    {
        [JsonProperty("sessionid")]
        public string SessionId { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("result")]
        public int? Result { get; set; }
    }
}

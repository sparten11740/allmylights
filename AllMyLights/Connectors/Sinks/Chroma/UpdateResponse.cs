using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class UpdateResponse
    {
        [JsonProperty("result")]
        public int Result { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}

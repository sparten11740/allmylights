using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class ChromaEffect
    {
        [JsonProperty("effect")]
        public string Effect { get; set; } = "CHROMA_STATIC";
        [JsonProperty("param")]
        public object Param { get; set; }
    }
}

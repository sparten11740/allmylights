
namespace AllMyLights.Connectors.Sources.OpenRGB
{
    public class OpenRGBSourceOptions: SourceOptions
    {

        public string Server { get; set; }
        public int? Port { get; set; }

        public int? PollingInterval { get; set; }
    }
}



using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Connectors.Sources.Mqtt
{
    public class MqttSourceOptions: SourceOptions
    {
        [Required]
        public string Server { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [Required]
        public Topics Topics { get; set; }
    }
}



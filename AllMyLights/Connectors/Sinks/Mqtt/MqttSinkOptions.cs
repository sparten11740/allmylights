using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class MqttSinkOptions : SinkOptions
    {
        [Required]
        public string Server { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [Required]
        public IEnumerable<string> Topics { get; set; }
    }
}

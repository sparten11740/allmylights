using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;

namespace AllMyLights.Models.Mqtt
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



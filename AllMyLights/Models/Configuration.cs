using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models
{
    public class Configuration
    {
        [Required]
        public MqttConfiguration Mqtt { get; set; }

        public OpenRGBConfiguration OpenRgb { get; set; }
    }

    public class OpenRGBConfiguration
    {
        public int? Port { get; set; }
        public string Server { get; set; }
    }

    public class MqttConfiguration
    {
        [Required]
        public string Server { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [Required]
        public Topics Topics { get; set; }
    }

    public class Topics
    {
        [Required]
        public string Command { get; set; }

        [Required]
        public Topic Result { get; set; }
    }

    public class Topic
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public string ValuePath { get; set; }

        public string ChannelLayout { get; set; }
    }
}



using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Mqtt
{
    public class Topic
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public string ValuePath { get; set; }

        public string ChannelLayout { get; set; }
    }
}



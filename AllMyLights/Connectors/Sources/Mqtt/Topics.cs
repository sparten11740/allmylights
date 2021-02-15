using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Connectors.Sources.Mqtt
{
    public class Topics
    {
        public string Command { get; set; }

        [Required]
        public string Result { get; set; }
    }
}



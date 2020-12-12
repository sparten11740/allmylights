using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Mqtt
{
    public class Topics
    {
        public string Command { get; set; }

        [Required]
        public string Result { get; set; }
    }
}



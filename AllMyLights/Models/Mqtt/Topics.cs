using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Mqtt
{
    public class Topics
    {
        [Required]
        public string Command { get; set; }

        [Required]
        public Topic Result { get; set; }
    }
}



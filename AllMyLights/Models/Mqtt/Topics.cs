using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Mqtt
{
    public class Topics
    {
        public string Command { get; set; }

        [Required]
        public Topic Result { get; set; }
    }
}



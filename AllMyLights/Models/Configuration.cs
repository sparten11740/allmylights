using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AllMyLights.Models
{
    public class Configuration
    {
        [Required]
        public string MQTTStateTopic { get; set; }
    }

}

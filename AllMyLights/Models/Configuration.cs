using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models
{
    public class Configuration
    {
        public Sources Sources { get; set; }
        public Sinks Sinks { get; set; }
    }
}



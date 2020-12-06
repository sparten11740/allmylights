using AllMyLights.Models.Mqtt;
using System.Collections.Generic;

namespace AllMyLights.Models
{
    public class Sources
    {
        public IEnumerable<MqttSourceParams> Mqtt { get; set; }
    }
}

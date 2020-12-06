using AllMyLights.Connectors.Sinks;
using AllMyLights.Models;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unmockable;

namespace AllMyLights.Connectors.Sources
{
    public class ConnectorFactory
    {
        private Configuration Configuration { get; }
        public ConnectorFactory(Configuration configuration)
        {
            Configuration = configuration;
        }

        public ISource[] GetSources()
        {
            return Configuration.Sources?.Mqtt
                ?.Select((it) => new MqttSource(
                    options: it,
                    mqttClient: new MqttFactory().CreateMqttClient()))
                .ToArray();
        }

        public ISink[] GetSinks()
        {
            return Configuration.Sinks?.OpenRgb
                ?.Select((options) => new OpenRGBSink(options, (new OpenRGB.NET.OpenRGBClient(
               ip: options?.Server ?? "127.0.0.1",
               port: options?.Port ?? 6742,
               autoconnect: false).Wrap())))
                .ToArray();
        }
    }
}

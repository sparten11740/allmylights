using System;
using System.Linq;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Models;
using AllMyLights.Models.Mqtt;
using AllMyLights.Models.OpenRGB;
using MQTTnet;
using NLog;

namespace AllMyLights.Connectors.Sources
{
    public class ConnectorFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Configuration Configuration { get; }
        public ConnectorFactory(Configuration configuration)
        {
            Configuration = configuration;
        }

        private OpenRGBSink GetOpenRGBSinkInstance(OpenRGBSinkOptions options) =>
            new OpenRGBSink(options, client: new OpenRGB.NET.OpenRGBClient(
                ip: options?.Server ?? "127.0.0.1",
                port: options?.Port ?? 6742,
                autoconnect: false));

        public ISource[] GetSources()
        {
            Logger.Info($"Configuring {Configuration.Sources.Count()} sources");
            return Configuration.Sources.Select(sourceOptions => sourceOptions switch
            {
                MqttSourceOptions options => new MqttSource(options, new MqttFactory().CreateMqttClient()),
                _ => throw new NotImplementedException($"Source for type {sourceOptions.Type} not implemented")
            }).ToArray();
        }

        public ISink[] GetSinks()
        {
            Logger.Info($"Configuring {Configuration.Sinks.Count()} sinks");
            return Configuration.Sinks.Select(sinkOptions => sinkOptions switch
            {
                OpenRGBSinkOptions options => GetOpenRGBSinkInstance(options),
                _ => throw new NotImplementedException($"Sinks for type {sinkOptions.Type} not implemented")
            }).ToArray();
        }
    }
}

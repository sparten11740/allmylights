using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sinks.Chroma;
using AllMyLights.Connectors.Sinks.OpenRGB;
using AllMyLights.Connectors.Sinks.Wallpaper;
using AllMyLights.Connectors.Sources;
using AllMyLights.Connectors.Sources.Mqtt;
using AllMyLights.Models.OpenRGB;
using AllMyLights.Platforms;
using MQTTnet;
using NLog;

namespace AllMyLights.Connectors
{
    public class ConnectorFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient = new HttpClient();

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
            return Configuration.Sources.Select<SourceOptions, ISource>(sourceOptions => sourceOptions switch
            {
                MqttSourceOptions options => new MqttSource(options, new MqttFactory().CreateMqttClient()),
                _ => throw new NotImplementedException($"Source for type {sourceOptions.Type} not implemented")
            }).ToArray();
        }

        public ISink[] GetSinks()
        {
            Logger.Info($"Configuring {Configuration.Sinks.Count()} sinks");
            return Configuration.Sinks.Select<SinkOptions, ISink>(sinkOptions => sinkOptions switch
            {
                OpenRGBSinkOptions options => GetOpenRGBSinkInstance(options),
                WallpaperSinkOptions options => new WallpaperSink(options, Desktop.GetPlatformInstance()),
                ChromaSinkOptions options => new ChromaSink(options, new ChromaClient(HttpClient), Observable.Interval(TimeSpan.FromSeconds(2))),
                _ => throw new NotImplementedException($"Sinks for type {sinkOptions.Type} not implemented")
            }).ToArray();
        }
    }
}

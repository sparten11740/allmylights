using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sinks.Chroma;
using AllMyLights.Connectors.Sinks.OpenRGB;
using AllMyLights.Connectors.Sinks.Wallpaper;
using AllMyLights.Connectors.Sources;
using AllMyLights.Connectors.Sources.Mqtt;
using AllMyLights.Connectors.Sources.OpenRGB;
using AllMyLights.Extensions;
using AllMyLights.Platforms;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NLog;
using OpenRGB.NET;

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

        private static Dictionary<string, IOpenRGBClient> OpenRGBClientRegistry { get; } = new Dictionary<string, IOpenRGBClient>();


        private IOpenRGBClient GetOpenRGBClientInstance(string server, int? port)
        {
            var portOrDefault = port ?? 6742;
            var serverOrDefault = server ?? "127.0.0.1";

            return OpenRGBClientRegistry.SetDefault($"{serverOrDefault}:{portOrDefault}", () => new OpenRGBClient(
                ip: serverOrDefault,
                port: portOrDefault,
                autoconnect: false
            ));
        }

        public ISource[] GetSources()
        {
            Logger.Info($"Configuring {Configuration.Sources.Count()} sources");
            return Configuration.Sources.Select<SourceOptions, ISource>(sourceOptions => sourceOptions switch
            {
                MqttSourceOptions options => new MqttSource(options, new MqttFactory().CreateManagedMqttClient()),
                OpenRGBSourceOptions options => new OpenRGBSource(
                    options,
                    GetOpenRGBClientInstance(options.Server, options.Port),
                    Observable.Interval(TimeSpan.FromMilliseconds(options.PollingInterval ?? 1000))
                ),
                _ => throw new NotImplementedException($"Source for type {sourceOptions.Type} not implemented")
            }).ToArray();
        }

        public ISink[] GetSinks()
        {
            Logger.Info($"Configuring {Configuration.Sinks.Count()} sinks");
            return Configuration.Sinks.Select<SinkOptions, ISink>(sinkOptions => sinkOptions switch
            {
                OpenRGBSinkOptions options => new OpenRGBSink(options, GetOpenRGBClientInstance(options.Server, options.Port)),
                MqttSinkOptions options => new MqttSink(options, new MqttFactory().CreateManagedMqttClient()),
                WallpaperSinkOptions options => new WallpaperSink(options, Desktop.GetPlatformInstance()),
                ChromaSinkOptions options => new ChromaSink(options, new ChromaClient(HttpClient), Observable.Interval(TimeSpan.FromSeconds(2))),
                _ => throw new NotImplementedException($"Sinks for type {sinkOptions.Type} not implemented")
            }).ToArray();
        }
    }
}

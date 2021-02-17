using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AllMyLights.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class ChromaSink : Sink
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private List<string> SupportedDevices { get; }
        private IChromaClient Client { get; }

        public ChromaSink(ChromaSinkOptions options, IChromaClient client, IObservable<long> heartbeatInterval) : base(options)
        {
            SupportedDevices = (options.SupportedDevices ?? new string[] { }).ToList();
            Client = client;
            Client.InitializeAsync().Wait();

            heartbeatInterval.Subscribe(async (_) =>
            {
                await Client.SendHeartbeatAsync();
            });

            Next.Subscribe(async (value) =>
            {
                switch (value)
                {
                    case Ref<Color> it:
                        await ApplyStaticEffect(it.Value);
                        break;
                    case string it:
                        await Update(it);
                        break;
                    default:
                        Logger.Error($"{nameof(ChromaSink)} received type {value.GetType()} it cannot handle. Please provide a {typeof(Color)} or a string containing a Chroma compatible payload");
                        break;
                }
            });
        }

        private async Task Update(string payload)
        {
            Logger.Debug(() => $"{nameof(ChromaSink)} received payload: {payload}. Applying to {string.Join(", ", SupportedDevices)}");
            try
            {
                JObject.Parse(payload);
                await Task.WhenAll(SupportedDevices.Select((device) => Client.UpdateAsync(device, payload)));
            }
            catch (JsonReaderException e)
            {
                Logger.Debug(() => $"{nameof(ChromaSink)} received invalid json: {payload} ({e.Message})");
            }
        }

        private async Task ApplyStaticEffect(Color color)
        {
            Logger.Debug(() => $"{nameof(ChromaSink)} received {color}. Applying to {string.Join(", ", SupportedDevices)}");
            await Task.WhenAll(SupportedDevices.Select((device) => Client.ApplyStaticEffectAsync(device, color)));
        }
    }
}

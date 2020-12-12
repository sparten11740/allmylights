using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reflection;
using AllMyLights.Common;
using AllMyLights.Models.OpenRGB;
using NLog;
using OpenRGB.NET;
using OpenRGB.NET.Models;

namespace AllMyLights.Connectors.Sinks
{
    public class OpenRGBSink: Sink
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IOpenRGBClient Client { get; }
        private OpenRGBSinkOptions Options { get; }

        public OpenRGBSink(
            OpenRGBSinkOptions options,
            IOpenRGBClient client): base(options)
        {
            Client = client;
            Options = options;
            Next.Subscribe((color) =>
            {
                switch (color)
                {
                    case Ref<System.Drawing.Color> it:
                        UpdateAll(it);
                        break;
                    default:
                        Logger.Error($"Sink {nameof(OpenRGBSink)} received type {color.GetType()} it cannot handle. Please provide a {typeof(System.Drawing.Color)}");
                        break;
                }

            });
        }

        public override IEnumerable<object> GetConsumers() => RequestCatching(() => {
            return Client.GetAllControllerData();
        });

        private Unit UpdateAll(Ref<System.Drawing.Color> colorRef) => RequestCatching(() =>
        {
            var color = colorRef.Value;
            Logger.Info($"Changing color to {color}");

            var count = Client.GetControllerCount();
            var devices = Client.GetAllControllerData();
            Logger.Info($"Found {count} devices to update");

            for (int id = 0; id < devices.Length; id++)
            {
                Device device = devices[id];
                var config = Options.Overrides?.GetValueOrDefault(device.Name);
                var ignore = config?.Ignore;
                if (ignore == true) break;

                var layout = config?.ChannelLayout;
                var deviceColor = (layout != null ? color.Rearrange(layout) : color).ToOpenRGBColor();

                if (config?.Zones != null)
                {
                    for (int zoneId = 0; zoneId < device.Zones.Length; zoneId++)
                    {
                        var zone = device.Zones[zoneId];
                        var zoneConfig = config.Zones.GetValueOrDefault(zone.Name);
                        var zoneLayout = zoneConfig?.ChannelLayout;
                        var zoneColor = (zoneLayout != null ? color.Rearrange(zoneLayout).ToOpenRGBColor() : deviceColor);

                        Client.UpdateZone(id, zoneId, Enumerable.Range(0, (int)zone.LedCount).Select(_ => zoneColor).ToArray());
                    };
                    continue;
                }

                var leds = device.Colors
                    .Select(_ => deviceColor)
                    .ToArray();

                Client.UpdateLeds(id, leds);
            }
            return Unit.Default;
        });

        private T RequestCatching<T>(Func<T> request)
        {
            try
            {
                if (!Client.Connected)
                {
                    Client.Connect();
                }

                return request();
            }
            catch (Exception e)
            {
                Logger.Error($"Disconnected from OpenRGB ({e.Message})");
                return Reconnect(() => RequestCatching(request));
            }
        }

        private T Reconnect<T>(Func<T> onSuccess)
        {
            // we have to be a little nasty here, since the client library does not expose any means of reconnecting
            var clientType = typeof(OpenRGBClient);
            var socketField = clientType.GetField("_socket", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                socketField.SetValue(Client, new Socket(SocketType.Stream, ProtocolType.Tcp));
                Client.Connect();
                return onSuccess();
            }
            catch (Exception e)
            {
                Logger.Error($"Connection to OpenRGB failed: {e.Message}");
            }

            return default;
        }

        public override string ToString() =>$"OpenRGBSink({Options.Server}:{Options.Port})";
    }
}

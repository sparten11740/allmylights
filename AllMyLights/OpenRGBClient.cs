using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reflection;
using AllMyLights.Models;
using NLog;
using OpenRGB.NET.Models;
using Unmockable;

namespace AllMyLights
{
    public class OpenRGBClient : IOpenRGBClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IUnmockable<OpenRGB.NET.OpenRGBClient> Client { get; }
        private Configuration Configuration { get; }

        public OpenRGBClient(IUnmockable<OpenRGB.NET.OpenRGBClient> openRGBClient, Configuration configuration)
        {
            Client = openRGBClient;
            Configuration = configuration;
        }


        public Unit UpdateAll(System.Drawing.Color color) => RequestCatching(() =>
        {
            Logger.Info($"Changing color to {color}");

            var count = Client.Execute(it => it.GetControllerCount());
            var devices = Client.Execute(it => it.GetAllControllerData());
            Logger.Info($"Found {count} devices to update");

            for (int id = 0; id < devices.Length; id++)
            {
                Device device = devices[id];
                var config = Configuration.OpenRgb.Overrides?.GetValueOrDefault(device.Name);
                var ignore = config?.Ignore;
                if (ignore == true) break;

                var layout = config?.ChannelLayout;
                var deviceColor = (layout != null ? color.Rearrange(layout) : color).ToOpenRGBColor();

                if(config?.Zones != null)
                {
                    for (int zoneId = 0; zoneId < device.Zones.Length; zoneId++)
                    {
                        var zone = device.Zones[zoneId];
                        var zoneConfig = config.Zones.GetValueOrDefault(zone.Name);
                        var zoneLayout = zoneConfig?.ChannelLayout;
                        var zoneColor = (zoneLayout != null ? color.Rearrange(zoneLayout).ToOpenRGBColor() : deviceColor);

                        Client.Execute((it) => it.UpdateZone(id, zoneId, Enumerable.Range(0, (int)zone.LedCount).Select(_ => zoneColor).ToArray()));
                    };
                    continue;
                }

                var leds = device.Colors
                    .Select(_ => deviceColor)
                    .ToArray();

                Client.Execute((it) => it.UpdateLeds(id, leds));
            }
            return Unit.Default;
        });

        public IEnumerable<Device> GetDevices() => RequestCatching(() => {
            return Client.Execute(it => it.GetAllControllerData());
        });

        private T RequestCatching<T>(Func<T> request)
        {
            try
            {
                if (!Client.Execute(it => it.Connected))
                {
                    Client.Execute(it => it.Connect());
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
            var clientType = typeof(OpenRGB.NET.OpenRGBClient);
            var socketField = clientType.GetField("_socket", BindingFlags.NonPublic | BindingFlags.Instance);
            var socket = Client.Execute((it) => socketField.GetValue(it) as Socket);

            try
            {
                Client.Execute((it) => socketField.SetValue(it, new Socket(SocketType.Stream, ProtocolType.Tcp))); 
                Client.Execute((it) => it.Connect());
                return onSuccess();
            }
            catch (Exception e)
            {
                Logger.Error($"Connection to OpenRGB failed: {e.Message}");
            }

            return default;
        }
    }
}

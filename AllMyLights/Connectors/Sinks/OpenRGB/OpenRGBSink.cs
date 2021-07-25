using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using AllMyLights.Common;
using AllMyLights.Extensions;
using NLog;
using OpenRGB.NET;
using OpenRGB.NET.Models;

namespace AllMyLights.Connectors.Sinks.OpenRGB
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
            Next.Subscribe((value) =>
            {
                switch (value)
                {
                    case Ref<System.Drawing.Color> it:
                        UpdateAll(it);
                        break;
                    case string it:
                        if (it.EndsWith(".orp"))
                        {
                            LoadProfile(it);
                            break;
                        }
                        goto default;
                    default:
                        Logger.Error($"Sink {nameof(OpenRGBSink)} received type {value.GetType()} it cannot handle. Please provide a {typeof(System.Drawing.Color)} or a string containing a profile name.");
                        break;
                }

            });
        }

        public override object GetInfo() => Client.RequestCatching(() => {
            return new OpenRGBInfo(Client.GetAllControllerData(), Client.GetProfiles());
        });

        private Unit LoadProfile(string profile) => Client.RequestCatching(() =>
        {
            var profileName = profile[0..^4];
            var profiles = Client.GetProfiles();

            if (!profiles.Contains(profileName))
            {
                Logger.Error($"Profile {profileName} does not exist on the server.");
                return Unit.Default;
            }

            Client.LoadProfile(profileName);


            return Unit.Default;
        });

        private Unit UpdateAll(Ref<System.Drawing.Color> colorRef) => Client.RequestCatching(() =>
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
                if (ignore == true)
                {
                    Logger.Debug($"Ignoring device {device.Name} as per configuration.");
                    continue;
                }

                if(config != null)
                {
                    Logger.Debug($"Override for device {device.Name} will be applied. ({config.ChannelLayout ?? "no device level channel layout"})");
                }
                else
                {
                    Logger.Debug($"No override for device {device.Name} found");
                }


                var layout = config?.ChannelLayout;
                var deviceColor = (layout != null ? color.Rearrange(layout) : color).ToOpenRGBColor();

                if (config?.Zones != null)
                {
                    Logger.Debug($"Zone level overrides for {device.Name} found.");
                    for (int zoneId = 0; zoneId < device.Zones.Length; zoneId++)
                    {
                        var zone = device.Zones[zoneId];
                        var zoneConfig = config.Zones.GetValueOrDefault(zone.Name);

                        if(zoneConfig != null)
                        {
                            Logger.Debug($"Override for zone {zone.Name} with Ignore={zoneConfig.Ignore} and ChannelLayout={zoneConfig.ChannelLayout} will be applied.");
                        } else
                        {
                            Logger.Debug($"No override for zone {zone.Name} found");
                        }

                        if (zoneConfig?.Ignore == true) continue;

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

        public override string ToString() =>$"{nameof(OpenRGBSink)}({(Id != null ? $"#{Id} " : "")}{Options.Server}:{Options.Port})";
    }
}

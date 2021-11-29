using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using AllMyLights.Extensions;
using AllMyLights.Platforms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace AllMyLights.Connectors.Sinks.Wallpaper
{
    

    public class WallpaperSink: Sink
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDesktop Desktop { get; }

        private string RelativeTo { get; }

        public WallpaperSink(WallpaperSinkOptions options, IDesktop desktop): base(options)
        {
            Desktop = desktop;
            RelativeTo = options.RelativeTo;
            Next.DistinctUntilChanged().Subscribe((raw) =>
            {
                Logger.Debug(@$"{ToString()} received value {raw})");

                if (raw is string value)
                {
                    Logger.Debug($"Applying background. {RelativeTo?.Let((it) => $"Relative to: {it}") ?? ""}");
                    Desktop.SetBackground(PrependRelativeTo(value));
                    return;
                }

                
                if (raw is JObject mapping)
                {
                    try
                    {
                        Logger.Debug("Assuming that received value is mapping of file path to display name.");
                        var filePathByScreen = mapping.ToObject<Dictionary<int, string>>()
                            .ToDictionary(pair => pair.Key, pair => PrependRelativeTo(pair.Value));

                        Logger.Debug(() => $"Activating the following wallpapers: {JsonConvert.SerializeObject(filePathByScreen)}");
                        Desktop.SetBackgrounds(filePathByScreen);
                    }
                    catch(JsonReaderException e)
                    {
                        Logger.Error($"The provided mapping is invalid: {e.Message}");
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                    return;
                }

           

                Logger.Error($"Received value {raw} cannot be consumed by {nameof(WallpaperSink)}");
            });
        }

        public override object GetInfo()
        {
            try
            {
                string[] files = !string.IsNullOrEmpty(RelativeTo) ? Directory.GetFiles(RelativeTo) : null;
                return new WallpaperSinkInfo(RelativeTo, files, Desktop.GetScreens());
            }
            catch(Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        private string PrependRelativeTo(string path)
        {
            if (Path.IsPathRooted(path)) return path;

            return Path.Join(RelativeTo, path);
        }

        public override string ToString() => $"{nameof(WallpaperSink)}({(Id != null ? $"#{Id} " : "")})";
    }
}

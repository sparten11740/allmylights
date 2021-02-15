using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using AllMyLights.Models.OpenRGB;
using AllMyLights.Platforms;
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
            Next.DistinctUntilChanged().Subscribe((value) =>
            {
                if(value is string)
                {
                    Logger.Debug(@$"{nameof(WallpaperSink)} received value {value}. Applying background. {(!string.IsNullOrEmpty(RelativeTo) ? $"Relative to: {RelativeTo}" : "")}");
                    Desktop.SetBackground(PrependRelativeTo(value as string));
                } else
                {
                    Logger.Error($"Received value {value} cannot be consumed by {nameof(WallpaperSink)}");
                }
            });
        }

        public override object GetInfo()
        {
            if (string.IsNullOrEmpty(RelativeTo)) return null;

            try
            {
                string[] files = Directory.GetFiles(RelativeTo);
                return $"Available file names under specified directory {RelativeTo}: {string.Join(", ", files.Select((it) => Path.GetFileName(it)))}";
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

            var preprended = Path.Join(RelativeTo, path);
            return Path.Join(RelativeTo, path);
        }
    }
}

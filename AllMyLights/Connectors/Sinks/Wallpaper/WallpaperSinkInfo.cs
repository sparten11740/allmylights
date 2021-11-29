using System.Collections.Generic;
using Newtonsoft.Json;

namespace AllMyLights.Connectors.Sinks.Wallpaper
{
    public struct WallpaperSinkInfo
    {
        public WallpaperSinkInfo(
            string relativeTo,
            IEnumerable<string> imageFiles,
            IEnumerable<int> screens)
        {
            RelativeTo = relativeTo;
            ImageFiles = imageFiles;
            Screens = screens;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RelativeTo { get; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "Available file names under specified directory")]
        public IEnumerable<string> ImageFiles { get; }
        public IEnumerable<int> Screens { get; }
    }
}

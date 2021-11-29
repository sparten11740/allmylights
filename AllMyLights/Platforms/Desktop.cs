using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AllMyLights.Platforms.Windows;
using NLog;

namespace AllMyLights.Platforms
{
    public abstract class Desktop : IDesktop
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Desktop GetPlatformInstance()
        {
            if (OperatingSystem.IsWindows()) return new WindowsDesktop();

            Logger.Error($"{nameof(Desktop)} not implemented for {RuntimeInformation.OSDescription}");
            Environment.Exit((int)ExitCode.PlatformNotSupported);

            return null;
        }

        public abstract IEnumerable<int> GetScreens();

        public abstract void SetBackground(string filePath);
        public abstract void SetBackgrounds(Dictionary<int, string> filePathByScreen);
    }
}

using System;
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

        public abstract void SetBackground(string filePath);
    }
}

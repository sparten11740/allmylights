using System;
using System.Runtime.InteropServices;
using AllMyLights.Platforms.Linux;
using AllMyLights.Platforms.Windows;
using NLog;

namespace AllMyLights.Platforms
{
    public abstract class AutostartEntry
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static AutostartEntry GetPlatformInstance() 
        {
            if (OperatingSystem.IsLinux()) return new SystemdAutostartEntry();
            if (OperatingSystem.IsWindows()) return new WindowsAutostartEntry();

            Logger.Error($"{nameof(AutostartEntry)} not implemented for {RuntimeInformation.OSDescription}");
            Environment.Exit((int)ExitCode.PlatformNotSupported);

            return null;
        }

        public abstract void Create(string configFile, string logLevel);
    }
}

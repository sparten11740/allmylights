using System;
using System.Diagnostics;
using System.Reflection;
using NLog;

namespace AllMyLights.Platforms.Windows
{
    public class WindowsAutostartEntry : AutostartEntry
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public override void Create(string configFile, string logLevel)
        {

            using var currentProcess = Process.GetCurrentProcess();

            CreateShortcut(
                name: "AllMyLights",
                path: Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                target: currentProcess.MainModule.FileName,
                arguments: $"--config {configFile ?? "allmylightsrc.json"} --log-level {logLevel} --minimized",
                workingDirectory: Environment.CurrentDirectory
            );
        }

        private static void CreateShortcut(string name, string path, string target, string arguments, string workingDirectory)
        {
            Logger.Info($"Creating shortcut {name} in {path} targeting {target} {arguments}");
            string shortcutLocation = System.IO.Path.Combine(path, name + ".lnk");
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);

            var shortcut = shellType.InvokeMember("CreateShortcut",
                  BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                  null, shell, new object[] { shortcutLocation });

            var shortcutType = shortcut.GetType();

            shortcutType.InvokeMember(
                "TargetPath",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                shortcut,
                new object[] { target });

            shortcutType.InvokeMember(
                "Arguments",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                shortcut,
                new object[] { arguments });

            shortcutType.InvokeMember(
                "WorkingDirectory",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                shortcut,
                new object[] { workingDirectory });

            shortcutType.InvokeMember(
                "Save",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                shortcut,
                null);
        }
    }
}

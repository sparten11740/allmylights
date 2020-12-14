using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;

namespace AllMyLights.Platforms.Linux
{

    public class SystemdAutostartEntry: AutostartEntry
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string Identifier = "allmylights";
        private static readonly string ServiceDefinitionFile = $"{Identifier}.service";

        private string InterpolateServiceDefinition(
            string user,
            string workingDirectory,
            string executable,
            string dotnetRoot,
            string configFile,
            string logLevel
        ) => @$"
[Unit]
Description=AllMyLights service to sync colors via MQTT to an OpenRGB instance

[Service]
WorkingDirectory={workingDirectory}
ExecStart={executable} --config {configFile} --log-level {logLevel}
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=allmylights
User={user}
Environment=DOTNET_ROOT={dotnetRoot}

[Install]
WantedBy=multi-user.target
        ";

        public override void Create(string configFile, string logLevel)
        {
            try
            {
                string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

                if (string.IsNullOrEmpty(dotnetRoot))
                {
                    Logger.Warn("Environment variable DOTNET_ROOT not set. App will pt");
                }

                using var currentProcess = Process.GetCurrentProcess(); 

                var definition = InterpolateServiceDefinition(
                    user: Environment.UserName,
                    workingDirectory: Environment.CurrentDirectory,
                    executable: currentProcess.MainModule.FileName,
                    dotnetRoot,
                    configFile ?? "allmylightsrc.json",
                    logLevel
                );

                Console.WriteLine(definition);
                Console.WriteLine("Do you want to proceed creating this service? (yes/no)");

                var decision = Console.ReadLine();
                if (!new string[] { "yes", "y", "ye" }.Contains(decision.ToLower())) return;

                File.WriteAllText(
                    $@"/etc/systemd/system/{ServiceDefinitionFile}",
                    definition
                );

                ExecuteCommand("service", Identifier, "start");
                ExecuteCommand("systemctl", "enable", Identifier);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Environment.Exit((int)ExitCode.Unavailable);
            }
        }

        private void ExecuteCommand(string cmd, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = cmd,
                RedirectStandardError = true
            };
            arguments.ToList().ForEach(startInfo.ArgumentList.Add);

            try
            {
                using var process = Process.Start(startInfo);
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new ApplicationException(output);
                }
            } catch(Exception e)
            {
                Logger.Error($"Couldn't execute {cmd}: {e.Message}");
                throw e;
            }
            
        }
    }
}

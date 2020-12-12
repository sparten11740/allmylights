using System;
using System.IO;
using System.Linq;
using System.Threading;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using AllMyLights.Json;
using AllMyLights.Models;
using Newtonsoft.Json;
using NJsonSchema.Generation;
using NLog;
using NLog.Conditions;
using NLog.Targets;

#if Windows
using System.Windows.Forms;
using AllMyLights.Platforms.Windows;
#endif

namespace AllMyLights
{
    class Program
    {
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// AllMyLights is a tool to sync colors from a home automation bus to OpenRGB managed peripherals via MQTT
        /// </summary>
        /// <param name="config">Path to the config file that contains the MQTT and OpenRGB settings</param>
        /// <param name="logLevel">Change the log level to either debug, info, warn, error, or off.</param>
        /// <param name="logFile">If provided, log output will additionally be captured in the provided file.</param>
        /// <param name="minimized">Minimize to tray after startup</param>
        /// <param name="listDevices">List all available OpenRGB devices and their zones. Returns right away</param>
        /// <param name="failOnUnknownProperty">Fails if an unknown property is encountered in the provided config file. Can be disabled.</param>
        static void Main(
            FileInfo config,
            string logLevel = "warn",
            string logFile = null,
            bool minimized = false,
            bool listDevices = false,
            bool failOnUnknownProperty = true
        )
        {
            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ConfigureLogging(logLevel, logFile);

            new ConfigurationValidator(new JsonSchemaGeneratorSettings
            {
                AllowReferencesWithProperties = true,
                FlattenInheritanceHierarchy = true,
                AlwaysAllowAdditionalObjectProperties = !failOnUnknownProperty
            }).Validate(content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);
            var factory = new ConnectorFactory(configuration);
            var sources = factory.GetSources();
            var sinks = factory.GetSinks();

            if (listDevices)
            {
                ListDevices(sinks);
                Environment.Exit(0);
            }

            var broker = new Broker().RegisterSources(sources).RegisterSinks(sinks);

#if Windows
            var trayIcon = TrayIcon.GetInstance(minimized);
            broker.RegisterSinks(trayIcon);
            ConsoleWindow.Show(!minimized);

            broker.Listen();
            Application.Run();
            trayIcon.Show(false);
#else
            broker.Listen();
            ResetEvent.WaitOne();
#endif
        }

        private static void ListDevices(ISink[] sinks)
        {
            foreach (ISink sink in sinks)
            {
                var devices = sink.GetConsumers();
                if (devices.Count() > 0)
                {
                    Console.WriteLine(sink);
                    Console.WriteLine(JsonConvert.SerializeObject(devices, Formatting.Indented));
                }
                else
                {
                    Console.WriteLine($"{sink} does not expose any device information.");
                }
            }
        }

        private static readonly string[] LogLevels = new string[] { "debug", "info", "warn", "error", "none" };
        private static void ConfigureLogging(string logLevel, string logFile)
        {
            if (!LogLevels.Contains(logLevel))
            {
                Logger.Error($"Log level can only be one of the following: {string.Join(',', LogLevels)}");
                Environment.Exit((int)ExitCode.InvalidArgument);
            }

            var minLevel = logLevel switch
            {
                "info" => LogLevel.Info,
                "debug" => LogLevel.Debug,
                "error" => LogLevel.Error,
                "off" => LogLevel.Off,
                _ => LogLevel.Warn
            };

            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new ColoredConsoleTarget("logconsole")
            {
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} (${level:uppercase=true}): ${message}"
            };

            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                condition: ConditionParser.ParseExpression("(level == LogLevel.Error)"),
                foregroundColor: ConsoleOutputColor.White,
                backgroundColor: ConsoleOutputColor.DarkRed
            ));
            config.AddRule(minLevel, LogLevel.Fatal, logconsole);

            if (logFile != null)
            {
                var logfile = new FileTarget("logfile")
                {
                    FileName = logFile,
                    Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} (${level:uppercase=true}): ${message}"
                };

                config.AddRule(minLevel, LogLevel.Fatal, logfile);
            }

            LogManager.Configuration = config;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using AllMyLights.Json;
using AllMyLights.Models;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using AllMyLights.Platforms;

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
        /// <param name="exportConfigSchemaTo">Writes the config file as Open API v3 schema to the provided filepath and exits afterwards.</param>
        /// <param name="logLevel">Change the log level to either debug, info, warn, error, or off.</param>
        /// <param name="logFile">If provided, log output will additionally be captured in the provided file.</param>
        /// <param name="minimized">Minimize to tray after startup</param>
        /// <param name="listDevices">List device information of devices connected to a sink if available</param>
        /// <param name="failOnUnknownProperty">Fails if an unknown property is encountered in the provided config file. Can be disabled.</param>
        /// <param name="enableAutostart">Setup autostart for logged in user with current folder as working directory</param>
        static void Main(
            FileInfo config,
            FileInfo exportConfigSchemaTo = null,
            string logLevel = "warn",
            string logFile = null,
            bool minimized = false,
            bool listDevices = false,
            bool failOnUnknownProperty = true,
            bool enableAutostart = false
        )
        {
            ConfigureLogging(logLevel, logFile);

            if (exportConfigSchemaTo != null)
            {
                ExportSchema(exportConfigSchemaTo);
                Environment.Exit(0);
            }

            if (enableAutostart)
            {
                AutostartEntry
                    .GetPlatformInstance()
                    .Create(config?.FullName, logLevel);
                Environment.Exit(0);
            }

            if (config == null)
            {
                Logger.Error($"Required parameter --{nameof(config)} not provided");
                Environment.Exit((int)ExitCode.MissingArgument);
            }

            if (!File.Exists(config.FullName))
            {
                Logger.Error($"File {config.FullName} does not exist.");
                Environment.Exit((int)ExitCode.InvalidArgument);
            }

            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

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

        private static void ExportSchema(FileInfo target)
        {
            JsonSchemaGeneratorSettings settings = new JsonSchemaGeneratorSettings()
            {
                AllowReferencesWithProperties = true,
                FlattenInheritanceHierarchy = true,
                SchemaType = SchemaType.OpenApi3
            };
            settings.SchemaProcessors.Add(new InheritanceSchemaProcessor());

            var schema = JsonSchema.FromType<Configuration>(settings);

            File.WriteAllText(
                target.FullName,
                schema.ToJson()
            );
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

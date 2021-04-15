using System;
using System.IO;
using System.Linq;
using System.Threading;
using AllMyLights.Json;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using AllMyLights.Platforms;
using AllMyLights.Connectors;

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
        /// <param name="info">List dynamic information of your sinks such as available options</param>
        /// <param name="failOnUnknownProperty">Fails if an unknown property is encountered in the provided config file. Can be disabled.</param>
        /// <param name="enableAutostart">Setup autostart for logged in user with current folder as working directory</param>
        static void Main(
            FileInfo config,
            FileInfo exportConfigSchemaTo = null,
            string logLevel = "info",
            string logFile = null,
            bool minimized = false,
            bool info = false,
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

            var configFullName = config?.FullName ?? Path.Combine(Directory.GetCurrentDirectory(), "allmylightsrc.json");
            if (enableAutostart)
            {
                AutostartEntry
                    .GetPlatformInstance()
                    .Create(configFullName, logLevel);
                Environment.Exit(0);
            }

            
            if (!File.Exists(configFullName))
            {
                if(config == null)
                {
                    Logger.Error($"Parameter --{nameof(config)} not provided and default config {configFullName} does not exist.");
                } else
                {
                    Logger.Error($"File {configFullName} does not exist.");
                }
                Environment.Exit((int)ExitCode.MissingArgument);
            }

            using StreamReader file = File.OpenText(configFullName);
            var content = file.ReadToEnd();

            new ConfigurationValidator(new JsonSchemaGeneratorSettings
            {
                AllowReferencesWithProperties = true,
                FlattenInheritanceHierarchy = true,
                AlwaysAllowAdditionalObjectProperties = !failOnUnknownProperty,
            }).Validate(content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);
            var factory = new ConnectorFactory(configuration);
            var sources = factory.GetSources();
            var sinks = factory.GetSinks();

            if (info)
            {
                ShowInfo(sources);
                ShowInfo(sinks);
                Environment.Exit(0);
            }

            var broker = new Broker()
                .RegisterSources(sources)
                .RegisterSinks(sinks)
                .UseRoutes(configuration.Routes.ToArray());

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

        private static void ShowInfo(IConnector[] connectors)
        {
            foreach (IConnector connector in connectors)
            {
                var info = connector.GetInfo();
                if (info != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{connector}:");
                    Console.WriteLine(JsonConvert.SerializeObject(info, Formatting.Indented));
                }
                else
                {
                    Console.WriteLine($"{connector} does not expose any configuration information.");
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

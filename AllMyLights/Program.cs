using System;
using System.IO;
using System.Linq;
using System.Threading;
using AllMyLights.Models;
using MQTTnet;
using Newtonsoft.Json;
using NJsonSchema;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using Unmockable;

#if Windows
using AllMyLights.Platforms.Windows;
using System.Windows.Forms;
#endif

enum ExitCode
{
    InvalidConfig = -1,
    InvalidArgument = -2,
}

namespace AllMyLights
{
    class Program
    {
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static ColorSubject ColorSubject;

        /// <summary>
        /// AllMyLights is a tool to sync colors from a home automation bus to OpenRGB managed peripherals via MQTT
        /// </summary>
        /// <param name="config">Path to the config file that contains the MQTT and OpenRGB settings</param>
        /// <param name="logLevel">Change the log level to either debug, info, warn, error, or off.</param>
        /// <param name="logFile">If provided, log output will additionally be captured in the provided file.</param>
        /// <param name="minimized">Minimize to tray after startup</param>
        /// <param name="listDevices">List all available OpenRGB devices and their zones. Returns right away</param>
        static void Main(
            FileInfo config,
            string logLevel = "warn",
            string logFile = null,
            bool minimized = false,
            bool listDevices = false
        )
        {
            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ConfigureLogging(logLevel, logFile);
            ValidateConfig(fileName: config.Name, content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);

            var openRgbClient = new OpenRGBClient(new OpenRGB.NET.OpenRGBClient(
                ip: configuration.OpenRgb?.Server ?? "127.0.0.1",
                port: configuration.OpenRgb?.Port ?? 6742,
                autoconnect: false
            ).Wrap(), configuration);

            if (listDevices)
            {
                Console.Write(JsonConvert.SerializeObject(openRgbClient.GetDevices(), Formatting.Indented));
                Environment.Exit(0);
            }

            var mqttClient = new MqttFactory().CreateMqttClient();
            ColorSubject = new ColorSubject(configuration, mqttClient);

            

            var broker = new OpenRGBBroker(ColorSubject, openRgbClient);
            broker.Listen();

#if Windows
            var trayIcon = TrayIcon.GetInstance(ColorSubject, minimized);
            ConsoleWindow.Show(!minimized);
            Application.Run();
            trayIcon.Show(false);
#else
            ResetEvent.WaitOne();
#endif
        }



        private static void ValidateConfig(string fileName, string content)
        {
            JsonSchema schema = JsonSchema.FromType<Configuration>();


            var errors = schema.Validate(content);

            if (errors.Count > 0)
            {
                Logger.Error($"Validation issues encountered in config file {fileName}:");
                foreach (var error in errors)
                    Logger.Error($"{error.Path}: {error.Kind}");

                Environment.Exit((int)ExitCode.InvalidConfig);
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

            if(logFile != null)
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

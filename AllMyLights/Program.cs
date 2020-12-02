using System;
using System.Linq;
using System.IO;
using System.Threading;
using AllMyLights.Models;
using MQTTnet;
using Newtonsoft.Json;
using NJsonSchema;
using NLog;
using OpenRGB.NET;
using Unmockable;

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

        /// <summary>
        /// AllMyLights is a tool to sync colors from a home automation bus to OpenRGB managed peripherals via MQTT
        /// </summary>
        /// <param name="config">Path to the config file that contains the MQTT and OpenRGB settings</param>
        /// <param name="logLevel">Change the log level to either debug, info, or warn.</param>
        static void Main(FileInfo config, string logLevel = "warn")
        {

            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ConfigureLogging(logLevel);
            ValidateConfig(fileName: config.Name, content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);

            var mqttClient = new MqttFactory().CreateMqttClient();
            var colorSubject = new ColorSubject(configuration, mqttClient);


            var openRGBClient = new OpenRGBClient(
                ip: configuration.OpenRgb?.Server ?? "127.0.0.1",
                port: configuration.OpenRgb?.Port ?? 6742
            ).Wrap();

            var broker = new OpenRGBBroker(colorSubject, openRGBClient);
            broker.Listen();

            ResetEvent.WaitOne();
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

        private static readonly string[] LogLevels = new string[] { "debug", "info", "warn" };
        private static void ConfigureLogging(string logLevel)
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
                _ => LogLevel.Warn
            };

            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(minLevel, LogLevel.Fatal, logconsole);
            logconsole.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} (${level:uppercase=true}): ${message}";
            LogManager.Configuration = config;
        }
    }
}

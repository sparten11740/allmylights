using System;
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
    INVALID_CONFIG = -1
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
        /// <param name="config">Path to the config file that contains the MQTT & OpenRGB settings</param>
        /// <param name="verbose">Include log statements for DEBUG and INFO log levels</param>
        static void Main(FileInfo config, bool? verbose = false)
        {

            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ConfigureLogging(verbose);
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

                Environment.Exit((int)ExitCode.INVALID_CONFIG);
            }
        }

        private static void ConfigureLogging(bool? verbose)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(verbose == true ? LogLevel.Debug : LogLevel.Warn, LogLevel.Fatal, logconsole);
            logconsole.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} (${level:uppercase=true}): ${message}";
            LogManager.Configuration = config;
        }
    }
}

using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Threading;
using AllMyLights.Models;
using MQTTnet;
using Newtonsoft.Json;
using NJsonSchema;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using System.Windows.Forms;
using System.Runtime.InteropServices;

enum ExitCode
{
    InvalidConfig = -1,
    InvalidArgument = -2,
}

namespace AllMyLights
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static ColorSubject ColorSubject;

        /// <summary>
        /// AllMyLights is a tool to sync colors from a home automation bus to OpenRGB managed peripherals via MQTT
        /// </summary>
        /// <param name="config">Path to the config file that contains the MQTT and OpenRGB settings</param>
        /// <param name="logLevel">Change the log level to either debug, info, warn, error, or off.</param>
        static void Main(FileInfo config, string logLevel = "warn")
        {
            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ConfigureLogging(logLevel);
            ValidateConfig(fileName: config.Name, content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);

            var mqttClient = new MqttFactory().CreateMqttClient();
            ColorSubject = new ColorSubject(configuration, mqttClient);


            OpenRGBClientFactory.GetInstance(configuration).Subscribe((openRgbClient) =>
            {
                var broker = new OpenRGBBroker(ColorSubject, openRgbClient);
                broker.Listen();
            });

            CreateTrayIcon();
            SetConsoleWindowVisibility(false);
            
            Application.Run();

            TrayIcon.Visible = false;
        }


        private static NotifyIcon TrayIcon;
        private static readonly ToolStripLabel TrayColorLabel = new ToolStripLabel();
        static void CreateTrayIcon()
        {

            var menu = new ContextMenuStrip();
            var exitButton = new ToolStripButton("Exit");

            menu.Items.Add(TrayColorLabel);
            menu.Items.Add(exitButton);

            TrayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "AllMyLights",
                ContextMenuStrip = menu,
                Visible = true
            };

            bool Visible = false;

            TrayIcon.MouseDoubleClick += (sender, e) =>
            {
                Visible = !Visible;
                SetConsoleWindowVisibility(Visible);
            };

            ColorSubject.Updates().Subscribe((color) =>
            {
                TrayColorLabel.Text = color.ToString();
            });

            exitButton.Click += (sender, e) => {
                Environment.Exit(0);
            };
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);

            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1);          
                else ShowWindow(hWnd, 0);             
            }
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
                "error" => LogLevel.Error,
                "off" => LogLevel.Off,
                _ => LogLevel.Warn
            };

            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new ColoredConsoleTarget("logconsole");
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                condition: ConditionParser.ParseExpression("(level == LogLevel.Error)"),
                foregroundColor: ConsoleOutputColor.White,
                backgroundColor: ConsoleOutputColor.DarkRed
            ));
            config.AddRule(minLevel, LogLevel.Fatal, logconsole);
            logconsole.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} (${level:uppercase=true}): ${message}";
            LogManager.Configuration = config;
        }
    }
}

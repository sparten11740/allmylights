using System;
using System.IO;
using System.Threading;
using AllMyLights.Models;
using MQTTnet;
using Newtonsoft.Json;
using NJsonSchema;

enum ExitCode
{
    INVALID_CONFIG = -1
}

namespace AllMyLights
{
    class Program
    {
        static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        static void Main(FileInfo config)
        {
            using StreamReader file = File.OpenText(config.FullName);
            var content = file.ReadToEnd();

            ValidateConfig(fileName: config.Name, content);

            var configuration = JsonConvert.DeserializeObject<Configuration>(content);
            var mqttClient = new MqttFactory().CreateMqttClient();

            new ColorSubject(configuration, mqttClient)
                .Updates()
                .Subscribe((it) => {
                    Console.WriteLine(it.ToString());
                });

           
            ResetEvent.WaitOne(); 
        }

        private static void ValidateConfig(string fileName, string content)
        {
            JsonSchema schema = JsonSchema.FromType<Configuration>();
            var errors = schema.Validate(content);

            if (errors.Count > 0)
            {
                Console.WriteLine($"Validation issues encountered in config file {fileName}:");
                foreach (var error in errors)
                    Console.WriteLine(error.Path + ": " + error.Kind);

                Environment.Exit((int)ExitCode.INVALID_CONFIG);
            }
        }
    }
}

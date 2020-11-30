using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AllMyLights.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace AllMyLights
{
    public class ColorSubject
    {
        private readonly ReplaySubject<Color> Subject = new ReplaySubject<Color>(1);
        private Configuration Configuration { get; }
        private IMqttClient MqttClient { get; }
        private IMqttClientOptions MqttClientOptions { get; }

        public ColorSubject(Configuration configuration, IMqttClient mqttClient)
        {
            Configuration = configuration;
            MqttClient = mqttClient;
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(Configuration.MQTT.Server, Configuration.MQTT.Port);


            if (Configuration.MQTT.Password != null && Configuration.MQTT.Username != null)
            {
                builder = builder.WithCredentials(Configuration.MQTT.Username, Configuration.MQTT.Password);
            }

            MqttClientOptions = builder.Build();

            Initialize();
        }


        private void HandleMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            JObject o = JObject.Parse(Encoding.UTF8.GetString(args.ApplicationMessage.Payload));
            var color = o.SelectToken(Configuration.MQTT.Topics.Result.ValuePath).ToString();

            Subject.OnNext(ColorConverter.Decode(color));
        }

        private async Task HandleConnected(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine($"Connection to mqtt server {Configuration.MQTT.Server} established");
            Console.WriteLine($"Attempting to subscribe to {Configuration.MQTT.Topics.Result.Path}");

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder().WithTopic(Configuration.MQTT.Topics.Result.Path).Build();
            await MqttClient.SubscribeAsync(topicFilter);

            Console.WriteLine($"Succesfully subscribed to {Configuration.MQTT.Topics.Result.Path}");
        }

        private async Task HandleDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Console.WriteLine("Connection to mqtt server was lost. Reconnecting in 5s ...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine($"Attemtping to reonnect to {Configuration.MQTT.Server}");

            try
            {
                await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Reconnecting failed. Connection couldn't be established: {e.Message}");
            }
        }

        private async void Initialize()
        {
            MqttClient.UseDisconnectedHandler(HandleDisconnected);
            MqttClient.UseConnectedHandler(HandleConnected);
            MqttClient.UseApplicationMessageReceivedHandler(HandleMessage);

            await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
        }

        public IObservable<Color> Updates()
        {
            return Subject.AsObservable();
        }
    }
}

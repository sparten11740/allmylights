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
using NLog;

namespace AllMyLights
{
    public class ColorSubject : IColorSubject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ReplaySubject<Color> Subject = new ReplaySubject<Color>(1);
        private Configuration Configuration { get; }
        private IMqttClient MqttClient { get; }
        private IMqttClientOptions MqttClientOptions { get; }

        public ColorSubject(Configuration configuration, IMqttClient mqttClient)
        {
            Configuration = configuration;
            MqttClient = mqttClient;
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(Configuration.Mqtt.Server, Configuration.Mqtt.Port);


            if (Configuration.Mqtt.Password != null && Configuration.Mqtt.Username != null)
            {
                builder = builder.WithCredentials(Configuration.Mqtt.Username, Configuration.Mqtt.Password);
            }

            MqttClientOptions = builder.Build();

            Initialize();
        }

        private async void Initialize()
        {
            MqttClient.UseDisconnectedHandler(HandleDisconnected);
            MqttClient.UseConnectedHandler(HandleConnected);
            MqttClient.UseApplicationMessageReceivedHandler(HandleMessage);

            await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
            await MqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic = Configuration.Mqtt.Topics.Command
            }, CancellationToken.None);

        }

        private void HandleMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            Logger.Debug($"Received payload {payload}");
            Logger.Debug($"Extracting color with JsonPath expression {Configuration.Mqtt.Topics.Result.ValuePath}");

            JObject o = JObject.Parse(payload);
            var color = o.SelectToken(Configuration.Mqtt.Topics.Result.ValuePath);

            if (color != null)
            {
                Subject.OnNext(ColorConverter.Decode(color.ToString(), Configuration.Mqtt.Topics.Result.ChannelLayout));
            }
        }

        private async Task HandleConnected(MqttClientConnectedEventArgs e)
        {
            Logger.Info($"Connection to mqtt server {Configuration.Mqtt.Server} established");
            Logger.Info($"Attempting to subscribe to {Configuration.Mqtt.Topics.Result.Path}");

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder().WithTopic(Configuration.Mqtt.Topics.Result.Path).Build();
            await MqttClient.SubscribeAsync(topicFilter);

            Logger.Info($"Succesfully subscribed to {Configuration.Mqtt.Topics.Result.Path}");
        }

        private async Task HandleDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Logger.Info("Connection to mqtt server was lost. Reconnecting in 5s ...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Logger.Info($"Attemtping to reonnect to {Configuration.Mqtt.Server}");

            try
            {
                await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Error($"Reconnecting failed. Connection couldn't be established: {e.Message}");
            }
        }

        public IObservable<Color> Get()
        {
            return Subject.AsObservable();
        }
    }
}

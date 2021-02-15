using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using NLog;

namespace AllMyLights.Connectors.Sources.Mqtt
{
    public class MqttSource : Source
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ReplaySubject<string> Subject = new ReplaySubject<string>(1);
        private IMqttClient MqttClient { get; }
        private IMqttClientOptions MqttClientOptions { get; }
        private MqttSourceOptions Options { get; }

        protected override IObservable<object> Value { get; }


        public MqttSource(MqttSourceOptions options, IMqttClient mqttClient): base(options)
        {
            Value = Subject.AsObservable();

            Options = options;
            MqttClient = mqttClient;
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(options.Server, options.Port);


            if (options.Password != null && options.Username != null)
            {
                builder = builder.WithCredentials(options.Username, options.Password);
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


            if(Options.Topics.Command != null)
            {
                await MqttClient.PublishAsync(new MqttApplicationMessage
                {
                    Topic = Options.Topics.Command
                }, CancellationToken.None);
            }
        }

        private void HandleMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            Logger.Debug($"Received payload {payload}");
            Subject.OnNext(payload);
        }

        private async Task HandleConnected(MqttClientConnectedEventArgs e)
        {
            Logger.Info($"Connection to mqtt server {Options.Server} established");
            Logger.Info($"Attempting to subscribe to {Options.Topics.Result}");

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder().WithTopic(Options.Topics.Result).Build();
            await MqttClient.SubscribeAsync(topicFilter);

            Logger.Info($"Succesfully subscribed to {Options.Topics.Result}");
        }

        private async Task HandleDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Logger.Info("Connection to mqtt server was lost. Reconnecting in 5s ...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Logger.Info($"Attemtping to reonnect to {Options.Server}");

            try
            {
                await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Error($"Reconnecting failed. Connection couldn't be established: {e.Message}");
            }
        }

        public override string ToString() => $"{nameof(MqttSource)}({Options.Server}:{Options.Port})";
    }
}

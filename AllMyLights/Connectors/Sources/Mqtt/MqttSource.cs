using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NLog;

namespace AllMyLights.Connectors.Sources.Mqtt
{
    public class MqttSource : Source
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly int RECONNECT_DELAY = 10;

        private readonly ReplaySubject<string> Subject = new(1);
        private IManagedMqttClient MqttClient { get; }
        
        private MqttSourceOptions Options { get; }
        private IManagedMqttClientOptions MqttClientOptions { get; }

        protected override IObservable<object> Value { get; }


        public MqttSource(MqttSourceOptions options, IManagedMqttClient mqttClient): base(options)
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

            var clientOptions = builder.Build();
            MqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithPendingMessagesOverflowStrategy(MQTTnet.Server.MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage)
                .WithClientOptions(clientOptions)
                .WithMaxPendingMessages(1).WithAutoReconnectDelay(TimeSpan.FromSeconds(RECONNECT_DELAY)).Build();



            Initialize().Wait();
        }

        private async Task Initialize()
        {
            MqttClient.UseDisconnectedHandler(HandleDisconnected);
            MqttClient.UseConnectedHandler(HandleConnected);
            MqttClient.UseApplicationMessageReceivedHandler(HandleMessage);

            await MqttClient.StartAsync(MqttClientOptions);


            if (Options.Topics.Command != null)
            {
                await MqttClient.PublishAsync(new ManagedMqttApplicationMessage
                {
                    ApplicationMessage = new MqttApplicationMessageBuilder().WithTopic(Options.Topics.Command).Build()
                });
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
            Logger.Info($"Connection to MQTT server (source) {Options.Server} established");
            Logger.Info($"Attempting to subscribe to {Options.Topics.Result}");

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder().WithTopic(Options.Topics.Result).Build();
            await MqttClient.SubscribeAsync(topicFilter);

            Logger.Info($"Succesfully subscribed to {Options.Topics.Result}");
        }

        private void HandleDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Logger.Info($"Connection to mqtt server was lost. Reconnecting in {RECONNECT_DELAY}s ...");
        }

        public override string ToString() => $"{nameof(MqttSource)}({Options.Server}:{Options.Port})";
    }
}

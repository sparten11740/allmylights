using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NLog;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class MqttSink : Sink
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly int RECONNECT_DELAY = 10;

        private IManagedMqttClient MqttClient { get; }
        private List<string> Topics { get; }

        private IManagedMqttClientOptions MqttClientOptions { get; }
        private MqttSinkOptions Options { get; }

        public MqttSink(MqttSinkOptions options, IManagedMqttClient client) : base(options)
        {
            MqttClient = client;
            Topics = options.Topics.ToList();
            Options = options;

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
                .WithMaxPendingMessages(1).WithAutoReconnectDelay(TimeSpan.FromSeconds(10)).Build();

            MqttClient.UseDisconnectedHandler(HandleDisconnected);
            MqttClient.UseConnectedHandler(HandleConnected);

            client.StartAsync(MqttClientOptions).Wait();


            Next.Subscribe(payload =>
            {
                Publish(payload).Wait();
            });
        }

        private async Task Publish(object payload)
        {
            var tasks = Topics.Select(async topic =>
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload.ToString())
                    .Build();

                await MqttClient.PublishAsync(new ManagedMqttApplicationMessage()
                {
                    ApplicationMessage = message
                });
            });
            await Task.WhenAll(tasks);
        }

        private void HandleConnected(MqttClientConnectedEventArgs e)
        {
            Logger.Info($"Connection to MQTT server (sink) {Options.Server} established");
        }

        private void HandleDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Logger.Info($"Connection to mqtt server was lost. Reconnecting in {RECONNECT_DELAY}s ...");
        }

        public override string ToString() => $"{nameof(MqttSink)}({(Id != null ? $"#{Id} " : "")}{Options.Server}:{Options.Port})";
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using AllMyLights.Connectors.Sinks.Chroma;
using Microsoft.Reactive.Testing;
using Moq;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class MqttSinkTest: ReactiveTest
    {
        MqttSinkOptions Options;

        public IManagedMqttClientOptions MqttClientOptions { get; private set; }
        public MqttClientTcpOptions MqttClientTcpOptions { get; private set; }

        readonly Mock<IManagedMqttClient> MqttClientMock = new();

        readonly string[] Topics = new string[] { "wayne/forum", "wayne/tower" };


        [SetUp]
        public void Setup()
        {
            Options = new MqttSinkOptions
            {
                Server = "wayne-foundation.com",
                Port = 1863,
                Password = "bruce-admires-robin",
                Username = "bwayne",
                Topics = Topics
            };

            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(Options.Server, Options.Port)
                .WithCredentials(Options.Username, Options.Password);


            MqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(clientOptions)
                .Build();

            MqttClientTcpOptions = MqttClientOptions.ClientOptions.ChannelOptions as MqttClientTcpOptions;
        }

        [Test]
        public void Should_initialize_MQTTClient()
        {
            var args = new List<IManagedMqttClientOptions>();
            MqttClientMock.Setup(m => m.StartAsync(Capture.In(args)));

            var subject = new MqttSink(Options, MqttClientMock.Object);
            var actualOptions = args.First();
            MqttClientTcpOptions actualChannelOptions = actualOptions.ClientOptions.ChannelOptions as MqttClientTcpOptions;

            MqttClientMock.Verify(it => it.StartAsync(It.IsAny<IManagedMqttClientOptions>()));

            Assert.AreEqual(actualOptions.ClientOptions.Credentials.Username, MqttClientOptions.ClientOptions.Credentials.Username);
            Assert.AreEqual(actualOptions.ClientOptions.Credentials.Password, MqttClientOptions.ClientOptions.Credentials.Password);
            Assert.AreEqual(actualChannelOptions.Server, MqttClientTcpOptions.Server);
            Assert.AreEqual(actualChannelOptions.Port, MqttClientTcpOptions.Port);
        }

        [Test]
        public void Should_publish_value_to_all_configured_topics()
        {
            var args = new List<ManagedMqttApplicationMessage>();
            MqttClientMock.Setup(it => it.PublishAsync(Capture.In(args)));

            var subject = new MqttSink(Options, MqttClientMock.Object);


            var value = "Batman";
            subject.Consume(value);

            Assert.AreEqual(2, args.Count());

            var first = args[0];
            var second = args[1];
            Assert.AreEqual(Topics[0], first.ApplicationMessage.Topic);
            Assert.AreEqual(value, first.ApplicationMessage.Payload);
            Assert.AreEqual(Topics[1], second.ApplicationMessage.Topic);
            Assert.AreEqual(value, second.ApplicationMessage.Payload);
        }
    }
}
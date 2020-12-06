using System;
using System.Text;
using System.Threading;
using AllMyLights.Models;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NUnit.Framework;
using Microsoft.Reactive.Testing;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Disconnecting;
using AllMyLights.Connectors.Sources;
using AllMyLights.Models.Mqtt;

namespace AllMyLights.Test
{
    public class MqttSourceTest : ReactiveTest
    {
        MqttSourceParams Options;

        public IMqttClientOptions MqttClientOptions { get; private set; }
        public MqttClientTcpOptions MqttClientTcpOptions { get; private set; }

        readonly Mock<IMqttClient> MqttClientMock = new Mock<IMqttClient>();

        [SetUp]
        public void Setup()
        {
            Options = new MqttSourceParams
            {
                Server = "wayne-foundation.com",
                Port = 1863,
                Password = "bruce-admires-robin",
                Username = "bwayne",
                Topics = new Topics
                {
                    Command = "cmnd/tasmota-dimmer/color",
                    Result = new Topic
                    {
                        Path = "stat/sonoff-1144-dimmer-5/RESULT",
                        ValuePath = "$.Color"
                    }
                }
            };

            MqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(Options.Server, Options.Port)
                .WithCredentials(Options.Username, Options.Password)
                .Build();

            MqttClientTcpOptions = MqttClientOptions.ChannelOptions as MqttClientTcpOptions;
        }

        [Test]
        public void Should_initialize_MQTTClient()
        {
            var args = new List<MqttClientOptions>();
            MqttClientMock.Setup(m => m.ConnectAsync(Capture.In(args), CancellationToken.None));

            var subject = new MqttSource(Options, MqttClientMock.Object);
            var actualOptions = args.First();
            MqttClientTcpOptions actualChannelOptions = actualOptions.ChannelOptions as MqttClientTcpOptions;

            MqttClientMock.Verify(it => it.ConnectAsync(It.IsAny<MqttClientOptions>(), CancellationToken.None));

            Assert.AreEqual(actualOptions.Credentials.Username, MqttClientOptions.Credentials.Username);
            Assert.AreEqual(actualOptions.Credentials.Password, MqttClientOptions.Credentials.Password);
            Assert.AreEqual(actualChannelOptions.Server, MqttClientTcpOptions.Server);
            Assert.AreEqual(actualChannelOptions.Port, MqttClientTcpOptions.Port);
        }

        [Test]
        public void Should_request_color_via_command_topic()
        {
            var args = new List<MqttApplicationMessage>();
            MqttClientMock.Setup(it => it.PublishAsync(Capture.In(args), CancellationToken.None));

            var subject = new MqttSource(Options, MqttClientMock.Object);

            MqttClientMock.Verify(it => it.PublishAsync(It.IsAny<MqttApplicationMessage>(), CancellationToken.None));
            Assert.AreEqual(Options.Topics.Command, args.First().Topic);
        }

        [Test]
        public void Should_subscribe_to_provided_topic()
        {
            MqttClientMock.SetupAllProperties();
            var subject = new MqttSource(Options, MqttClientMock.Object);

            var args = new List<MqttClientSubscribeOptions>();
            MqttClientMock.Setup(it => it.SubscribeAsync(Capture.In(args), CancellationToken.None));
            MqttClientMock.Object.ConnectedHandler.HandleConnectedAsync(new MqttClientConnectedEventArgs(new Mock<MqttClientAuthenticateResult>().Object));
            var filter = args.First().TopicFilters.First();

            MqttClientMock.Verify(it => it.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), CancellationToken.None));
            Assert.AreEqual(Options.Topics.Result.Path, filter.Topic);
        }

        [Test]
        public void Should_reconnect_after_disconnecting()
        {
            MqttClientMock.SetupAllProperties();

            var args = new List<MqttClientOptions>();
            MqttClientMock.Setup(it => it.ConnectAsync(Capture.In(args), CancellationToken.None));

            var subject = new MqttSource(Options, MqttClientMock.Object);
            MqttClientMock.Object.DisconnectedHandler.HandleDisconnectedAsync(new MqttClientDisconnectedEventArgs(
                true,
                new Exception("Networ error"),
                new Mock<MqttClientAuthenticateResult>().Object,
                MqttClientDisconnectReason.DisconnectWithWillMessage
            ));

            var actualOptions = args.First();
            MqttClientTcpOptions actualChannelOptions = actualOptions.ChannelOptions as MqttClientTcpOptions;


            MqttClientMock.Verify(it => it.ConnectAsync(It.IsAny<MqttClientOptions>(), CancellationToken.None));

            Assert.AreEqual(actualOptions.Credentials.Username, MqttClientOptions.Credentials.Username);
            Assert.AreEqual(actualOptions.Credentials.Password, MqttClientOptions.Credentials.Password);
            Assert.AreEqual(actualChannelOptions.Server, MqttClientTcpOptions.Server);
            Assert.AreEqual(actualChannelOptions.Port, MqttClientTcpOptions.Port);
        }

        [Test]
        public void Should_consume_message_and_emit_color()
        {
            var black = "#000000";
            Color expectedColor = Color.FromArgb(255, 0, 0, 0);

            var message = new MqttApplicationMessage { Payload = Encoding.UTF8.GetBytes($"{{ \"Color\": \"{black}\" }}") };
            var eventArgs = new MqttApplicationMessageReceivedEventArgs("", message);
            var scheduler = new TestScheduler();

            MqttClientMock.SetupAllProperties();

            var subject = new MqttSource(Options, MqttClientMock.Object);

            scheduler.Schedule(TimeSpan.FromTicks(20), () =>
            {
                MqttClientMock.Object.ApplicationMessageReceivedHandler.HandleApplicationMessageReceivedAsync(eventArgs);
            });

            var actual = scheduler.Start(
              () => subject.Get(),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<Color>>[] {
                OnNext(20, expectedColor)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
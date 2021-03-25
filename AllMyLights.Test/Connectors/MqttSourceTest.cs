using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using AllMyLights.Common;
using AllMyLights.Connectors.Sources.Mqtt;
using AllMyLights.Transformations;
using AllMyLights.Transformations.Color;
using AllMyLights.Transformations.JsonPath;
using Microsoft.Reactive.Testing;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class MqttSourceTest: ReactiveTest
    {
        MqttSourceOptions Options;

        public IManagedMqttClientOptions MqttClientOptions { get; private set; }
        public MqttClientTcpOptions MqttClientTcpOptions { get; private set; }

        readonly Mock<IManagedMqttClient> MqttClientMock = new Mock<IManagedMqttClient>();

        [SetUp]
        public void Setup()
        {
            Options = new MqttSourceOptions
            {
                Server = "wayne-foundation.com",
                Port = 1863,
                Password = "bruce-admires-robin",
                Username = "bwayne",
                Topics = new Topics
                {
                    Command = "cmnd/tasmota-dimmer/color",
                    Result = "stat/sonoff-1144-dimmer-5/RESULT",
                }
            };


            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(Options.Server, Options.Port)
                .WithCredentials(Options.Username, Options.Password).Build();
            
            MqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(clientOptions)
                .Build();

            MqttClientTcpOptions = clientOptions.ChannelOptions as MqttClientTcpOptions;
        }

        [Test]
        public void Should_initialize_MQTTClient()
        {
            var args = new List<IManagedMqttClientOptions>();
            MqttClientMock.Setup(m => m.StartAsync(Capture.In(args)));

            var subject = new MqttSource(Options, MqttClientMock.Object);
            var actualOptions = args.First();
            MqttClientTcpOptions actualChannelOptions = actualOptions.ClientOptions.ChannelOptions as MqttClientTcpOptions;

            MqttClientMock.Verify(it => it.StartAsync(It.IsAny<IManagedMqttClientOptions>()));

            Assert.AreEqual(actualOptions.ClientOptions.Credentials.Username, MqttClientOptions.ClientOptions.Credentials.Username);
            Assert.AreEqual(actualOptions.ClientOptions.Credentials.Password, MqttClientOptions.ClientOptions.Credentials.Password);
            Assert.AreEqual(actualChannelOptions.Server, MqttClientTcpOptions.Server);
            Assert.AreEqual(actualChannelOptions.Port, MqttClientTcpOptions.Port);
        }

        [Test]
        public void Should_request_value_via_command_topic()
        {
            var args = new List<ManagedMqttApplicationMessage>();
            MqttClientMock.Setup(it => it.PublishAsync(Capture.In(args)));

            var subject = new MqttSource(Options, MqttClientMock.Object);

            MqttClientMock.Verify(it => it.PublishAsync(It.IsAny<ManagedMqttApplicationMessage>()));
            Assert.AreEqual(Options.Topics.Command, args.First().ApplicationMessage.Topic);
        }

        [Test]
        public void Should_subscribe_to_provided_topic()
        {
            MqttClientMock.SetupAllProperties();
            var subject = new MqttSource(Options, MqttClientMock.Object);

            MqttClientMock.Object.ConnectedHandler.HandleConnectedAsync(new MqttClientConnectedEventArgs(new Mock<MqttClientAuthenticateResult>().Object));

            MqttClientMock.Verify(it => it.SubscribeAsync(It.Is<IEnumerable<MqttTopicFilter>>((topics) => Options.Topics.Result == topics.First().Topic)));
        }


        [Test]
        public void Should_consume_message_and_emit_payload()
        {
            var black = "#000000";

            string payload = $"{{ \"Color\": \"{black}\" }}";
            var message = new MqttApplicationMessage { Payload = Encoding.UTF8.GetBytes(payload) };
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

            var expected = new Recorded<Notification<object>>[] {
                OnNext(20, (object)payload)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }


        [Test]
        public void Should_consume_message_and_apply_transformations()
        {
            var red = "#ff0000";
            Color expectedColor = Color.FromArgb(255, 0, 255, 0);

            string payload = $"{{ \"Color\": \"{red}\" }}";
            var message = new MqttApplicationMessage { Payload = Encoding.UTF8.GetBytes(payload) };
            var eventArgs = new MqttApplicationMessageReceivedEventArgs("", message);
            var scheduler = new TestScheduler();

            MqttClientMock.SetupAllProperties();

            Options.Transformations = new List<TransformationOptions>()
            {
                new JsonPathTransformationOptions() { Expression = "$.Color" },
                new ColorTransformationOptions() { ChannelLayout = "GRB" },
            }.ToArray();

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

            var expected = new Recorded<Notification<object>>[] {
                OnNext(20, (object)new Ref<Color>(expectedColor))
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
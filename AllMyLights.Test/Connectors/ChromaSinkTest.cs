using System;
using Moq;
using NUnit.Framework;
using Microsoft.Reactive.Testing;
using Microsoft.Reactive.Testing;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Connectors.Sinks.Chroma;
using System.Drawing;
using AllMyLights.Common;
using System.Linq;
using Newtonsoft.Json;

namespace AllMyLights.Test
{
    public class ChromaSinkTest: ReactiveTest
    {


        [Test]
        public void Should_initialize_and_send_heartbeats()
        {
            var clientMock = new Mock<IChromaClient>();
            var options = new ChromaSinkOptions() { };

            clientMock.Setup(it => it.InitializeAsync());
            clientMock.Setup(it => it.SendHeartbeatAsync());


            var scheduler = new TestScheduler();
            var heartbeatInterval = scheduler.CreateColdObservable(
                new Recorded<Notification<long>>(10, Notification.CreateOnNext(1L)),
                new Recorded<Notification<long>>(20, Notification.CreateOnNext(2L))
            );

            var sink = new ChromaSink(options, clientMock.Object, heartbeatInterval);
            scheduler.Start();

            clientMock.Verify((it) => it.InitializeAsync());
            clientMock.Verify((it) => it.SendHeartbeatAsync(), Times.Exactly(2));
        }

        [Test]
        public void Should_apply_static_effect_to_supported_devices()
        {
            var supportedDevices = new string[] {
                "keyboard",
                "mouse",
                "headset",
                "mousepad",
                "keypad",
                "chromalink"
            };
            var clientMock = new Mock<IChromaClient>();
            var options = new ChromaSinkOptions() { SupportedDevices = supportedDevices };

            clientMock.Setup(it => it.InitializeAsync());
            clientMock.Setup(it => it.ApplyStaticEffectAsync(It.IsAny<string>(), It.IsAny<Color>()));

            var color = new Ref<Color>(Color.AliceBlue);
            var sink = new ChromaSink(options, clientMock.Object, Observable.Empty<long>());
            sink.Consume(color);

            clientMock.Verify((it) => it.InitializeAsync());
            supportedDevices.ToList().ForEach((device) =>
            {
                clientMock.Verify((it) => it.ApplyStaticEffectAsync(device, color.Value));
            });
        }

        [Test]
        public void Should_apply_custom_effect()
        {
            var supportedDevices = new string[] {
                "mouse",
                "keyboard"
            };
            var clientMock = new Mock<IChromaClient>();
            var options = new ChromaSinkOptions() { SupportedDevices = supportedDevices };

            clientMock.Setup(it => it.InitializeAsync());
            clientMock.Setup(it => it.ApplyStaticEffectAsync(It.IsAny<string>(), It.IsAny<Color>()));

            var payload = JsonConvert.SerializeObject(new ChromaEffect()
            {
                Effect = "CHROMA_CUSTOM2",
                Param = new long[][]
                {
                    new long[] { 255, 255, 255, 255, 255, 255, 255 },
                    new long[] { 65280,65280,65280,65280,65280,65280,65280 },
                }
            });
            var sink = new ChromaSink(options, clientMock.Object, Observable.Empty<long>());
            sink.Consume(payload);

            clientMock.Verify((it) => it.InitializeAsync());
            clientMock.Verify((it) => it.UpdateAsync("mouse", payload));
            clientMock.Verify((it) => it.UpdateAsync("keyboard", payload));
        }

        [Test]
        public void Should_not_call_client_with_invalid_json()
        {
            var supportedDevices = new string[] {
                "mouse"
            };
            var clientMock = new Mock<IChromaClient>();
            var options = new ChromaSinkOptions() { SupportedDevices = supportedDevices };

            clientMock.Setup(it => it.InitializeAsync());
            clientMock.Setup(it => it.ApplyStaticEffectAsync(It.IsAny<string>(), It.IsAny<Color>()));

            var invalidJson = @"
                {
                    ""effect"": ""CHROMA_CUSTOM2""
                    ""param"": [
                        [ 255,255,255,255,255,255,255 ],
                        [ 65280,65280,65280,65280,65280,65280,65280 ]
                    ]
            ";
            var sink = new ChromaSink(options, clientMock.Object, Observable.Empty<long>());
            sink.Consume(invalidJson);

            clientMock.Verify((it) => it.InitializeAsync());
            clientMock.Verify((it) => it.UpdateAsync("mouse", It.IsAny<string>()), Times.Never);
        }
    }
}
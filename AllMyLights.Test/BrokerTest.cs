using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class BrokerTest : ReactiveTest
    {
        Mock<ISource> MqttSource = new Mock<ISource>();
        Mock<ISource> SSESource = new Mock<ISource>();
        
        Mock<ISink> OpenRGBSink = new Mock<ISink>();
        Mock<ISink> WallpaperSink = new Mock<ISink>();

        [Test]
        public void Should_invoke_sinks_with_colors_emitted_from_sources()
        {
            var red = System.Drawing.Color.FromName("red") as object;
            var black = System.Drawing.Color.FromName("black") as object;
            var green = System.Drawing.Color.FromName("green") as object;

            var broker = new Broker()
                .RegisterSources(MqttSource.Object, SSESource.Object)
                .RegisterSinks(OpenRGBSink.Object, WallpaperSink.Object);

            var scheduler = new TestScheduler();

            MqttSource.Setup(it => it.Get()).Returns(() =>
            {
                return scheduler.CreateHotObservable(OnNext(150, red));
            });

            SSESource.Setup(it => it.Get()).Returns(() =>
            {
                return scheduler.CreateHotObservable(OnNext(100, black), OnNext(300, green));
            });

            broker.Listen();

            scheduler.AdvanceBy(200);

            OpenRGBSink.Verify((it) => it.Consume(black));
            OpenRGBSink.Verify((it) => it.Consume(red));

            WallpaperSink.Verify((it) => it.Consume(black));
            WallpaperSink.Verify((it) => it.Consume(red));
        }

        [Test]
        public void Should_emit_values_along_routes()
        {
            var viaRoute1 = "via-route-1";
            var viaRoute2 = "via-route-2";

            var scheduler = new TestScheduler();

            MqttSource.SetupGet(it => it.Id).Returns("source-1");
            SSESource.SetupGet(it => it.Id).Returns("source-2");

            OpenRGBSink.SetupGet(it => it.Id).Returns("sink-1");
            WallpaperSink.SetupGet(it => it.Id).Returns("sink-2");

            var routes = new RouteOptions[]
            {
                new RouteOptions
                {
                    From = "source-1",
                    To = new string[]
                    {
                        "sink-1",
                        "sink-2"
                    }
                },
                new RouteOptions
                {
                    From = "source-2",
                    To = new string[]
                    {
                        "sink-1",
                    }
                }
            };

            var broker = new Broker()
               .RegisterSources(MqttSource.Object, SSESource.Object)
               .RegisterSinks(OpenRGBSink.Object, WallpaperSink.Object)
               .UseRoutes(routes);


            MqttSource.Setup(it => it.Get()).Returns(() =>
            {
                return scheduler.CreateHotObservable(OnNext(150, viaRoute1));
            });

            SSESource.Setup(it => it.Get()).Returns(() =>
            {
                return scheduler.CreateHotObservable(OnNext(100, viaRoute2));
            });


            broker.Listen();

            scheduler.AdvanceBy(200);

            OpenRGBSink.Verify((it) => it.Consume(viaRoute1), Times.Once());
            OpenRGBSink.Verify((it) => it.Consume(viaRoute2), Times.Once());

            WallpaperSink.Verify((it) => it.Consume(viaRoute1), Times.Once());
            WallpaperSink.Verify((it) => it.Consume(viaRoute2), Times.Never());
        }
    }
}
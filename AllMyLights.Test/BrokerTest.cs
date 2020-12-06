using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using AllMyLights.Models;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using Unmockable;

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
            var red = System.Drawing.Color.FromName("red");
            var black = System.Drawing.Color.FromName("black");
            var green = System.Drawing.Color.FromName("green");

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
    }
}
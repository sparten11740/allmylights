using AllMyLights.Models;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using Unmockable;

namespace AllMyLights.Test
{
    public class OpenRGBBrokerTest : ReactiveTest
    {
        Mock<IColorSubject> ColorSubjectMock = new Mock<IColorSubject>();
        Mock<IOpenRGBClient> OpenRGBClientMock = new Mock<IOpenRGBClient>();

        [Test]
        public void Should_invoken_OpenRGB_with_color_from_MQTT_message()
        {
            var targetColor = System.Drawing.Color.FromName("red");
            
            var scheduler = new TestScheduler();
            ColorSubjectMock.Setup(it => it.Updates()).Returns(() =>
            {
                return scheduler.CreateHotObservable(
                        OnNext(150, targetColor)
                    );
            });

            var synchronization = new OpenRGBBroker(
                ColorSubjectMock.Object,
                OpenRGBClientMock.Object
            );
            synchronization.Listen();


            scheduler.AdvanceBy(200);

            OpenRGBClientMock.Verify((it) => it.UpdateAll(targetColor));
        }
    }
}
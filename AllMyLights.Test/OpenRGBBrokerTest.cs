using AllMyLights.Models;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using Unmockable;

namespace AllMyLights.Test
{
    public class OpenRGBSynchronizationTest : ReactiveTest
    {
        Configuration Config;
        Mock<IColorSubject> ColorSubjectMock = new Mock<IColorSubject>();

        [Test]
        public void Should_invoken_OpenRGB_with_color_from_MQTT_message()
        {
            var targetColor = System.Drawing.Color.FromName("red");
            var expectedColor = targetColor.ToOpenRGBColor();

            var razerMouse = new Device();
            var corsairH150i = new Device();

            razerMouse.SetColors(new Color(), new Color());
            corsairH150i.SetColors(new Color());

            var openRGBClientMock = Interceptor.For<OpenRGBClient>()
                .Setup(it => it.GetControllerCount())
                .Returns(2)
                .Setup(it => it.GetAllControllerData())
                .Returns(new Device[] { razerMouse, corsairH150i })
                .Setup(it => it.UpdateLeds(
                    0,
                    Arg.Where<Color[]>(them => them.ContainsExactly(expectedColor, expectedColor))
                ))
                .Setup(it => it.UpdateLeds(
                    1,
                    Arg.Where<Color[]>(them => them.ContainsExactly(expectedColor))
                ));

            var scheduler = new TestScheduler();
            ColorSubjectMock.Setup(it => it.Updates()).Returns(() =>
            {
                return scheduler.CreateHotObservable(
                        OnNext(150, targetColor)
                    );
            });

            var synchronization = new OpenRGBBroker(
                ColorSubjectMock.Object,
                openRGBClientMock
            );
            synchronization.Listen();


            scheduler.AdvanceBy(200);

            openRGBClientMock.Verify();
        }


    }
}
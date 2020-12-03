using System.Net.Sockets;
using NUnit.Framework;
using OpenRGB.NET.Models;
using Unmockable;

namespace AllMyLights.Test
{
    public class OpenRGBClientTest
    {

        [Test]
        public void Should_update_OpenRGB_devices_with_color()
        {
            var targetColor = System.Drawing.Color.FromName("red");
            var expectedColor = targetColor.ToOpenRGBColor();

            var razerMouse = new Device();
            var corsairH150i = new Device();

            razerMouse.SetColors(new Color(), new Color());
            corsairH150i.SetColors(new Color());

            var openRGBClientMock = Interceptor.For<OpenRGB.NET.OpenRGBClient>()
                .Setup(it => it.Connected).Returns(true)
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


            var client = new OpenRGBClient(openRGBClientMock);
            client.UpdateAll(targetColor);

            openRGBClientMock.Verify();
        }
    }
}
using System.Collections.Generic;
using System.Net.Sockets;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Models;
using AllMyLights.Models.OpenRGB;
using NUnit.Framework;
using OpenRGB.NET.Models;
using Unmockable;

namespace AllMyLights.Test
{
    public class OpenRGBSinkTest
    {
        OpenRGBSinkParams Options = new OpenRGBSinkParams()
        {
            Overrides = new Dictionary<string, DeviceOverride>()
                    {
                        { "MSI X570 Tomahawk", new DeviceOverride() {  ChannelLayout = "RBG"} },
                        { "MSI B450 Gaming Pro", new DeviceOverride() {  Ignore = true } },
                        { "MSI X570 Unify", new DeviceOverride() {
                            Zones = new Dictionary<string, ZoneOverride>
                            {
                                { "JRGB2",  new ZoneOverride() { ChannelLayout = "RBG" } }
                            }
                        } },
                    }
        };

        [Test]
        public void Should_update_devices_with_color()
        {
            var targetColor = System.Drawing.Color.FromName("red");
            var expectedColor = targetColor.ToOpenRGBColor();

            var razerMouse = new Device();
            var corsairH150i = new Device();
            razerMouse.SetName("Razer Copperhead");
            corsairH150i.SetName("Corsair Hydro H150i RGB Pro");
            
            razerMouse.SetColors(new Color(), new Color());
            corsairH150i.SetColors(new Color());

            var openRGBClientMock = Interceptor.For<OpenRGB.NET.OpenRGBClient>()
                .Setup(it => it.Connected)
                .Returns(true)
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


            var client = new OpenRGBSink(Options, openRGBClientMock);
            client.Consume(targetColor);

            openRGBClientMock.Verify();
        }


        [Test]
        public void Should_update_devices_with_channel_layout_defined_in_override()
        {
            var targetColor = System.Drawing.Color.FromArgb(0, 255, 0);
            // override swaps channels G <-> B
            var expectedColor = System.Drawing.Color.FromArgb(0, 0, 255).ToOpenRGBColor();

            var tomahawk = new Device();
            tomahawk.SetName("MSI X570 Tomahawk");
            tomahawk.SetColors(new Color());

            var openRGBClientMock = Interceptor.For<OpenRGB.NET.OpenRGBClient>()
                .Setup(it => it.Connected)
                .Returns(true)
                .Setup(it => it.GetControllerCount())
                .Returns(2)
                .Setup(it => it.GetAllControllerData())
                .Returns(new Device[] { tomahawk })
                .Setup(it => it.UpdateLeds(
                    0,
                    Arg.Where<Color[]>(them => them.ContainsExactly(expectedColor))
                ));


            var client = new OpenRGBSink(Options, openRGBClientMock);
            client.Consume(targetColor);

            openRGBClientMock.Verify();
        }


        [Test]
        public void Should_not_update_devices_with_ignore_true()
        {
            var targetColor = System.Drawing.Color.FromArgb(0, 255, 0);

            var gamingPro = new Device();
            gamingPro.SetName("MSI B450 Gaming Pro").SetColors(new Color());

            var openRGBClientMock = Interceptor.For<OpenRGB.NET.OpenRGBClient>()
                .Setup(it => it.Connected)
                .Returns(true)
                .Setup(it => it.GetControllerCount())
                .Returns(2)
                .Setup(it => it.GetAllControllerData())
                .Returns(new Device[] { gamingPro });


            var client = new OpenRGBSink(Options, openRGBClientMock);
            client.Consume(targetColor);

            openRGBClientMock.Verify();
        }


        [Test]
        public void Should_update_zones_with_channel_layout_defined_in_override()
        {
            var targetColor = System.Drawing.Color.FromArgb(0, 255, 0);
            // override swaps channels G <-> B
            var color = targetColor.ToOpenRGBColor();
            var overriddenColor = System.Drawing.Color.FromArgb(0, 0, 255).ToOpenRGBColor();


            var unify = new Device()
                .SetName("MSI X570 Unify")
                .SetColors(new Color())
                .SetZones(
                    new Zone().Set(name: "JRGB2", ledCount: 2),
                    new Zone().Set(name: "not overridden", ledCount: 1)
                );

            var openRGBClientMock = Interceptor.For<OpenRGB.NET.OpenRGBClient>()
                .Setup(it => it.Connected)
                .Returns(true)
                .Setup(it => it.GetControllerCount())
                .Returns(1)
                .Setup(it => it.GetAllControllerData())
                .Returns(new Device[] { unify })
                .Setup(it => it.UpdateZone(
                    0,
                    0,
                    Arg.Where<Color[]>(them => them.ContainsExactly(overriddenColor, overriddenColor))
                ))
                .Setup(it => it.UpdateZone(
                    0,
                    1,
                    Arg.Where<Color[]>(them => them.ContainsExactly(color))
                ));


            var client = new OpenRGBSink(Options, openRGBClientMock);
            client.Consume(targetColor);

            openRGBClientMock.Verify();
        }
    }
}
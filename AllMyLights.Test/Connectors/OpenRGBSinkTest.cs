using System.Collections.Generic;
using AllMyLights.Common;
using AllMyLights.Connectors.Sinks.OpenRGB;
using Moq;
using NUnit.Framework;
using OpenRGB.NET;
using OpenRGB.NET.Models;

namespace AllMyLights.Test
{
    public class OpenRGBSinkTest
    {
        OpenRGBSinkOptions Options = new OpenRGBSinkOptions()
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

            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();
            openRGBClientMock.Setup(it => it.GetControllerCount()).Returns(2).Verifiable();
            openRGBClientMock.Setup(it => it.GetAllControllerData()).Returns(new Device[] { razerMouse, corsairH150i }).Verifiable();
            openRGBClientMock.Setup(it => it.UpdateLeds(
                    0,
                    It.Is<Color[]>(them => them.ContainsExactly(expectedColor, expectedColor))
                )).Verifiable();
            openRGBClientMock.Setup(it => it.UpdateLeds(
                    1,
                    It.Is<Color[]>(them => them.ContainsExactly(expectedColor))
                )).Verifiable();


            var sink = new OpenRGBSink(Options, openRGBClientMock.Object);
            sink.Consume(new Ref<System.Drawing.Color>(targetColor));

            openRGBClientMock.Verify();
        }

        [Test]
        public void Should_load_a_profile()
        {

            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();

            openRGBClientMock.Setup(it => it.GetProfiles()).Returns(new string[] { "Blue.orp" }).Verifiable();
            openRGBClientMock.Setup(it => it.LoadProfile("Blue.orp")).Verifiable();

            var sink = new OpenRGBSink(Options, openRGBClientMock.Object);
            sink.Consume("Blue.orp");

            openRGBClientMock.Verify();
        }

        [Test]
        public void Should_not_try_to_load_a_non_existing_profile()
        {

            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();

            openRGBClientMock.Setup(it => it.GetProfiles()).Returns(new string[] { "Blue.orp" }).Verifiable();

            var sink = new OpenRGBSink(Options, openRGBClientMock.Object);
            sink.Consume("Pink.orp");

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


            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();
            openRGBClientMock.Setup(it => it.GetControllerCount()).Returns(2).Verifiable();
            openRGBClientMock.Setup(it => it.GetAllControllerData()).Returns(new Device[] { tomahawk }).Verifiable();
            openRGBClientMock.Setup(it => it.UpdateLeds(
                    0,
                    It.Is<Color[]>(them => them.ContainsExactly(expectedColor))
                )).Verifiable();


            var client = new OpenRGBSink(Options, openRGBClientMock.Object);
            client.Consume(new Ref<System.Drawing.Color>(targetColor));

            openRGBClientMock.Verify();
        }


        [Test]
        public void Should_not_update_devices_with_ignore_true()
        {
            var targetColor = System.Drawing.Color.FromArgb(0, 255, 0);

            var gamingPro = new Device();
            gamingPro.SetName("MSI B450 Gaming Pro").SetColors(new Color());

            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();
            openRGBClientMock.Setup(it => it.GetControllerCount()).Returns(2).Verifiable();
            openRGBClientMock.Setup(it => it.GetAllControllerData()).Returns(new Device[] { gamingPro }).Verifiable();


            var client = new OpenRGBSink(Options, openRGBClientMock.Object);
            client.Consume(new Ref<System.Drawing.Color>(targetColor));

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

            var openRGBClientMock = new Mock<IOpenRGBClient>();
            openRGBClientMock.Setup(it => it.Connected).Returns(true).Verifiable();
            openRGBClientMock.Setup(it => it.GetControllerCount()).Returns(1).Verifiable();
            openRGBClientMock.Setup(it => it.GetAllControllerData()).Returns(new Device[] { unify }).Verifiable();
            openRGBClientMock.Setup(it => it.UpdateZone(
                    0,
                    0,
                    It.Is<Color[]>(them => them.ContainsExactly(overriddenColor, overriddenColor))
                ));
            openRGBClientMock.Setup(it => it.UpdateZone(
                    0,
                    1,
                    It.Is<Color[]>(them => them.ContainsExactly(color))
                )).Verifiable();


            var client = new OpenRGBSink(Options, openRGBClientMock.Object);
            client.Consume(new Ref<System.Drawing.Color>(targetColor));

            openRGBClientMock.Verify();
        }
    }
}
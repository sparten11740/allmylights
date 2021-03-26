using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using AllMyLights.Connectors.Sources.OpenRGB;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using OpenRGB.NET;
using OpenRGB.NET.Models;

namespace AllMyLights.Test
{
    public class OpenRGBSourceTest: ReactiveTest
    {
        OpenRGBSourceOptions Options;

        readonly Mock<IOpenRGBClient> OpenRGBClientMock = new Mock<IOpenRGBClient>();

        [SetUp]
        public void Setup()
        {
            Options = new OpenRGBSourceOptions
            {
                Server = "wayne-foundation.com",
                Port = 1863,
                PollingInterval = 10
            };
        }


        [Test]
        public void Should_emit_distinct_device_colors()
        {
            var scheduler = new TestScheduler();
            var pollingInterval = scheduler.CreateColdObservable(
                new Recorded<Notification<long>>(10, Notification.CreateOnNext(1L)),
                new Recorded<Notification<long>>(20, Notification.CreateOnNext(2L)),
                new Recorded<Notification<long>>(30, Notification.CreateOnNext(2L))
            );

            var red = System.Drawing.Color.FromArgb(255, 0, 0).ToOpenRGBColor();
            var green = System.Drawing.Color.FromArgb(0, 255, 0).ToOpenRGBColor();
            var blue = System.Drawing.Color.FromArgb(0, 0, 255).ToOpenRGBColor();

            var corsairH150i = new Device();
            corsairH150i.SetColors(red, green);
            corsairH150i.SetName("Corsair H150");

            var devices = new Device[] {
                corsairH150i
            };

            var corsairH150iUnchanged = new Device();
            corsairH150iUnchanged.SetColors(red, green);
            corsairH150iUnchanged.SetName("Corsair H150");

            var devicesUnchanged = new Device[] {
                corsairH150iUnchanged
            };


            var updatedCorsairH150i = new Device();
            updatedCorsairH150i.SetColors(green, blue);
            updatedCorsairH150i.SetName("Corsair H150");

            var updatedDevices = new Device[] {
                updatedCorsairH150i
            };


            OpenRGBClientMock.SetupSequence(it => it.GetAllControllerData())
                .Returns(devices)
                .Returns(devicesUnchanged)
                .Returns(updatedDevices);


            var subject = new OpenRGBSource(
                Options,
                OpenRGBClientMock.Object,
                pollingInterval
            );

            var actual = scheduler.Start(
             () => subject.Get() as IObservable<Dictionary<string, DeviceState>>,
             created: 0,
             subscribed: 0,
             disposed: 100
           );


            var deviceState = new DeviceState(new List<System.Drawing.Color> { red.ToSystemColor(), green.ToSystemColor() });
            var deviceStateUpdated = new DeviceState(new List<System.Drawing.Color> { green.ToSystemColor(), blue.ToSystemColor() });
            
            var expected = new Recorded<Notification<Dictionary<string, DeviceState>>>[] {
                OnNext(10, (Dictionary<string, DeviceState> actual) => actual["Corsair H150"].Colors.SequenceEqual(deviceState.Colors) ),
                OnNext(30, (Dictionary<string, DeviceState> actual) => actual["Corsair H150"].Colors.SequenceEqual(deviceStateUpdated.Colors) ),
            };


            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
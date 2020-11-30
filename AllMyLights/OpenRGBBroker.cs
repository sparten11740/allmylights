using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using NLog;
using OpenRGB.NET;
using Unmockable;

namespace AllMyLights
{
    public class OpenRGBBroker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IUnmockable<OpenRGBClient> OpenRGBClient { get; }
        public IColorSubject ColorSubject { get; }

        public OpenRGBBroker(
            IColorSubject colorSubject,
            IUnmockable<OpenRGBClient> openRGBClient
        )
        {
            ColorSubject = colorSubject;
            OpenRGBClient = openRGBClient;
        }

        public void Listen()
        {
            ColorSubject
                .Updates()
                .Subscribe(UpdateAll);
        }

        public void UpdateAll(Color color)
        {
            Logger.Info($"Changing color to {color}");

            var count = OpenRGBClient.Execute(it => it.GetControllerCount());
            var devices = OpenRGBClient.Execute(it => it.GetAllControllerData());

            Logger.Info($"Found {count} devices to update");

            for (int i = 0; i < devices.Length; i++)
            {
                var leds = Enumerable.Range(0, devices[i].Colors.Length)
                    .Select(_ => color.ToOpenRGBColor())
                    .ToArray();

                OpenRGBClient.Execute((it) => it.UpdateLeds(i, leds));
            }
        }
    }
}

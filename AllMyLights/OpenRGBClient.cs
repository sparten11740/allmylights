using System;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using NLog;
using Unmockable;

namespace AllMyLights
{
    public class OpenRGBClient : IOpenRGBClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IUnmockable<OpenRGB.NET.OpenRGBClient> Client { get; }

        public OpenRGBClient(IUnmockable<OpenRGB.NET.OpenRGBClient> openRGBClient)
        {
            Client = openRGBClient;
        }


        public void UpdateAll(Color color) => RequestCatching(() =>
        {
            Logger.Info($"Changing color to {color}");

            var count = Client.Execute(it => it.GetControllerCount());
            var devices = Client.Execute(it => it.GetAllControllerData());
            Logger.Info($"Found {count} devices to update");

            for (int i = 0; i < devices.Length; i++)
            {
                var leds = Enumerable.Range(0, devices[i].Colors.Length)
                    .Select(_ => color.ToOpenRGBColor())
                    .ToArray();

                Client.Execute((it) => it.UpdateLeds(i, leds));
            }
        });

        private void RequestCatching(Action request)
        {
            try
            {
                if (!Client.Execute(it => it.Connected))
                {
                    Client.Execute(it => it.Connect());
                }

                request();
            }
            catch (Exception e)
            {
                Logger.Error($"Disconnected from OpenRGB ({e.Message})");
                Reconnect(() => RequestCatching(request));
            }
        }

        private void Reconnect(Action onSuccess)
        {
            // we have to be a little nasty here, since the client library does not expose any means of reconnecting
            var clientType = typeof(OpenRGB.NET.OpenRGBClient);
            var socketField = clientType.GetField("_socket", BindingFlags.NonPublic | BindingFlags.Instance);
            var socket = Client.Execute((it) => socketField.GetValue(it) as Socket);

            try
            {
                Client.Execute((it) => socketField.SetValue(it, new Socket(SocketType.Stream, ProtocolType.Tcp))); 
                Client.Execute((it) => it.Connect());
                onSuccess();
            }
            catch (Exception e)
            {
                Logger.Error($"Connection to OpenRGB failed: {e.Message}");
            }
        }
    }
}

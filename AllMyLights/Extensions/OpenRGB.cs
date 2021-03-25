using System;
using System.Net.Sockets;
using System.Reflection;
using NLog;
using OpenRGB.NET;

namespace AllMyLights.Extensions
{
    public static class OpenRGB
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static T RequestCatching<T>(this IOpenRGBClient client, Func<T> request)
        {
            try
            {
                if (!client.Connected)
                {
                    client.Connect();
                }

                return request();
            }
            catch (Exception e)
            {
                Logger.Error($"Disconnected from OpenRGB ({e.Message})");
                return client.Reconnect(() => client.RequestCatching(request));
            }
        }

        public static T Reconnect<T>(this IOpenRGBClient client, Func<T> onSuccess)
        {
            // we have to be a little nasty here, since the client library does not expose any means of reconnecting
            var clientType = typeof(OpenRGBClient);
            var socketField = clientType.GetField("_socket", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                socketField.SetValue(client, new Socket(SocketType.Stream, ProtocolType.Tcp));
                client.Connect();
                return onSuccess();
            }
            catch (Exception e)
            {
                Logger.Error($"Connection to OpenRGB failed: {e.Message}");
            }

            return default;
        }
    }
}

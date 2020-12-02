using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Models;
using NLog;
using OpenRGB.NET;
using Unmockable;

namespace AllMyLights
{
    public static class OpenRGBClientFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IObservable<IUnmockable<OpenRGBClient>> GetInstance(Configuration configuration)
        {
            return Observable.Create<IUnmockable<OpenRGBClient>>((observer) =>
            {
                string ip = configuration.OpenRgb?.Server ?? "127.0.0.1";
                int port = configuration.OpenRgb?.Port ?? 6742;
                try
                {
                    var openRGBClient = new OpenRGBClient(ip: ip, port: port).Wrap();
                    observer.OnNext(openRGBClient);
                    observer.OnCompleted();
                }
                catch (TimeoutException e)
                {
                    Logger.Error($"Failed to connect to OpenRGB. Make sure that your server is running at {ip}:{port}. Attempting to reconnect in 10s...");
                    observer.OnError(e);
                }

                return () => { };
            }).RetryWhen((e) => Observable.Interval(TimeSpan.FromSeconds(10)));
        }
    }
}

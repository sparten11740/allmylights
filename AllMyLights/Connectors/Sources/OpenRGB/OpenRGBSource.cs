using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using AllMyLights.Extensions;
using NLog;

namespace AllMyLights.Connectors.Sources.OpenRGB
{
    public class OpenRGBSource: Source
    {
        protected override IObservable<object> Value { get; }

        private IOpenRGBClient Client { get; }
        private readonly ReplaySubject<string> Subject = new(1);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Device[] LastDevices { get; set; }

        public OpenRGBSource(
            OpenRGBSourceOptions options,
            IOpenRGBClient client,
            IObservable<long> pollingInterval
        ) : base(options)
        {
            Value = Subject.AsObservable();
            Client = client;



            pollingInterval.Subscribe((_) =>
            {
                var devices = Client.RequestCatching(() => Client.GetAllControllerData());

                if(LastDevices != null && devices.SequenceEqual(LastDevices, new DeviceEqualityComparer()))
                {
                    return;
                }

                Logger.Info($"OpenRGB device color changed");

                LastDevices = devices;

                var deviceStates = new Dictionary<string, DeviceState>();

                devices.ToList().ForEach(it =>
                {
                    deviceStates.Add(it.Name, new DeviceState()
                    {
                        Colors = it.Colors.Select(color => color.ToHexString())
                    });
                });

                Subject.OnNext(JsonConvert.SerializeObject(deviceStates));
            });
        }
    }
}

using OpenRGB.NET.Models;
using System.Collections.Generic;
using System.Reactive;

namespace AllMyLights
{
    public interface IOpenRGBClient
    {
        Unit UpdateAll(System.Drawing.Color color);
        IEnumerable<Device> GetDevices();
    }
}
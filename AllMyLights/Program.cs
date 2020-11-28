using System;
using System.Linq;
using OpenRGB.NET;

namespace AllMyLights
{
    class Program
    {
        static void Main(string[] args)
        {
            using var client = new OpenRGBClient(autoconnect: true, timeout: 1000);

            var deviceCount = client.GetControllerCount();
            var devices = client.GetAllControllerData();
        }
    }
}

using System;
using System.Reactive.Linq;

namespace AllMyLights
{
    public class OpenRGBBroker
    {
        public IOpenRGBClient OpenRGBClient { get; }
        public IColorSubject ColorSubject { get; }

        public OpenRGBBroker(
            IColorSubject colorSubject,
            IOpenRGBClient openRGBClient
        )
        {
            ColorSubject = colorSubject;
            OpenRGBClient = openRGBClient;
        }

        public void Listen()
        {
            ColorSubject
                .Get()
                .Subscribe((color) => {
                    OpenRGBClient.UpdateAll(color);
                });
        }
    }
}

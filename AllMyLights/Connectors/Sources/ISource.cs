using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AllMyLights.Connectors.Sources
{
    public interface ISource
    {
        IObservable<Color> Get();
    }
}

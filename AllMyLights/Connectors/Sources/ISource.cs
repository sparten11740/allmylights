using System;

namespace AllMyLights.Connectors.Sources
{
    public interface ISource
    {
        IObservable<object> Get();
    }
}

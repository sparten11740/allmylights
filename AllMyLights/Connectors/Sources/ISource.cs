using System;

namespace AllMyLights.Connectors.Sources
{
    public interface ISource: IConnector
    {
        IObservable<object> Get();
    }
}

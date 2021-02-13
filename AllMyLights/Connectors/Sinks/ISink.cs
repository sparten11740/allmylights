using System.Collections.Generic;

namespace AllMyLights.Connectors.Sinks
{
    public interface ISink
    {
        void Consume(object value);
        object GetInfo() => null;
    }
}

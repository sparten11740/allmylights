using System.Collections.Generic;

namespace AllMyLights.Connectors.Sinks
{
    public interface ISink
    {
        void Consume(object value);
        IEnumerable<object> GetConsumers() => new List<object>();
    }
}

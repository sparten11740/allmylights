using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AllMyLights.Connectors.Sinks
{
    public interface ISink
    {
        void Consume(Color color);
        IEnumerable<object> GetConsumers() => new List<object>();
    }
}

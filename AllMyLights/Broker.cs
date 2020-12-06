using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace AllMyLights
{
    public class Broker
    {

        private List<ISink> Sinks { get; } = new List<ISink>();
        private List<ISource> Sources { get; } = new List<ISource>();

        public Broker RegisterSources(params ISource[] sources)
        {
            Sources.AddRange(sources);
            return this;
        }

        public Broker RegisterSinks(params ISink[] sinks)
        {
            Sinks.AddRange(sinks);
            return this;
        }

        public void Listen()
        {
            Observable
                .Merge(Sources.Select((it) => it.Get()))
                .Subscribe((color) =>
            {
                Sinks.ForEach((sink) => sink.Consume(color));
            });

        }
    }
}

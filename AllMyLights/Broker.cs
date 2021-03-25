using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using NLog;

namespace AllMyLights
{
    public class Broker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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


        public IObservable<TReturn> HandleError<TException, TReturn>(TException e) where TException: Exception
        {
            return Observable.Empty<TReturn>();
        }

        public void Listen()
        {

            Observable
                .Merge(Sources.Select((it) => it.Get()))
                .Catch(Observable.Empty<object>())
                .Subscribe((value) =>
            {
                Sinks.ForEach((sink) => sink.Consume(value));
            });

        }
    }
}

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
        private List<RouteOptions> Routes { get; set; } = new List<RouteOptions>();

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

        public Broker UseRoutes(params RouteOptions[] routes)
        {
            Routes.AddRange(routes);
            return this;
        }


        public IObservable<TReturn> HandleError<TException, TReturn>(TException e) where TException: Exception
        {
            return Observable.Empty<TReturn>();
        }

        public void Listen()
        {

            if(Routes.Count() == 0) {
                Logger.Info("No routes specified. All emitted values will be passed on to all sinks.");
                Observable
                    .Merge(Sources.Select((it) => it.Get()))
                    .Catch(Observable.Empty<object>())
                    .Subscribe((value) =>
                {
                    Sinks.ForEach((sink) => sink.Consume(value));
                });
                return;
            }

            Logger.Info("Setting up routes...");
            Routes.ForEach(route =>
            {

                var source = Sources.Find(source => source.Id == route.From);
                var sinks = Sinks.Where(sink => route.To.Contains(sink.Id)).ToList();
                Logger.Info($"Values from {source} will be passed on to {string.Join(", ", sinks.Select(it => it.ToString()))}");

                source
                    .Get()
                    .Catch(Observable.Empty<object>())
                    .Subscribe((value) =>
                    {
                        sinks.ForEach((sink) => sink.Consume(value));
                    });
            });

            var unconnectedSources = Sources.Where((source) => !Routes.Select((route) => route.From).Contains(source.Id));
            var unconnectedSinks = Sinks.Where((sink) => !Routes.SelectMany((route) => route.To).Contains(sink.Id));

            if (unconnectedSources.Count() > 0)
            {
                Logger.Warn($"Found {unconnectedSources.Count()} unconnected sources. Emitted values from those will never be processed by any sinks:");
                Logger.Warn(string.Join(", ", unconnectedSources.Select(source => source.ToString())));
            }

            if (unconnectedSinks.Count() > 0)
            {
                Logger.Warn($"Found {unconnectedSinks.Count()} unconnected sinks (not targeted by any route):");
                Logger.Warn(string.Join(", ", unconnectedSinks.Select(sink => sink.ToString())));
                Logger.Warn("Please double check your configuration");
            }
        }
    }
}

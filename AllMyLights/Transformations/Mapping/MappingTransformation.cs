﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace AllMyLights.Transformations.Mapping
{
    public class MappingTransformation : ITransformation<object>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string Type = "Mapping";

        private IEnumerable<Tuple<Regex, object>> Mappings { get; }
        private bool FailOnMiss { get; } = false;

        public MappingTransformation(MappingTransformationOptions options)
        {
            Mappings = options.Mappings.Select(t => new Tuple<Regex, object>(new Regex(t.From), t.To));
            FailOnMiss = options.FailOnMiss;
        }

        public Func<IObservable<object>, IObservable<object>> GetOperator()
        {
            return (source) => source.Select(input =>
            {
                var inputString = input as string;
                Match match = null;

                Logger.Debug($"See if any mappings apply for {inputString}");

                var mapping = Mappings.FirstOrDefault(m =>
                {
                    match = m.Item1.Match(inputString);
                    return match.Success;
                });

                Logger.Debug(() => $"{(match.Success ? $"{inputString} matched by {mapping.Item1}. Substituting {mapping.Item2}..." : "No applicable mapping found.")}");

                if (!match.Success)
                {
                    return FailOnMiss ? Observable.Empty<string>() : Observable.Return(inputString);
                }

                if(mapping.Item2 is string to)
                {
                    return Observable.Return(match.Result(to));
                }


                return Observable.Return(mapping.Item2);
            })
            .Switch()
            .Do((output) =>
            {
                Logger.Debug($"Mapping returns substituted value {output}");
            });
        }
    }
}

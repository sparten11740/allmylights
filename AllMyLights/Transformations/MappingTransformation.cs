using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using AllMyLights.Models.Transformations;
using System.Text.RegularExpressions;
using NLog;

namespace AllMyLights.Transformations
{
    public class MappingTransformation : ITransformation<string>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string Type = "Mapping";

        private IEnumerable<Tuple<Regex, string>> Mappings { get; }
        private bool FailOnMiss { get; } = false;

        public MappingTransformation(MappingTransformationOptions options)
        {
            Mappings = options.Mappings.Select(t => new Tuple<Regex, string>(new Regex(t.From), t.To));
            FailOnMiss = options.FailOnMiss;
        }

        public Func<IObservable<object>, IObservable<string>> GetOperator()
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

                IObservable<string> output = match.Success ?
                    Observable.Return(match.Result(mapping.Item2)) :
                    (FailOnMiss ? Observable.Empty<string>() : Observable.Return(inputString));


                return output;
            })
            .Switch()
            .Do((output) =>
            {
                Logger.Debug($"Mapping returns substituted value {output}");
            });
        }
    }
}

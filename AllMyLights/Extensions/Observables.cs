using System;
using System.Linq;
using System.Reactive.Linq;

namespace AllMyLights.Extensions
{
    public static class Observables
    {
        public static IObservable<object> Pipe(
            this IObservable<object> source,
            params Func<IObservable<dynamic>, IObservable<object>>[] transformations)
        {
            if(transformations.Count() == 0)
            {
                return source;
            }

            return transformations.Aggregate(source, (current , transform) => transform(current));
        }
    }
}

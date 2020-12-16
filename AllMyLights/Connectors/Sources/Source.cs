using System;
using System.Collections.Generic;
using System.Linq;
using AllMyLights.Extensions;
using AllMyLights.Models;
using AllMyLights.Transformations;

namespace AllMyLights.Connectors.Sources
{
    public abstract class Source: ISource
    {
        public IEnumerable<ITransformation<object>> Transformations { get; }

        protected virtual IObservable<object> Value { get; }

        public Source(SourceOptions options) {
            Transformations = options.Transformations?.Select((it) => TransformationFactory.GetInstance(it)) ?? new List<ITransformation<object>>();
        }

        public IObservable<object> Get() => Value.Pipe(Transformations.Select(it => it.GetOperator()).ToArray());
    }
}

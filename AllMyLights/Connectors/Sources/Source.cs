using System;
using System.Collections.Generic;
using System.Linq;
using AllMyLights.Extensions;
using AllMyLights.Transformations;

namespace AllMyLights.Connectors.Sources
{
    public abstract class Source: ISource
    {
        public IEnumerable<ITransformation<object>> Transformations { get; }

        protected virtual IObservable<object> Value { get; }
        public string Id { get; }

        public Source(SourceOptions options) {
            Id = options.Id;
            Transformations = options.Transformations?.Select((it) => TransformationFactory.GetInstance(it)) ?? new List<ITransformation<object>>();
        }

        public IObservable<object> Get() => Value.Pipe(Transformations.Select(it => it.GetOperator()).ToArray());

        public virtual object GetInfo() => null;
    }
}

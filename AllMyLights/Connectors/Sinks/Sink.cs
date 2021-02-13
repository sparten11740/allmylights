using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AllMyLights.Extensions;
using AllMyLights.Models;
using AllMyLights.Transformations;

namespace AllMyLights.Connectors.Sinks
{
    public abstract class Sink: ISink
    {
        public IEnumerable<ITransformation<object>> Transformations { get;  }

        private Subject<object> Subject = new Subject<object>();

        protected IObservable<object> Next { get; }

        public Sink(SinkOptions options) {
            Transformations = options.Transformations?.Select((it) => TransformationFactory.GetInstance(it)) ?? new List<ITransformation<object>>();
            Next = Subject.AsObservable().Pipe(Transformations.Select(it => it.GetOperator()).ToArray());
        }

        public void Consume(object value)
        {
            Subject.OnNext(value);
        }

        public virtual object GetInfo() => null;
    }
}

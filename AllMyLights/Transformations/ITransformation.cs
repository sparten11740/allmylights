using System;

namespace AllMyLights.Transformations
{
    public interface ITransformation<out TReturn>
    {
        public Func<IObservable<object>, IObservable<TReturn>> GetOperator();
    }
}

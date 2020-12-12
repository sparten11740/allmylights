using System;
using System.Linq;
namespace AllMyLights.Transformations
{
    public interface ITransformation<out TReturn>
    {
        public Func<IObservable<object>, IObservable<TReturn>> GetOperator();
    }
}

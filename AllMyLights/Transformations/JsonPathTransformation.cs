using System;
using System.Reactive.Linq;
using AllMyLights.Models.Transformations;
using Newtonsoft.Json.Linq;

namespace AllMyLights.Transformations
{
    public class JsonPathTransformation<T>: ITransformation<T> 
    {
        public const string Type = "JsonPath";

        private string Path { get; set; }

        public JsonPathTransformation(JsonPathTransformationOptions options)
        {
            Path = options.Expression;
        }

        public Func<IObservable<object>, IObservable<T>> GetOperator()
        {
            return (source) =>
            {
                return source.Select((input) =>
                {
                    if (!(input is string)) {
                        throw new ArgumentException($"{nameof(JsonPathTransformation<T>)} requires input to be of type string");
                    }

                    JObject o = JObject.Parse(input as string);
                    var value = o.SelectToken(Path);

                    return value.ToObject<T>();
                });
            };
        }
    }
}

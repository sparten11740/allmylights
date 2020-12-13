using System;
using System.Reactive.Linq;
using AllMyLights.Models.Transformations;
using Newtonsoft.Json.Linq;
using NLog;

namespace AllMyLights.Transformations
{
    public class JsonPathTransformation<T>: ITransformation<T> 
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

                    Logger.Debug($"Applying JsonPath expression {Path} to {input}");

                    JObject o = JObject.Parse(input as string);
                    var value = o.SelectToken(Path);

                    Logger.Debug($"Expression returned value {value}");

                    return value.ToObject<T>();
                });
            };
        }
    }
}

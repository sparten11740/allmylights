using System;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using NLog;

namespace AllMyLights.Transformations.JsonPath
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
                        Logger.Error($"{nameof(JsonPathTransformation<T>)} requires input to be of type string");
                        return Observable.Empty<T>();
                    }

                    Logger.Debug($"Applying JsonPath expression {Path} to {input}");

                    try
                    {
                        JObject o = JObject.Parse(input as string);
                        var value = o.SelectToken(Path);

                        if (value == null)
                        {
                            Logger.Warn($"{Path} yielded no result on the given input");
                            return Observable.Empty<T>();
                        }

                        Logger.Debug($"Expression returned value {value}");

                        return Observable.Return(value.ToObject<T>());
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        return Observable.Empty<T>();
                    }
                }).Switch();
            };
        }
    }
}

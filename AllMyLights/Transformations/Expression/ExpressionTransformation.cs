using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using AllMyLights.Common;
using CodingSeb.ExpressionEvaluator;
using NLog;
using SystemColor = System.Drawing.Color;

namespace AllMyLights.Transformations.Expression
{
    public class ExpressionTransformation<T>: ITransformation<T>  where T: class
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string Type = "Expression";

        private string Expression { get; }

        private ExpressionEvaluator Evaluator { get; } = new ExpressionEvaluator
        {
            Variables = new Dictionary<string, object>()
        };

        public ExpressionTransformation(ExpressionTransformationOptions options)
        {
            Expression = options.Expression;
            Evaluator.StaticTypesForExtensionsMethods.Add(typeof(ColorConverter));
            Evaluator.StaticTypesForExtensionsMethods.Add(typeof(Enumerable));
        }

        public Func<IObservable<object>, IObservable<T>> GetOperator()
        {
            return (source) =>
            {
                return source.Select((input) =>
                {
                    try
                    {
                        var value = input switch
                        {
                            Ref<SystemColor> r => r.Value,
                            _ => input
                        };

                        Evaluator.Variables["value"] = value;

                        Logger.Debug($"Expression received value {value}");

                        var result = Evaluator.Evaluate(Expression) as T;

                        Logger.Debug($"Expression returned value {result}");

                        return Observable.Return(result);
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

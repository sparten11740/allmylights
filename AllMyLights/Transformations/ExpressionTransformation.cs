﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Models.Transformations;
using CodingSeb.ExpressionEvaluator;
using NLog;

namespace AllMyLights.Transformations
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
        }

        public Func<IObservable<object>, IObservable<T>> GetOperator()
        {
            return (source) =>
            {
                return source.Select((input) =>
                {
                    try
                    {
                        var value = input is Ref<Color> ? (input as Ref<Color>).Value : input;
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

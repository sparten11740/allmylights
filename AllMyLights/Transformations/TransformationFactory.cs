using System;
using AllMyLights.Transformations;
using AllMyLights.Transformations.Color;
using AllMyLights.Transformations.Expression;
using AllMyLights.Transformations.JsonPath;
using AllMyLights.Transformations.Mapping;

namespace AllMyLights.Transformations
{
    public class TransformationFactory
    {
        public static ITransformation<object> GetInstance(TransformationOptions options)
        {
            return options switch
            {
                ColorTransformationOptions colorOptions => new ColorTransformation(colorOptions),
                MappingTransformationOptions mappingOptions => new MappingTransformation(mappingOptions),
                ExpressionTransformationOptions expressionOptions => new ExpressionTransformation<object>(expressionOptions),
                JsonPathTransformationOptions jpathOptions => new JsonPathTransformation<string>(jpathOptions),
                _ => throw new NotImplementedException($"Transformation for type {options.Type} not registered")
            };
        }
    }
}

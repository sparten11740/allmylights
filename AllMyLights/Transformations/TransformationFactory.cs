using System;
using AllMyLights.Models.Transformations;

namespace AllMyLights.Transformations
{
    public class TransformationFactory
    {
        public static ITransformation<object> GetInstance(TransformationOptions options)
        {
            return options switch
            {
                ColorTransformationOptions colorOptions => new ColorTransformation(colorOptions),
                JsonPathTransformationOptions jpathOptions => new JsonPathTransformation<string>(jpathOptions),
                _ => throw new NotImplementedException($"Transformation for type {options.Type} not registered")
            };
        }
    }
}

using System.Runtime.Serialization;
using AllMyLights.JsonConverters;
using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace AllMyLights.Models.Transformations
{
    [JsonSchemaFlatten]
    [JsonConverter(typeof(InheritanceConverter), "Type", nameof(TransformationOptions))]
    [
        KnownType(typeof(ColorTransformationOptions)),
        KnownType(typeof(JsonPathTransformationOptions)),
        KnownType(typeof(MappingTransformationOptions)),
        KnownType(typeof(ExpressionTransformationOptions)),
    ]
    public class TransformationOptions
    {
        public string Type { get; set; }
    }
}
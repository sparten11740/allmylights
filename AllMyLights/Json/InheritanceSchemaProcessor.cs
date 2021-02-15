using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using AllMyLights.Transformations;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;

namespace AllMyLights.Json
{
    public class InheritanceSchemaProcessor : ISchemaProcessor
    {

        private readonly ICollection<Type> BaseTypes = new List<Type>() {
            typeof(SourceOptions),
            typeof(SinkOptions),
            typeof(TransformationOptions)
        };


        public void Process(SchemaProcessorContext context)
        {
            if (BaseTypes.Contains(context.Type))
            {
                var attributes = context.Type.GetCustomAttributes(typeof(KnownTypeAttribute), true) as KnownTypeAttribute[];
                context.Schema.DiscriminatorObject = new OpenApiDiscriminator()
                {
                    PropertyName = "Type"
                };

                context.Schema.AllowAdditionalProperties = true;

                foreach (var attribute in attributes)
                {
                    var schema = context.Generator.GenerateWithReference<JsonSchema>(
                        attribute.Type.ToContextualType(),
                        context.Resolver
                    );

                    context.Schema.AnyOf.Add(schema);
                    context.Schema.DiscriminatorObject.Mapping.Add(
                        attribute.Type.Name.Replace(context.Type.Name, string.Empty),
                        schema
                    );
                }
            }
        }
    }
}

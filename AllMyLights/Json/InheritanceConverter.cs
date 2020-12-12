using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.Converters;

namespace AllMyLights.JsonConverters
{
    public class InheritanceConverter : JsonInheritanceConverter
    {
        private string TypeSuffix { get; }

        public InheritanceConverter(string name, string typeSuffix) : base(name)
        {
            TypeSuffix = typeSuffix;
        }

        public override string GetDiscriminatorValue(Type type)
        {
            return type.Name.Replace(TypeSuffix, string.Empty);
        }

        protected override Type GetDiscriminatorType(JObject jObject, Type objectType, string discriminatorValue)
        {
            return base.GetDiscriminatorType(jObject, objectType, discriminatorValue + TypeSuffix);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AllMyLights.Extensions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Validation;
using NLog;

namespace AllMyLights.Json
{

    public class InheritanceValidator<T>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string Dicriminator = "Type";

        private readonly Dictionary<string, JsonSchema> Schemas = new Dictionary<string, JsonSchema>();
        private JArray Objects { get; set; }
        private string Path { set; get; }
        private ICollection<Func<JObject, int, bool>> ChildValidators { get; set; } = new List<Func<JObject, int, bool>>();
        private JsonSchemaGeneratorSettings Settings { get; }
        private Action<SchemaValidationError> ErrorHandler { get; set; } = Logger.Error;

        public InheritanceValidator(
            JArray objects,
            JsonSchemaGeneratorSettings settings)
        {
            Settings = settings;
            Objects = objects;

            SetupSchemas();
        }

        public InheritanceValidator<T> ValidateIsolated<TProp>(string property)
        {
            ChildValidators.Add((obj, index) =>
            {
                var children = obj.SelectToken($"$.{property}");
                obj.Remove(property);


                return new InheritanceValidator<TProp>(children as JArray, Settings)
                    .At($"{Path}[{index}].{property}")
                    .OnError(RaiseError)
                    .Validate();
            });
            return this;
        }

        public InheritanceValidator<T> At(string path)
        {
            Path = path;
            return this;
        }

        public InheritanceValidator<T> OnError(Action<SchemaValidationError> errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }

        private void RaiseError(SchemaValidationError error)
        {
            ErrorHandler.Invoke(error);
        }

        private void RaiseErrors(ICollection<ValidationError> errors, string path)
        {
            foreach (ValidationError error in errors)
            {
                RaiseError(new SchemaValidationError(path ?? error.Path, error.Message()));
            }
        }

        public bool Validate()
        {
            if (Objects == null)
            {
                return true;
            }

            var count = Objects.Count();
            var isValid = true;
            for (int i = 0; i < count; i++)
            {
                var obj = (JObject)Objects[i];


                isValid = ChildValidators.Select(validate => validate(obj, 0)).Aggregate(true, (a, b) => a && b) && isValid;
                string discriminatorValue = obj.SelectToken($"$.{Dicriminator}")?.ToString();

                if (discriminatorValue == null)
                {
                    RaiseError(new SchemaValidationError(
                         path: $"{Path}[{i}]",
                         message: $"Required property {Dicriminator} is missing."
                    ));
                    continue;
                }

                try
                {
                    var schema = Schemas[discriminatorValue];
                    var errors = schema.Validate(obj);
                    isValid = isValid && errors.Count() == 0;

                    RaiseErrors(errors, $"{Path}[{i}]");
                }
                catch (KeyNotFoundException)
                {
                    RaiseError(new SchemaValidationError(
                         path: $"{Path}[{i}].{Dicriminator}",
                         message: $"Property {Dicriminator} can only be one of the following: {string.Join(", ", Schemas.Keys)}. (found {discriminatorValue})"

                    ));
                    continue;
                }
            }
            return isValid;
        }

        private void SetupSchemas()
        {
            var type = typeof(T);
            var typeAttributes = type
                .GetCustomAttributes(typeof(KnownTypeAttribute), true)
                as KnownTypeAttribute[];

            typeAttributes.ToList().ForEach(attribute =>
            {
                Schemas.Add(attribute.Type.Name.Replace(
                    typeof(T).Name,
                    string.Empty), JsonSchema.FromType(attribute.Type, Settings));
            });
        }
    }

}

using System;
using System.Linq;
using AllMyLights.Connectors.Sinks;
using AllMyLights.Connectors.Sources;
using AllMyLights.Extensions;
using AllMyLights.Transformations;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NLog;

namespace AllMyLights.Json
{

    public partial class ConfigurationValidator
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private JsonSchemaGeneratorSettings Settings { get; }

        public ConfigurationValidator(JsonSchemaGeneratorSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// NJsonSchema only supports validating JSON Schema draft 4 type schemas.
        /// Open API concepts like discriminator and discrimantor mappings are disregarded
        /// and hence we're left to take the json apart and manually validate.
        /// </summary>
        ///

        public void Validate(string config)
        {
            var rootSchema = JsonSchema.FromType<Configuration>();
            var o = JObject.Parse(config);

            JArray sources = o.SelectToken($"$.{nameof(Configuration.Sources)}") as JArray;
            JArray sinks = o.SelectToken($"$.{nameof(Configuration.Sinks)}") as JArray;

            o.Remove("Sources");
            o.Remove("Sinks");

            var errors = rootSchema.Validate(o).Select(it => new SchemaValidationError(it.Path, it.Message())).ToList();

            new InheritanceValidator<SourceOptions>(sources, Settings)
                .WithPath($"/{nameof(Configuration.Sources)}")
                .OnError(errors.Add)
                .ValidateIsolated<TransformationOptions>(nameof(SourceOptions.Transformations))
                .Validate();

            new InheritanceValidator<SinkOptions>(sinks, Settings)
                .WithPath($"/{nameof(Configuration.Sinks)}")
                .OnError(errors.Add)
                .ValidateIsolated<TransformationOptions>(nameof(SinkOptions.Transformations))
                .Validate();

            if(errors.Count() > 0)
            {
                Logger.Error("Validation of config file failed. We found the following issues with your file:");
                errors.ForEach(Logger.Error);

                Environment.Exit((int)ExitCode.InvalidConfig);
            }

        }

    }

}

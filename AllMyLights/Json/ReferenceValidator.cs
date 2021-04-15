using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;

namespace AllMyLights.Json
{

    public class ReferenceValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private JArray Objects { get; set; }
        private string Path { set; get; }
        private string Property { set; get; }
        private string Entity { set; get; }

        private Action<SchemaValidationError> ErrorHandler { get; set; } = Logger.Error;

        private IEnumerable<string> KnownIds { get; set; }

        public ReferenceValidator(JArray objects)
        {
            Objects = objects;
        }



        public ReferenceValidator At(string path)
        {
            Path = path;
            return this;
        }

        public ReferenceValidator Known(IEnumerable<string> ids)
        {
            KnownIds = ids;
            return this;
        }

        public ReferenceValidator EntityName(string entityName)
        {
            Entity = entityName;
            return this;
        }

        public ReferenceValidator ForProperty(string property)
        {
            Property = property;
            return this;
        }

        public ReferenceValidator OnError(Action<SchemaValidationError> errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }

        private void RaiseError(SchemaValidationError error)
        {
            ErrorHandler.Invoke(error);
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

                var value = obj.GetValue(Property);

                var ids = value switch
                {
                    JValue single => new string[] { single.ToString() },
                    JArray multiple => multiple.ToObject<string[]>()
                };

                ids.Where(id => !KnownIds.Contains(id))
                    .ToList()
                    .ForEach(id => {
                        isValid = false;
                        RaiseError(new SchemaValidationError(
                             path: $"{Path}[{i}].{Property}",
                             message: $"Referenced {Entity} id does not exist. Has to be one of: {string.Join(", ", KnownIds)}. (found {id})"

                        ));
                    });
            }

            return isValid;
        }
    }

}

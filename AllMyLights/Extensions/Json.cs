using NJsonSchema.Validation;

namespace AllMyLights.Extensions
{
    public static class Json
    {
       public static string Message(this ValidationError error)
        {
            return error.Kind switch
            {
                ValidationErrorKind.PropertyRequired => $"The required property {error.Property} is missing.",
                ValidationErrorKind.NoAdditionalPropertiesAllowed => $"Unknown property {error.Property} used. Maybe a typo?",
                _ => error.Kind.ToString()
            };
        }
    }
}

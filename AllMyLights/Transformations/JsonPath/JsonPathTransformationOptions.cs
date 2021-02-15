using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Transformations.JsonPath
{
    public class JsonPathTransformationOptions: TransformationOptions
    {
        [Required]
        public string Expression { get; set; }
    }
}
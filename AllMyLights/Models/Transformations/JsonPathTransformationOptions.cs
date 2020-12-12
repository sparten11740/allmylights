using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Transformations
{
    public class JsonPathTransformationOptions: TransformationOptions
    {
        [Required]
        public string Expression { get; set; }
    }
}
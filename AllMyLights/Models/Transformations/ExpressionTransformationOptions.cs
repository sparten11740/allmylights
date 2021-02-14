using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Transformations
{
    public class ExpressionTransformationOptions: TransformationOptions
    {
        [Required]
        public string Expression { get; set; }
    }
}
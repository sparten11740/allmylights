using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Transformations.Expression
{
    public class ExpressionTransformationOptions: TransformationOptions
    {
        [Required]
        public string Expression { get; set; }
    }
}
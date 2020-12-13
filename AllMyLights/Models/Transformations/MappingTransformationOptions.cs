using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AllMyLights.Models.Transformations
{
    public class MappingTransformationOptions: TransformationOptions
    {
        public bool FailOnMiss { get; set; } = false;

        [Required]
        public ICollection<Mapping> Mappings { get; set; }


        public class Mapping
        {
            public string From { get; set; }
            public string To { get; set; }
        }
    }
}
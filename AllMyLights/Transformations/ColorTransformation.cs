using System;
using System.Drawing;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Models.Transformations;

namespace AllMyLights.Transformations
{
    public class ColorTransformation: ITransformation<Ref<Color>>
    {
        public const string Type = "Color";

        private string ChannelLayout { get; }

        public ColorTransformation(ColorTransformationOptions options)
        {
            ChannelLayout = options.ChannelLayout;
        }

        public Func<IObservable<object>, IObservable<Ref<Color>>> GetOperator()
        {
            return (source) =>
            {
                return source.Select((input) =>
                {
                    if (!(input is string)) {
                        throw new ArgumentException($"{nameof(ColorTransformation)} requires input to be of type string");
                    }

                    return new Ref<Color>(ColorConverter.Decode(input as string, ChannelLayout));
                });
            };
        }
    }
}

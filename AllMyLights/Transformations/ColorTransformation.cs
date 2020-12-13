using System;
using System.Drawing;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Models.Transformations;
using NLog;

namespace AllMyLights.Transformations
{
    public class ColorTransformation: ITransformation<Ref<Color>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

                    Logger.Debug($"Decoding color from {input}");
                    Ref<Color> colorRef = new Ref<Color>(ColorConverter.Decode(input as string, ChannelLayout));
                    Logger.Debug($"Decoded {colorRef.Value}");

                    return colorRef;
                });
            };
        }
    }
}

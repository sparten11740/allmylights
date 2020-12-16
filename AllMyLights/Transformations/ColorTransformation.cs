using System;
using System.Drawing;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Models.Transformations;
using NLog;

namespace AllMyLights.Transformations
{
    public class ColorTransformation : ITransformation<Ref<Color>>
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
                    if (!(input is string))
                    {
                        Logger.Error($"{nameof(ColorTransformation)} requires input to be of type string");
                        return Observable.Empty<Ref<Color>>();
                    }

                    Logger.Debug($"Decoding color from {input}");

                    try
                    {
                        Ref<Color> colorRef = new Ref<Color>(ColorConverter.Decode(input as string, ChannelLayout));
                        Logger.Debug($"Decoded {colorRef.Value}");
                        return Observable.Return(colorRef);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        return Observable.Empty<Ref<Color>>();
                    }

                }).Switch();
            };
        }
    }
}

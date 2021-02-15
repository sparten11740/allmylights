using System;
using SystemColor = System.Drawing.Color;
using System.Reactive.Linq;
using AllMyLights.Common;
using NLog;

namespace AllMyLights.Transformations.Color
{
    public class ColorTransformation : ITransformation<Ref<SystemColor>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string Type = "Color";

        private string ChannelLayout { get; }

        public ColorTransformation(ColorTransformationOptions options)
        {
            ChannelLayout = options.ChannelLayout;
        }

        public Func<IObservable<object>, IObservable<Ref<SystemColor>>> GetOperator()
        {
            return (source) =>
            {
                return source.Select((input) =>
                {
                    if (!(input is string))
                    {
                        Logger.Error($"{nameof(ColorTransformation)} requires input to be of type string");
                        return Observable.Empty<Ref<SystemColor>>();
                    }

                    Logger.Debug($"Decoding color from {input}");

                    try
                    {
                        Ref<SystemColor> colorRef = new Ref<SystemColor>(ColorConverter.Decode(input as string, ChannelLayout));
                        Logger.Debug($"Decoded {colorRef.Value}");
                        return Observable.Return(colorRef);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        return Observable.Empty<Ref<SystemColor>>();
                    }

                }).Switch();
            };
        }
    }
}

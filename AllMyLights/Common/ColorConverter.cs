using AllMyLights.Extensions;
using NLog;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AllMyLights
{
    public static class ColorConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static Regex Word { get; } = new Regex("^[a-zA-Z]+$");

        public static Color Decode(string input, string channelLayout = null)
        {
            try
            {
                FromHex(channelLayout != null ? ApplyLayout(input, channelLayout) : input, out var a, out var r, out var g, out var b);
                return Color.FromArgb(a, r, g, b);
            }
            catch (ArgumentException)
            {
                Logger.Info($"{input} is no valid hex-code.");

                if (!Word.Match(input).Success)
                {
                    throw new ArgumentException($"{input} cannot be a valid color name");
                }

                return Color.FromName(input);
            }
        }

        private static string ApplyLayout(string input, string channelLayout)
        {
            var hex = input.StartsWith("#") ? input[1..] : input;
            var channels = channelLayout
                                .ToCharArray()
                                .Select((channel, i) => (channel, i * 2))
                                .Select((it) => (it.channel, hex.Substring(it.Item2, 2)))
                                .ToDictionary(it => it.channel, it => it.Item2);

            return $"{channels.GetOrDefault('R', "00")}{channels.GetOrDefault('G', "00")}{channels.GetOrDefault('B', "00")}{channels.GetOrDefault('A', "FF")}";
        }

        public static int GetChannel(this Color color, char channel) => channel switch
        {
            'R' => color.R,
            'G' => color.G,
            'B' => color.B,
            'A' => color.A,
            _ => default(int)
        };

        public static Color Rearrange(this Color color, string layout)
        {
            var channels = layout
                .ToCharArray()
                .Select((channel) => color.GetChannel(channel));

            return Color.FromArgb(channels.Count() == 4 ? channels.ElementAt(3) : 255, channels.ElementAtOrDefault(0), channels.ElementAtOrDefault(1), channels.ElementAtOrDefault(2));
        }

        public static  OpenRGB.NET.Models.Color ToOpenRGBColor(this Color color) => new OpenRGB.NET.Models.Color(color.R, color.G, color.B);


        private static void FromHex(string hex, out byte a, out byte r, out byte g, out byte b)
        {
            hex = ToRgbaHex(hex);
            if (hex == null || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packedValue))
            {
                throw new ArgumentException("Hexadecimal string is not in the correct format.", nameof(hex));
            }

            a = (byte)(packedValue >> 0);
            r = (byte)(packedValue >> 24);
            g = (byte)(packedValue >> 16);
            b = (byte)(packedValue >> 8);
        }


        private static string ToRgbaHex(string hex)
        {
            hex = hex.StartsWith("#") ? hex[1..] : hex;

            if (hex.Length == 8)
            {
                return hex;
            }

            if (hex.Length == 6)
            {
                return hex + "FF";
            }

            if (hex.Length < 3 || hex.Length > 4)
            {
                return null;
            }

            string red = char.ToString(hex[0]);
            string green = char.ToString(hex[1]);
            string blue = char.ToString(hex[2]);
            string alpha = hex.Length == 3 ? "F" : char.ToString(hex[3]);

            return string.Concat(red, red, green, green, blue, blue, alpha, alpha);
        }
    }
}

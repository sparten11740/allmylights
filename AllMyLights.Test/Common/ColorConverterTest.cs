using System;
using System.Drawing;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class ColorConverterTest
    {

        [Test]
        public void Should_decode_from_hex_code()
        {
            var color = ColorConverter.Decode("#000000");

            Assert.AreEqual(0, Convert.ToInt32(color.R));
            Assert.AreEqual(0, Convert.ToInt32(color.G));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
        }

        [Test]
        public void Should_decode_from_short_hex_code()
        {
            var color = ColorConverter.Decode("#fff");

            Assert.AreEqual(255, Convert.ToInt32(color.R));
            Assert.AreEqual(255, Convert.ToInt32(color.G));
            Assert.AreEqual(255, Convert.ToInt32(color.B));
        }

        [Test]
        public void Should_decode_from_rgba_hex_code()
        {
            var color = ColorConverter.Decode("#000000ff");

            Assert.AreEqual(255, Convert.ToInt32(color.A));
            Assert.AreEqual(0, Convert.ToInt32(color.G));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
        }

        [Test]
        public void Should_decode_from_color_name()
        {
            var color = ColorConverter.Decode("red");

            Assert.AreEqual(255, Convert.ToInt32(color.R));
            Assert.AreEqual(0, Convert.ToInt32(color.G));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
        }

        [Test]
        public void Should_not_decode_as_color_name_on_invalid_hex_for_given_channel_layout()
        {
            try
            {
                var color = ColorConverter.Decode("#ff0000", "_RGB");
                Assert.Fail("Should throw on neither valid hex nor valid color name");
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Should_decode_from_hex_code_with_layout_XRGB()
        {
            var color = ColorConverter.Decode("#FF226688", "_RGB");

            Assert.AreEqual(0x22, Convert.ToInt32(color.R));
            Assert.AreEqual(0x66, Convert.ToInt32(color.G));
            Assert.AreEqual(0x88, Convert.ToInt32(color.B));
            Assert.AreEqual(0xff, Convert.ToInt32(color.A));
        }

        [Test]
        public void Should_decode_from_hex_code_with_layout_ARGB()
        {
            var color = ColorConverter.Decode("#77112233", "ARGB");

            Assert.AreEqual(0x11, Convert.ToInt32(color.R));
            Assert.AreEqual(0x22, Convert.ToInt32(color.G));
            Assert.AreEqual(0x33, Convert.ToInt32(color.B));
            Assert.AreEqual(0x77, Convert.ToInt32(color.A));
        }

        [Test]
        public void Should_decode_from_hex_code_with_layout_BXRG()
        {
            var color = ColorConverter.Decode("#77112233", "B_RG");

            Assert.AreEqual(0x22, Convert.ToInt32(color.R));
            Assert.AreEqual(0x33, Convert.ToInt32(color.G));
            Assert.AreEqual(0x77, Convert.ToInt32(color.B));
            Assert.AreEqual(0xff, Convert.ToInt32(color.A));
        }

        [TestCase("RBG", 11, 33, 22)]
        [TestCase("BRG", 33, 11, 22)]
        [TestCase("BGR", 33, 22, 11)]
        [TestCase("GBR", 22, 33, 11)]
        [TestCase("GRB", 22, 11, 33)]
        public void Should_rearrange_channel_layout_to_RBG(string layout, int red, int green, int blue)
        {
            var color = Color.FromArgb(11, 22, 33);
            var rearranged = color.Rearrange(layout);

            Assert.AreEqual(red, Convert.ToInt32(rearranged.R));
            Assert.AreEqual(green, Convert.ToInt32(rearranged.G));
            Assert.AreEqual(blue, Convert.ToInt32(rearranged.B)); ;
        }


        [TestCase("FF0000", 255)]
        [TestCase("00FF00", 65280)]
        [TestCase("0000FF", 16711680)]
        public void Should_convert_to_BGR_decimal(string hex, int expectedBgr)
        {
            var color = ColorConverter.Decode(hex);

            Assert.AreEqual(expectedBgr, color.ToBgrDecimal());
        }
    }
}
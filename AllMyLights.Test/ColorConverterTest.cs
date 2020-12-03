using System;
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
    }
}
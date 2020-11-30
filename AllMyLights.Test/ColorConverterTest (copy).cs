using System;
using System.Threading.Tasks;
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
        public async Task Should_decode_from_short_hex_code()
        {
            var color = ColorConverter.Decode("#fff");

            Assert.AreEqual(255, Convert.ToInt32(color.R));
            Assert.AreEqual(255, Convert.ToInt32(color.G));
            Assert.AreEqual(255, Convert.ToInt32(color.B));
        }

        [Test]
        public async Task Should_decode_from_argb_hex_code()
        {
            var color = ColorConverter.Decode("#000000ff");

            Assert.AreEqual(255, Convert.ToInt32(color.A));
            Assert.AreEqual(0, Convert.ToInt32(color.G));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
        }

        [Test]
        public async Task Should_decode_from_color_name()
        {
            var color = ColorConverter.Decode("red");

            Assert.AreEqual(255, Convert.ToInt32(color.R));
            Assert.AreEqual(0, Convert.ToInt32(color.G));
            Assert.AreEqual(0, Convert.ToInt32(color.B));
        }
    }
}
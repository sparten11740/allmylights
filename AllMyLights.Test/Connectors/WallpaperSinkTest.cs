#if Windows
using System.Collections.Generic;
using System.IO;
using AllMyLights.Connectors.Sinks.Wallpaper;
using AllMyLights.Platforms;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class WallpaperSinkTest
    {
  
        [Test]
        public void Should_set_background()
        {
            var filePath = @"C:\absolute\path\batman.jpg";
            var desktopMock = new Mock<IDesktop>();
            desktopMock.Setup(it => it.SetBackground(filePath)).Verifiable();

            var sink = new WallpaperSink(new WallpaperSinkOptions(), desktopMock.Object);
            sink.Consume(filePath);

            desktopMock.Verify();
        }

        [Test]
        public void Should_set_background_per_screen()
        {
            var input = JObject.Parse(@"{""0"": ""C:\\absolute\\path\\batman.jpg"", ""1"": ""C:\\absolute\\path\\joker.jpg""}");

            var expectedFilePathByScreen = new Dictionary<int, string>()
            {
                { 0, @"C:\absolute\path\batman.jpg" },
                { 1, @"C:\absolute\path\joker.jpg" },
            };

            var desktopMock = new Mock<IDesktop>();
            desktopMock.Setup(it => it.SetBackgrounds(expectedFilePathByScreen)).Verifiable();

            var sink = new WallpaperSink(new WallpaperSinkOptions(), desktopMock.Object);
            sink.Consume(input);

            desktopMock.Verify();
        }

        [Test]
        public void Should_do_nothing_on_receiving_the_same_value_twice()
        {
            var filePath = @"C:\absolute\path\batman.jpg";
            var otherPath = @"C:\absolute\path\other.jpg";
            var desktopMock = new Mock<IDesktop>();
            desktopMock.Setup(it => it.SetBackground(filePath));
            desktopMock.Setup(it => it.SetBackground(otherPath)).Verifiable();

            var sink = new WallpaperSink(new WallpaperSinkOptions(), desktopMock.Object);
            sink.Consume(filePath);
            sink.Consume(filePath);
            sink.Consume(otherPath);

            desktopMock.Verify(it => it.SetBackground(filePath), Times.Once);
        }

        [Test]
        public void Should_prepend_configured_directory_to_relative_paths()
        {
            var filePath = @"relative\path\batman.jpg";
            var desktopMock = new Mock<IDesktop>();
            desktopMock.Setup(it => it.SetBackground(It.IsAny<string>())).Verifiable();

            var relativeTo = @"D:\wayne\enterprises";
            var sink = new WallpaperSink(new WallpaperSinkOptions() { 
                RelativeTo = relativeTo
            }, desktopMock.Object);

            sink.Consume(filePath);

            var expected = Path.Combine(relativeTo, filePath);

            desktopMock.Verify(it => it.SetBackground(expected));
        }

        
        [Test]
        public void Should_prepend_configured_directory_to_relative_paths_for_each_display()
        {
            var input = JObject.Parse(@"{""0"": ""relative\\path\\batman.jpg"", ""1"": ""relative\\path\\alfred.jpg""}");

            var expectedFilePathByScreen = new Dictionary<int, string>()
            {
                { 0, @"D:\wayne\enterprises\relative\path\batman.jpg" },
                { 1, @"D:\wayne\enterprises\relative\path\alfred.jpg" },
            };

            var desktopMock = new Mock<IDesktop>();
            desktopMock.Setup(it => it.SetBackgrounds(expectedFilePathByScreen)).Verifiable();

            var relativeTo = @"D:\wayne\enterprises";
            var sink = new WallpaperSink(new WallpaperSinkOptions() { RelativeTo = relativeTo }, desktopMock.Object);
            sink.Consume(input);

            desktopMock.Verify();
        }
    }
}
#endif
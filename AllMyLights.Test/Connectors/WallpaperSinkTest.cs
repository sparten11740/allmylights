using AllMyLights.Connectors.Sinks.Wallpaper;
using AllMyLights.Models.OpenRGB;
using AllMyLights.Platforms;
using Moq;
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

            var sink = new WallpaperSink(new WallpaperSinkOptions() { 
                RelativeTo = @"D:\wayne\enterprises"
            }, desktopMock.Object);

            sink.Consume(filePath);

            desktopMock.Verify(it => it.SetBackground(@"D:\wayne\enterprises\relative\path\batman.jpg"));
        }
    }
}
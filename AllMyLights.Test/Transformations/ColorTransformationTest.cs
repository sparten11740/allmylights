using System;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Models.Transformations;
using AllMyLights.Transformations;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class ColorTransformationTest : ReactiveTest
    {

        [Test]
        public void Should_transform_color()
        {
            var black = "#000000";
            Ref<Color> expectedColor = new Ref<Color>(Color.FromArgb(255, 0, 0, 0));
            var source = Observable.Return(black);

            var transformation = new ColorTransformation(new ColorTransformationOptions());

            var scheduler = new TestScheduler();
            

            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<Ref<Color>>>[] {
                OnNext(10, expectedColor),
                OnCompleted<Ref<Color>>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [Test]
        public void Should_transform_color_using_given_channel_layout()
        {
            var black = "#FF0000";
            var expectedColor = new Ref<Color>(Color.FromArgb(255, 0, 255, 0));
            var source = Observable.Return(black);

            var transformation = new ColorTransformation(new ColorTransformationOptions()
            {
                ChannelLayout = "GRB"
            });

            var scheduler = new TestScheduler();


            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<Ref<Color>>>[] {
                OnNext(10, expectedColor),
                OnCompleted<Ref<Color>>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [Test]
        public void Should_throw_argument_exception_for_invalid_input()
        {
            var source = Observable.Return(new object());

            var transformation = new ColorTransformation(new ColorTransformationOptions());

            var scheduler = new TestScheduler();


            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );


            Recorded<Notification<Ref<Color>>> first = actual.Messages.First();
            Equals(NotificationKind.OnError, first.Value.Kind);
            Equals(typeof(ArgumentException), first.Value.Exception.GetType());
        }
    }
}
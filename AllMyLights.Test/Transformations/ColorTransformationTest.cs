using System;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Transformations;
using AllMyLights.Transformations.Color;
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

        [TestCase("wrong type")]
        [TestCase("exception on decode")]
        public void Should_not_break_for_invalid_input(string testCase)
        {
            var input = testCase switch
            {
                "wrong type" => new object(),
                "exception on decode" => "#kennadecode"
            };

            var source = Observable.Return(input);

            var transformation = new ColorTransformation(new ColorTransformationOptions());

            var scheduler = new TestScheduler();


            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 0,
              disposed: 100
            );


            var expected = new Recorded<Notification<Ref<Color>>>[] {
                OnCompleted<Ref<Color>>(1)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
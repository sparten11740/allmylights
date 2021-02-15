using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Common;
using AllMyLights.Transformations;
using AllMyLights.Transformations.Expression;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class ExpressionTransformationTest : ReactiveTest
    {

        [Test]
        public void Should_expose_input_value_in_context()
        {
            var input = "Bruce";
            var source = Observable.Return(input);

            var options = new ExpressionTransformationOptions() {
                Expression = "\"Hello \" + value"
            };

            var transformation = new ExpressionTransformation<string>(options);

            var scheduler = new TestScheduler();

            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<string>>[] {
                OnNext(10, "Hello Bruce"),
                OnCompleted<string>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [TestCase("Bluish", 0)]
        [TestCase("Bluish", 1)]
        [TestCase("Bluish", 2)]
        [TestCase("Greenish", 3)]
        [TestCase("Greenish", 4)]
        [TestCase("Greenish", 5)]
        [TestCase("Redish", 6)]
        [TestCase("Redish", 7)]
        [TestCase("Redish", 8)]
        public void Should_allow_expressions_on_non_primitive_values(string expectation, int index)
        {
            var colors = new Color[]
            {
                Color.AliceBlue,
                Color.Blue,
                Color.BlueViolet,
                Color.Green,
                Color.DarkGreen,
                Color.DarkOliveGreen,
                Color.Red,
                Color.DarkRed,
                Color.DarkOrange
            };


            var input = new Ref<Color>(colors[index]);
            var source = Observable.Return(input);

            var options = new ExpressionTransformationOptions()
            {
                Expression = "value.B > value.R && value.B > value.G ? \"Bluish\" : (value.G > value.B && value.G > value.R ? \"Greenish\" : \"Redish\")"
            };

            var transformation = new ExpressionTransformation<string>(options);

            var scheduler = new TestScheduler();

            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<string>>[] {
                OnNext(10, expectation),
                OnCompleted<string>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [Test]
        public void Should_not_break_for_invalid_input()
        {
            var input = "some string";
            var source = Observable.Return(input);
            var expressionExpectingColor = "value.B > value.R && value.B > value.G ? \"Bluish\" : \"Something else\"";

            var options = new ExpressionTransformationOptions()
            {
                Expression = expressionExpectingColor
            };

            var transformation = new ExpressionTransformation<string>(options);

            var scheduler = new TestScheduler();

            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<string>>[] {
                OnCompleted<string>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
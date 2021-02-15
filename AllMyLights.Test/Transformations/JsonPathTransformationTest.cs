using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Transformations.JsonPath;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class JsonPathTransformationTest : ReactiveTest
    {

        [Test]
        public void Should_extract_deeply_nested_value()
        {
            var json = "{\"data\":{\"livingRoom\":{\"color\": \"red\"}}}";
            var source = Observable.Return(json);

            var options = new JsonPathTransformationOptions() { Expression = "$.data.livingRoom.color" };
            var transformation = new JsonPathTransformation<string>(options);

            var scheduler = new TestScheduler();
            

            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 10,
              disposed: 100
            );

            var expected = new Recorded<Notification<string>>[] {
                OnNext(10, "red"),
                OnCompleted<string>(10)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [TestCase("wrong type")]
        [TestCase("malformed json")]
        [TestCase("no match")]
        public void Should_not_break_for_invalid_input(string testCase)
        {
            var input = testCase switch {
                "no match" => "{\"POWER\": \"OFF\"}",
                "malformed json" => "{\"POWER\":",
                "wrong type" => new object(),
            };

            var source = Observable.Return(input);

            var options = new JsonPathTransformationOptions() { Expression = "$.data.livingRoom.color" };
            var transformation = new JsonPathTransformation<string>(options);

            var scheduler = new TestScheduler();


            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 0,
              disposed: 100
            );


            var expected = new Recorded<Notification<string>>[] {
                OnCompleted<string>(1)
            };

            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }
    }
}
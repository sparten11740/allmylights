using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Models.Transformations;
using AllMyLights.Transformations;
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

        [Test]
        public void Should_throw_argument_exception_for_invalid_input()
        {
            var source = Observable.Return(new object());

            var options = new JsonPathTransformationOptions() { Expression = "$.data.livingRoom.color" };
            var transformation = new JsonPathTransformation<string>(options);

            var scheduler = new TestScheduler();


            var actual = scheduler.Start(
              () => transformation.GetOperator()(source),
              created: 0,
              subscribed: 0,
              disposed: 100
            );



            Recorded<Notification<string>> first = actual.Messages.First();
            Equals(NotificationKind.OnError, first.Value.Kind);
            Equals(typeof(ArgumentException), first.Value.Exception.GetType());
        }
    }
}
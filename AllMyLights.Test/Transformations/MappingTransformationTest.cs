using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AllMyLights.Models.Transformations;
using AllMyLights.Transformations;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace AllMyLights.Test
{
    public class MappingTransformationTest : ReactiveTest
    {
        [Test]
        public void Should_match_simple_string()
        {
            new Validator()
                .AddMapping("red", "blue")
                .StartWith("red")
                .ExpectOutput("blue")
                .Verify();
        }

        [Test]
        public void Should_match_regex_and_replace_substitutions()
        {
            new Validator()
                .AddMapping("^([a-zA-Z]+)\\-", "bruce-$1")
                .StartWith("wayne-enterprises")
                .ExpectOutput("bruce-wayne")
                .Verify();
        }

        [Test]
        public void Should_return_input_value_on_miss()
        {
            new Validator()
                .AddMapping("bruce-wayne", "blue")
                .StartWith("red")
                .ExpectOutput("red")
                .Verify();
        }

        [Test]
        public void Should_fail_on_miss_if_configured_to()
        {
            new Validator()
                .AddMapping("bruce-wayne", "blue")
                .StartWith("red")
                .FailOnMiss()
                .ExpectFailure()
                .Verify();
        }

        private class Validator
        {
            private MappingTransformationOptions Options = new MappingTransformationOptions()
            {
                Mappings = new List<MappingTransformationOptions.Mapping>() { }
            };

            private string Input { get; set; }
            private string Output { get; set; }
            private bool ShouldFail { get; set; } = false;

            public Validator StartWith(string input)
            {
                Input = input;
                return this;
            }

            public Validator AddMapping(string match, string substitute)
            {
                Options.Mappings.Add(new MappingTransformationOptions.Mapping() {
                        From = match,
                        To = substitute
                });
                return this;
            }

            public Validator FailOnMiss()
            {
                Options.FailOnMiss = true;
                return this;
            }

            public Validator ExpectOutput(string output)
            {
                Output = output;
                return this;
            }

            public Validator ExpectFailure()
            {
                ShouldFail = true;
                return this;
            }

            public void Verify()
            {
                var source = Observable.Return(Input);

                var scheduler = new TestScheduler();
                var transformation = new MappingTransformation(Options);


                if (ShouldFail)
                {
                    ReactiveAssert.Throws<Exception>(() => {
                        transformation.GetOperator()(source).Subscribe();
                    });
                }
                else
                {

                    var actual = scheduler.Start(
                      () => transformation.GetOperator()(source),
                      created: 0,
                      subscribed: 10,
                      disposed: 100
                    );
                    var expected = new Recorded<Notification<string>>[] {
                    OnNext(10, Output),
                    OnCompleted<string>(10)
                    };
                    ReactiveAssert.AreElementsEqual(expected, actual.Messages);

                }

            }
        }

    }
}
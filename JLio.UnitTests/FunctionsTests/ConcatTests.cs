using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class ConcatTests
    {
        private IExecutionContext executeOptions;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeOptions = ExecutionContext.CreateDefault();
        }

        [TestCase("=concat()", "{}", "")]
        [TestCase("=concat('a','b','c')", "{}", "abc")]
        [TestCase("= concat ( 'a' , concat('a','b','c') ,  'concat('a','b','c')' )", "{}", "aabcconcat('a','b','c')")]
        [TestCase("=concat($.a, 'b', $.c)", "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
        [TestCase("=concat($.a, $.b, $.c)",
            "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
        public void ScriptTest(string function, string data, string resultValue)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
            Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.ToString());
        }

        [TestCase("=concat($.a, $.b, $.c)555",
            "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
        public void ScriptTestWithWarnings(string function, string data, string resultValue)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
            Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.ToString());
        }

        [Test]
        public void CanBeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Add(new Concat("'a'", "'b'"))
                    .OnPath("$.result")
                ;
            var result = script.Execute(new JObject());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.result").Type, JTokenType.Null);
            Assert.AreEqual("ab", result.Data.SelectToken("$.result").ToString());
        }
        
        [Test]
        public void CanbeUsedInFluentApi_Set()
        {
            var script = new JLioScript()
                .Set(new Concat("'a'", "'b'"))
                .OnPath("$.demo");
            var result = script.Execute(JToken.Parse("{\"demo\":{\"pageIndex\":5,\"shouldBeRemoved\":true}}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.demo").Type, JTokenType.Null);
            Assert.AreEqual("ab", result.Data.SelectToken("$.demo").ToString());
        }
    }
}
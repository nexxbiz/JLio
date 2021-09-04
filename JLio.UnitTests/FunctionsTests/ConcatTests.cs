using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class ConcatTests
    {
        private JLioExecutionOptions executeOptions;
        private JLioParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = JLioParseOptions.CreateDefault();
            executeOptions = JLioExecutionOptions.CreateDefault();
        }

        [TestCase("=concat()", "{}", "")]
        [TestCase("=concat('a','b','c')", "{}", "abc")]
        [TestCase("=concat($.a, 'b', $.c)", "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
        [TestCase("=concat($.a, $.b, $.c)",
            "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
        public void ScriptTest(string function, string data, string resultValue)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsFalse(string.IsNullOrEmpty(executeOptions.Logger.LogText));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
            Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.ToString());
        }

        [Test]
        public void CanbeUsedInFluentApi()
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
    }
}
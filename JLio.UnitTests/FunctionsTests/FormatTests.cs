using System.Linq;
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
    public class FormatTests
    {
        private IExecutionContext executeContext;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeContext = ExecutionContext.CreateDefault();
        }

        [TestCase("=format($.source,'dd-MM-YYYY')", "{\"source\" : \"2022-01-10T21:15:15.113Z\"}", "{\"new\": 1 }")]
        public void ScriptTestWithPath(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=format()", "{\"result\" : [1,2]}")]
        public void WillReturnErrorFalse(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result[*]\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
        }

        [Test]
        public void CanBeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Set(new Format("$.demo", "dd-MM-yyyy"))
                    .OnPath("$.id")
                    .Set(new Format("newer"))
                    .OnPath("$.demo")
                ;
            var result = script.Execute(JToken.Parse("{\"demo\" : 1}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }
    }
}
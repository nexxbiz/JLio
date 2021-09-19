using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class DatetimeFunctionTests
    {
        private ExecutionOptions executeOptions;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeOptions = ExecutionOptions.CreateDefault();
        }

        [TestCase("=datetime(now)", "{}")]
        [TestCase("=datetime(UTC)", "{}")]
        [TestCase("=datetime(startOfDay)", "{}")]
        [TestCase("=datetime(startOfDayUTC)", "{}")]
        [TestCase("=datetime()", "{}")]
        [TestCase("=datetime('dd-MM-yyyy HH:mm')", "{}")]
        [TestCase("=datetime(UTC, 'dd-MM-yy HH:mm')", "{}")]
        [TestCase("=datetime($.dateSelection, $.format)",
            "{\"dateSelection\":\"UTC\",\"format\":\"HH:mm on dd-MM-yy\"}")]
        public void ScriptTest(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
        }

        [Test]
        public void CanbeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Add(new Datetime("UTC", "'dd-MM-yyyy HH:mm:ss'"))
                    .OnPath("$.date")
                    .Add(new Datetime("'HH:mm:ss'"))
                    .OnPath("$.now")
                ;
            var result = script.Execute(new JObject());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.date").Type, JTokenType.Null);
            Assert.AreNotEqual(result.Data.SelectToken("$.now").Type, JTokenType.Null);
        }
    }
}
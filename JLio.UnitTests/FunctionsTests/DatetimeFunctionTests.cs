using JLio.Client;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using JLio.Commands.Builders;
using JLio.Functions;

namespace JLio.UnitTests.FunctionsTests
{
    public class DatetimeFunctionTests
    {
        private JLioExecutionOptions executeOptions;
        private JLioParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = JLioParseOptions.CreateDefault();
            executeOptions = JLioExecutionOptions.CreateDefault();
        }

        [TestCase("=datetime(UTC)", "{}")]
        [TestCase("=datetime()", "{}")]
        [TestCase("=datetime('dd-MM-yyyy HH:mm')", "{}")]
        [TestCase("=datetime(UTC, 'dd-MM-yy HH:mm')", "{}")]
        [TestCase("=datetime($.dateSelection, $.format)",
            "{\"dateSelection\":\"UTC\",\"format\":\"HH:mm on dd-MM-yy\"}")]
        public void scriptTest(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
        }

        [Test]
        public void CanbeUsedInFluentApi()
        {
            var script = new JLioScript()
                 .Add(new DatetimeFunction("UTC","'dd-MM-yyyy HH:mm:ss'"))
                 .OnPath("$.date")
                 .Add(new DatetimeFunction("'HH:mm:ss'"))
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
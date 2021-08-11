using JLio.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

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
            var result = JLioScript.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
        }
    }
}
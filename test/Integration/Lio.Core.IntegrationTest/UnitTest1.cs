using Xunit;

namespace Lio.Core.IntegrationTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // var runner = new ScriptRunner(new NewtonsoftMutator(), null, null);
            //
            // runner.RunScriptAsync(new ScriptDefinition()
            // {
            //     new Add("", true);
            // }, new ScriptInput(), CancellationToken.None);

            //var runner = new ScriptRunner(null, null, null);

            //var scripttext = "[]";
            //var script = JLio2Convert.Parse(scripttext);

            //var scriptRunner =
            //    new ScriptRunner(null, null); // gets an provider injected that can provide different imutators, of item fetchers

            //var data = JToken.Parse(
            //    "{\r\n  \"myString\": \"demo2\",\r\n  \"myNumber\": 2.2,\r\n  \"myInteger\": 20,\r\n  \"myObject\": {\r\n    \"myObject\": {\"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]},\r\n    \"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]\r\n  },\r\n  \"myArray\": [\r\n    2,\r\n    20,\r\n    200,\r\n    2000\r\n  ],\r\n  \"myBoolean\": true,\r\n  \"myNull\": null\r\n}");

            //var options = ScriptRunnerOptions.SetOptions(new NewtonsoftMutator(), null);


            //var result = scriptRunner.RunScriptAsync(script, data, options);

            //or

            //var result2 = script.RunScriptAsync(Scriptrunner, data, options);
        }
    }
}
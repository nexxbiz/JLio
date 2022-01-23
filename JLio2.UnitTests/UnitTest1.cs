using NUnit.Framework;

namespace JLio2.UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        //var scripttext = "[]";
        //var script = JLioConvert.JsonParse(scripttext);

        //var scriptRunner = ScriptRunner // gets an provider injected that can provide different imutators, of item fetchers

        //var data = JToken.Parse(
        //    "{\r\n  \"myString\": \"demo2\",\r\n  \"myNumber\": 2.2,\r\n  \"myInteger\": 20,\r\n  \"myObject\": {\r\n    \"myObject\": {\"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]},\r\n    \"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]\r\n  },\r\n  \"myArray\": [\r\n    2,\r\n    20,\r\n    200,\r\n    2000\r\n  ],\r\n  \"myBoolean\": true,\r\n  \"myNull\": null\r\n}");

        //var options  = new LioOptions(NewtonsoftMutator,pathfetcher,)
        //                    .Add(NewtonsoftMutator)

        //scriptRunner.ExecuteAsync(script, data, options);

        //var result =
        //    script.RunAsync(data); // default (system.text.json)

        //var result =
        //    script.RunAsync(data, options); // default (system.text.json); //execute is going to be a extention method on the script with a newtonsoft implementation, there will e a system.textjson implementation as well 
    }
}
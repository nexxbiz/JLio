using System.Linq;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class IndirectTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;
    private JToken testData;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executionContext = ExecutionContext.CreateDefault();
        
        // Create test data inline for simplicity
        testData = JToken.Parse(@"{
            ""paths"": {
                ""userPath"": ""$.data.users[0]"",
                ""emailPath"": ""$.data.users[0].email"",
                ""invalidPath"": ""$.nonexistent.path"",
                ""emptyPath"": """",
                ""numberPath"": 123,
                ""configPath"": ""$.config"",
                ""sourceDataPath"": ""$.sourceData.items[0]"",
                ""targetPath"": ""$.target"",
                ""removePath"": ""$.data.users[0].tempField""
            },
            ""data"": {
                ""users"": [
                    {
                        ""id"": 1,
                        ""name"": ""Alice Johnson"",
                        ""email"": ""alice@example.com"",
                        ""tempField"": ""to be removed""
                    }
                ]
            },
            ""config"": {
                ""currentUserPath"": ""$.data.users[0]"",
                ""dynamicProperty"": ""email"",
                ""isEnabled"": true
            },
            ""sourceData"": {
                ""items"": [
                    {
                        ""title"": ""Source Item"",
                        ""value"": 42
                    }
                ]
            },
            ""target"": {
                ""existing"": ""data""
            }
        }");
    }

    #region Basic Indirect Function Tests

    [Test]
    public void Indirect_WithValidPath_ShouldReturnCorrectToken()
    {
        var indirect = new Indirect("$.paths.userPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.First().SelectToken("name")?.Value<string>());
        Assert.AreEqual(1, result.Data.First().SelectToken("id")?.Value<int>());
    }

    [Test]
    public void Indirect_WithPathToStringValue_ShouldReturnStringValue()
    {
        var indirect = new Indirect("$.paths.emailPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("alice@example.com", result.Data.First().Value<string>());
    }

    [Test]
    public void Indirect_WithNonExistentPath_ShouldFail()
    {
        var indirect = new Indirect("$.paths.nonExistentPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Indirect_WithPathToNonString_ShouldFail()
    {
        var indirect = new Indirect("$.paths.numberPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Indirect_WithPathToEmptyString_ShouldFail()
    {
        var indirect = new Indirect("$.paths.emptyPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Indirect_WithInvalidJSONPath_ShouldFail()
    {
        var indirect = new Indirect("$.paths.invalidPath");
        var result = indirect.Execute(testData, testData, executionContext);

        Assert.IsFalse(result.Success);
    }

    #endregion

    #region Indirect in Script Tests with ADD Command

    [Test]
    public void Indirect_InScript_Add_ShouldWork()
    {
        var script = "[{'path':'$.result','value':'=indirect($.paths.emailPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("alice@example.com", result.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_Add_WithComplexData_ShouldWork()
    {
        var script = "[{'path':'$.result','value':'=indirect($.paths.userPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.result.name")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_Add_WithConfigObject_ShouldWork()
    {
        var script = "[{'path':'$.configCopy','value':'=indirect($.paths.configPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.data.users[0]", result.Data.SelectToken("$.configCopy.currentUserPath")?.Value<string>());
        Assert.AreEqual(true, result.Data.SelectToken("$.configCopy.isEnabled")?.Value<bool>());
    }

    #endregion

    #region Indirect in Script Tests with SET Command

    [Test]
    public void Indirect_InScript_Set_ShouldWork()
    {
        var script = "[{'path':'$.config.dynamicProperty','value':'=indirect($.paths.emailPath)','command':'set'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("alice@example.com", result.Data.SelectToken("$.config.dynamicProperty")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_Set_WithUserObject_ShouldWork()
    {
        var script = "[{'path':'$.target','value':'=indirect($.paths.userPath)','command':'set'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.target.name")?.Value<string>());
        Assert.AreEqual(1, result.Data.SelectToken("$.target.id")?.Value<int>());
        // Original target.existing should be replaced
        Assert.IsNull(result.Data.SelectToken("$.target.existing"));
    }

    #endregion

    #region Indirect in Script Tests with PUT Command

    [Test]
    public void Indirect_InScript_Put_ShouldWork()
    {
        var script = "[{'path':'$.newData','value':'=indirect($.paths.userPath)','command':'put'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.newData.name")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_Put_OverwriteExisting_ShouldWork()
    {
        var script = "[{'path':'$.target','value':'=indirect($.paths.configPath)','command':'put'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.data.users[0]", result.Data.SelectToken("$.target.currentUserPath")?.Value<string>());
        Assert.AreEqual(true, result.Data.SelectToken("$.target.isEnabled")?.Value<bool>());
    }

    #endregion

    #region Indirect in Script Tests with COPY Command

    [Test]
    public void Indirect_InScript_Copy_ShouldWork()
    {
        var script = "[{'fromPath':'=indirect($.paths.userPath)','toPath':'$.userCopy','command':'copy'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.userCopy.name")?.Value<string>());
        // Original should still exist
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.data.users[0].name")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_Copy_WithIndirectTarget_ShouldWork()
    {
        var script = "[{'fromPath':'$.config','toPath':'=indirect($.paths.targetPath)','command':'copy'}]".Replace("'","\"");
        var jlioscript = JLioConvert.Parse(script, parseOptions);
        var result = jlioscript.Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.data.users[0]", result.Data.SelectToken("$.target.currentUserPath")?.Value<string>());
        // Original config should still exist
        Assert.AreEqual("$.data.users[0]", result.Data.SelectToken("$.config.currentUserPath")?.Value<string>());
    }

    #endregion

    #region Indirect in Script Tests with MOVE Command

    [Test]
    public void Indirect_InScript_Move_ShouldWork()
    {
        var script = "[{'fromPath':'=indirect($.paths.sourceDataPath)','toPath':'$.movedData','command':'move'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Source Item", result.Data.SelectToken("$.movedData.title")?.Value<string>());
        Assert.AreEqual(42, result.Data.SelectToken("$.movedData.value")?.Value<int>());
        // Original should be removed
        Assert.IsNull(result.Data.SelectToken("$.sourceData.items[0]"));
    }

    [Test]
    public void Indirect_InScript_Move_WithIndirectTarget_ShouldWork()
    {
        var script = "[{'fromPath':'$.sourceData','toPath':'=indirect($.paths.targetPath)','command':'move'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Source Item", result.Data.SelectToken("$.target.items[0].title")?.Value<string>());
        // Original sourceData should be removed
        Assert.IsNull(result.Data.SelectToken("$.sourceData"));
    }

    #endregion

    #region Indirect in Script Tests with REMOVE Command

    [Test]
    public void Indirect_InScript_Remove_ShouldWork()
    {
        var script = "[{'path':'=indirect($.paths.removePath)','command':'remove'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        // Field should be removed
        Assert.IsNull(result.Data.SelectToken("$.data.users[0].tempField"));
        // Other fields should remain
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.data.users[0].name")?.Value<string>());
    }

    #endregion

    #region Complex Indirect Scenarios

    [Test]
    public void Indirect_InScript_ChainedOperations_ShouldWork()
    {
        var script = @"[
            {'path':'$.step1','value':'=indirect($.paths.emailPath)','command':'add'},
            {'path':'$.step2','value':'=indirect($.paths.userPath)','command':'add'},
            {'fromPath':'=indirect($.paths.configPath)','toPath':'$.step3','command':'copy'}
        ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("alice@example.com", result.Data.SelectToken("$.step1")?.Value<string>());
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.step2.name")?.Value<string>());
        Assert.AreEqual("$.data.users[0]", result.Data.SelectToken("$.step3.currentUserPath")?.Value<string>());
    }

    [Test]
    public void Indirect_InScript_WithArrays_ShouldWork()
    {
        // Add array path to test data for this test
        var testDataWithArray = JToken.Parse(testData.ToString());
        testDataWithArray["paths"]["userArrayPath"] = "$.data.users";
        
        var script = "[{'path':'$.allUsers','value':'=indirect($.paths.userArrayPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testDataWithArray, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.allUsers[0].name")?.Value<string>());
    }


    #endregion

    #region Error Handling Tests

    [Test]
    public void Indirect_InScript_WithInvalidPath_ShouldFail()
    {
        var script = "[{'path':'$.result','value':'=indirect($.paths.invalidPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Indirect_InScript_WithNonStringPath_ShouldFail()
    {
        var script = "[{'path':'$.result','value':'=indirect($.paths.numberPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Indirect_InScript_WithEmptyPath_ShouldFail()
    {
        var script = "[{'path':'$.result','value':'=indirect($.paths.emptyPath)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);
        
        Assert.IsFalse(result.Success);
    }

    #endregion
}
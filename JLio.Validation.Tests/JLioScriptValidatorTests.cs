using JLio.Validation;
using JLio.Validation.Commands;
using JLio.Validation.Validation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JLio.Validation.Tests;

public class JLioScriptValidatorTests
{
    [Fact]
    public void Validate_ReturnsError_WhenScriptIsNotArray()
    {
        // Arrange
        var script = JToken.Parse(@"""not an array""");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Issues);
        Assert.Equal("Script.NotArray", result.Issues[0].Code);
        Assert.Equal(IssueSeverity.Error, result.Issues[0].Severity);
        Assert.Equal("", result.Issues[0].JsonPointer);
    }

    [Fact]
    public void Validate_Add_MinimalValid()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.myObject.newProperty"", ""value"": ""x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_Add_FunctionValue_Allowed()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.myObject.newProperty"", ""value"": ""=newGuid()"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_Add_MissingPath_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""value"": ""x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Prop.Missing" && i.Message.Contains("path"));
    }

    [Fact]
    public void Validate_Add_EmptyPath_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": """", ""value"": ""x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Path.Empty");
    }

    [Fact]
    public void Validate_Add_InvalidPath_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""myObject.noDollar"", ""value"": ""x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Path.Invalid" && i.Message.Contains("must start with '$'"));
    }

    [Fact]
    public void Validate_Add_FilterPath_WarnsPartialSupport()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.items[?(@.id > 5)].newProp"", ""value"": ""x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success); // Should still succeed despite warning
        Assert.Contains(result.Issues, i => i.Code == "Path.FilterPartialSupport" && i.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void Validate_UnknownCommand_Disallowed_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""unknown"", ""path"": ""$.x"", ""value"": 1 }
        ]");
        var options = new JLioValidationOptions { AllowUnknownCommands = false };
        var validator = JLioScriptValidator.CreateDefault(options);

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Command.Unknown" && i.Severity == IssueSeverity.Error);
    }

    [Fact]
    public void Validate_UnknownCommand_Allowed_IsWarning()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""unknown"", ""path"": ""$.x"", ""value"": 1 }
        ]");
        var options = new JLioValidationOptions { AllowUnknownCommands = true };
        var validator = JLioScriptValidator.CreateDefault(options);

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success); // Should succeed with warning
        Assert.Contains(result.Issues, i => i.Code == "Command.Unknown" && i.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void Validate_CommandPropertyMissing_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""path"": ""$.x"", ""value"": 1 }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Command.MissingOrInvalid");
    }

    [Fact]
    public void Validate_Add_ValueMissing_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.x"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Prop.Missing" && i.Message.Contains("value"));
    }

    [Fact]
    public void Validate_Generic_PathSanity_OnUnknownCommand()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""unknown"", ""path"": ""invalid-path"", ""value"": 1 }
        ]");
        var options = new JLioValidationOptions { AllowUnknownCommands = true };
        var validator = JLioScriptValidator.CreateDefault(options);

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success); // Should fail due to invalid path
        Assert.Contains(result.Issues, i => i.Code == "Path.Invalid");
        Assert.Contains(result.Issues, i => i.Code == "Command.Unknown" && i.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void Validate_IssuesContainJsonPointerAndIndex()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.valid"", ""value"": ""x"" },
            { ""command"": ""add"", ""path"": """", ""value"": ""y"" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        var pathEmptyIssue = result.Issues.First(i => i.Code == "Path.Empty");
        Assert.Equal("/1/path", pathEmptyIssue.JsonPointer);
        Assert.Equal(1, pathEmptyIssue.CommandIndex);
    }

    [Fact]
    public void Validate_Add_FunctionExpression_Invalid_EmptyAfterEquals()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.x"", ""value"": ""="" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Function.Invalid" && i.Message.Contains("empty after"));
    }

    [Fact]
    public void Validate_Add_FunctionExpression_Invalid_WhitespaceAfterEquals()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.x"", ""value"": ""=   "" }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Function.Invalid" && i.Message.Contains("whitespace only"));
    }

    [Fact]
    public void Validate_ComplexValidScript_WithExamples()
    {
        // Example from the spec
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.myObject.newProp"", ""value"": ""new value"" },
            { ""command"": ""unknown"", ""path"": ""$.x"", ""value"": 1 }
        ]");

        var validator = JLioScriptValidator.CreateDefault(new JLioValidationOptions
        {
            AllowUnknownCommands = true
        });

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success); // Should succeed since unknown commands are allowed
        Assert.Single(result.Issues);
        Assert.Equal("Command.Unknown", result.Issues[0].Code);
        Assert.Equal(IssueSeverity.Warning, result.Issues[0].Severity);
        Assert.Equal(1, result.Issues[0].CommandIndex);
    }

    [Fact]
    public void Validate_Add_ComplexJsonPathExpressions()
    {
        // Arrange
        var script = JToken.Parse(@"[
            { ""command"": ""add"", ""path"": ""$.store.book[*].author"", ""value"": ""Unknown"" },
            { ""command"": ""add"", ""path"": ""$..price"", ""value"": 0 },
            { ""command"": ""add"", ""path"": ""$.arrays.numbers[2]"", ""value"": 42 }
        ]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_CommandNotObject_IsError()
    {
        // Arrange
        var script = JToken.Parse(@"[""not an object""]");
        var validator = JLioScriptValidator.CreateDefault();

        // Act
        var result = validator.Validate(script);

        // Assert
        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Code == "Command.NotObject");
    }
}
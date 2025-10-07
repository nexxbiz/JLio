using JLio.Validation.Paths;
using JLio.Validation.Validation;
using Xunit;

namespace JLio.Validation.Tests;

public class BasicJsonPathValidatorTests
{
    private readonly BasicJsonPathValidator _validator = new();

    [Fact]
    public void Validate_ValidSimplePath_ReturnsValid()
    {
        // Arrange
        var path = "$.store.book";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidArrayPath_ReturnsValid()
    {
        // Arrange
        var path = "$.store.book[0].title";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidWildcardPath_ReturnsValid()
    {
        // Arrange
        var path = "$.store.book[*].author";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidRecursiveDescentPath_ReturnsValid()
    {
        // Arrange
        var path = "$..price";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidRecursiveDescentWithProperty_ReturnsValid()
    {
        // Arrange
        var path = "$.store..book"; // Valid recursive descent

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ValidFilterPath_ReturnsValidWithWarning()
    {
        // Arrange
        var path = "$.store.book[?(@.price < 10)]";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.FilterPartialSupport", issues[0].Code);
        Assert.Equal(IssueSeverity.Warning, issues[0].Severity);
    }

    [Fact]
    public void Validate_ValidQuotedPropertyPath_ReturnsValid()
    {
        // Arrange
        var path = "$['property-with-dashes']";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_EmptyPath_ReturnsError()
    {
        // Arrange
        var path = "";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Empty", issues[0].Code);
        Assert.Equal(IssueSeverity.Error, issues[0].Severity);
    }

    [Fact]
    public void Validate_WhitespaceOnlyPath_ReturnsError()
    {
        // Arrange
        var path = "   ";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Invalid", issues[0].Code);
        Assert.Contains("whitespace only", issues[0].Message);
    }

    [Fact]
    public void Validate_PathNotStartingWithDollar_ReturnsError()
    {
        // Arrange
        var path = "store.book";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Invalid", issues[0].Code);
        Assert.Contains("must start with '$'", issues[0].Message);
    }

    [Fact]
    public void Validate_UnbalancedBrackets_ReturnsError()
    {
        // Arrange
        var path = "$.store.book[0.title";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Invalid", issues[0].Code);
        Assert.Contains("unbalanced", issues[0].Message);
    }

    [Fact]
    public void Validate_UnbalancedParentheses_ReturnsError()
    {
        // Arrange
        var path = "$.store.book[?(@.price < 10]";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Invalid", issues[0].Code);
        Assert.Contains("unbalanced", issues[0].Message);
    }

    [Fact]
    public void Validate_EmptySegments_ReturnsError()
    {
        // Arrange
        var path = "$.store...book"; // Too many dots (three dots is invalid)

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.False(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.Invalid", issues[0].Code);
        Assert.Contains("empty segments", issues[0].Message);
    }

    [Theory]
    [InlineData("$.store.book[?(@.price > 5)].title")]
    [InlineData("$.items[?(@.category == 'fiction')]")]
    [InlineData("$..book[?(@.isbn)]")]
    public void Validate_MultipleFilterExpressions_EachGeneratesWarning(string path)
    {
        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.All(issues, i => Assert.Equal("Path.FilterPartialSupport", i.Code));
        Assert.All(issues, i => Assert.Equal(IssueSeverity.Warning, i.Severity));
    }

    [Fact]
    public void Validate_QuotedStringsInFilter_HandlesCorrectly()
    {
        // Arrange - quotes inside filter should be handled correctly
        var path = "$.books[?(@.title == 'Lord of the Rings')]";

        // Act
        var (isValid, issues) = _validator.Validate(path, "/0/path", 0);

        // Assert
        Assert.True(isValid);
        Assert.Single(issues);
        Assert.Equal("Path.FilterPartialSupport", issues[0].Code);
    }
}
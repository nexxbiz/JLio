using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.ETL;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Newtonsoft.Json;

namespace JLio.UnitTests.FunctionsTests;

public class ScriptPathTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;
    private string testDataPath;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText().RegisterETL();
        executionContext = ExecutionContext.CreateDefault();
        
        // Get the test data directory path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        testDataPath = Path.Combine(assemblyDirectory, "TestData", "ScriptPathTests");
    }

    [Test]
    public void PathFunction_WithNoArguments_ReturnsCurrentTokenPath()
    {
        var testCase = LoadTestCase("basic-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithRelativePathArgument_ReturnsAbsolutePath()
    {
        var testCase = LoadTestCase("relative-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_ComplexExample_WithSecondPath()
    {
        var testCase = LoadTestCase("complex-example.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_AtRootLevel_ReturnsRootPath()
    {
        var testCase = LoadTestCase("root-level.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithParentPathIndicator_ReturnsCurrentItemPath()
    {
        var testCase = LoadTestCase("parent-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithAtSymbolOnly_ReturnsCurrentPath()
    {
        // Arrange - using inline test case structure
        var testCase = new TestCaseData
        {
            Data = JObject.Parse(@"{
                ""sample"": {
                    ""myArray"": [
                        { ""myItem"": ""value1"" }
                    ]
                }
            }"),
            Script = @"[
                {
                    ""path"": ""$.sample.myArray[*].selfPath"",
                    ""value"": ""=path(@)"",
                    ""command"": ""add""
                }
            ]"
        };

        // Act
        var result = JLioConvert.Parse(testCase.Script, parseOptions).Execute(testCase.Data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.sample.myArray[0]", result.Data.SelectToken("$.sample.myArray[0].selfPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange - using inline test case structure
        var testCase = new TestCaseData
        {
            Data = JObject.Parse(@"{
                ""sample"": {
                    ""myArray"": [
                        { ""myItem"": ""value1"" }
                    ]
                }
            }"),
            Script = @"[
                {
                    ""path"": ""$.sample.myArray[*].absolutePath"",
                    ""value"": ""=path($.some.absolute.path)"",
                    ""command"": ""add""
                }
            ]"
        };

        // Act
        var result = JLioConvert.Parse(testCase.Script, parseOptions).Execute(testCase.Data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.some.absolute.path", result.Data.SelectToken("$.sample.myArray[0].absolutePath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithEmptyStringArgument_ReturnsCurrentPath()
    {
        // Arrange - using proper JSON escaping like other working tests
        var script = @"[{""path"":""$.sample.myArray[*].currentPath"",""value"":""=path('')"",""command"":""add""}]";
        
        var data = JObject.Parse(@"{
            ""sample"": {
                ""myArray"": [
                    { ""myItem"": ""value1"" }
                ]
            }
        }");

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.sample.myArray[0]", result.Data.SelectToken("$.sample.myArray[0].currentPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithNoArguments_ReturnsCurrentTokenPath()
    {
        // Arrange
        var pathFunction = new ScriptPath();
        var currentToken = JToken.Parse(@"{ ""test"": ""value"" }");
        var currentTokenAtPath = currentToken.SelectToken("$.test");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.test", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithRelativePath_ReturnsAbsolutePath()
    {
        // Arrange
        var pathFunction = new ScriptPath("@.childProperty");
        var currentToken = JToken.Parse(@"{ ""parent"": { ""childProperty"": ""value"" } }");
        var currentTokenAtPath = currentToken.SelectToken("$.parent");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.parent.childProperty", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange
        var pathFunction = new ScriptPath("$.some.absolute.path");
        var currentToken = JToken.Parse(@"{ ""test"": ""value"" }");
        var currentTokenAtPath = currentToken.SelectToken("$.test");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.some.absolute.path", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_FunctionName_IsPath()
    {
        // Arrange
        var pathFunction = new ScriptPath();

        // Act & Assert
        Assert.AreEqual("path", pathFunction.FunctionName);
    }

    [Test]
    public void PathFunction_BooksAuthorsExample_WithParentNotationIndirectAndPath()
    {
        // Load test files - demonstrates merge with unique keys, parent notation <--, count, resolve, and remove
        var inputFile = Path.Combine(testDataPath, "books-authors-data.json");
        var expectedFile = Path.Combine(testDataPath, "books-authors-expected.json");
        var scriptFile = Path.Combine(testDataPath, "books-authors-script.json");

        var data = JToken.Parse(File.ReadAllText(inputFile));
        var expected = JToken.Parse(File.ReadAllText(expectedFile));
        var script = File.ReadAllText(scriptFile);

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success, "Script execution should succeed");

        // Verify all_authors collection was removed
        Assert.IsNull(result.Data.SelectToken("$.all_authors"), "all_authors collection should be removed");

        // Verify authors collection still exists and has correct structure
        var authors = result.Data.SelectTokens("$.authors[*]").ToList();
        Assert.AreEqual(3, authors.Count, "Should have 3 distinct authors");

        // Verify each author has books property but NOT number_of_books property
        foreach (var author in authors)
        {
            Assert.IsNotNull(author.SelectToken("books"), "Each author should have books property");
            Assert.IsNull(author.SelectToken("number_of_books"), "Authors should NOT have number_of_books property (removed by script)");
        }

        // Verify books have updated author information with number_of_books
        var books = result.Data.SelectTokens("$.books[*]").ToList();
        foreach (var book in books)
        {
            var authorNumberOfBooks = book.SelectToken("author.number_of_books")?.Value<int>();
            Assert.IsNotNull(authorNumberOfBooks, "Each book's author should have number_of_books property");
            Assert.Greater(authorNumberOfBooks.Value, 0, "number_of_books should be greater than 0");
        }

        // Verify specific author book counts in the books collection
        var fitzgeraldBooksInOriginal = result.Data.SelectTokens("$.books[?(@.author.id == 101)]").ToList();
        var expectedFitzgeraldCount = fitzgeraldBooksInOriginal.Count;

        foreach (var book in fitzgeraldBooksInOriginal)
        {
            Assert.AreEqual(2, book.SelectToken("author.number_of_books")?.Value<int>(),
                "F. Scott Fitzgerald should have 2 books");
        }

        var orwellBooksInOriginal = result.Data.SelectTokens("$.books[?(@.author.id == 103)]").ToList();
        var expectedOrwellCount = orwellBooksInOriginal.Count;

        foreach (var book in orwellBooksInOriginal)
        {
            Assert.AreEqual(2, book.SelectToken("author.number_of_books")?.Value<int>(),
                "George Orwell should have 2 books");
        }

        var leeBooksInOriginal = result.Data.SelectTokens("$.books[?(@.author.id == 102)]").ToList();
        var expectedLeeCount = leeBooksInOriginal.Count;

        foreach (var book in leeBooksInOriginal)
        {
            Assert.AreEqual(1, book.SelectToken("author.number_of_books")?.Value<int>(),
                "Harper Lee should have 1 book");
        }

        // Verify specific expected counts
        Assert.AreEqual(2, fitzgeraldBooksInOriginal.Count, "F. Scott Fitzgerald should have 2 books in books collection");
        Assert.AreEqual(2, orwellBooksInOriginal.Count, "George Orwell should have 2 books in books collection");
        Assert.AreEqual(1, leeBooksInOriginal.Count, "Harper Lee should have 1 book in books collection");

        // Verify authors have correct books structure
        var fitzgeraldAuthor = result.Data.SelectToken("$.authors[?(@.id == 101)]");
        Assert.IsNotNull(fitzgeraldAuthor, "F. Scott Fitzgerald should exist in authors");
        var fitzgeraldBooks = fitzgeraldAuthor.SelectToken("books");
        Assert.IsTrue(fitzgeraldBooks is JArray, "F. Scott Fitzgerald should have books as array (multiple books)");
        Assert.AreEqual(2, ((JArray)fitzgeraldBooks).Count, "F. Scott Fitzgerald should have 2 books in authors collection");

        var leeAuthor = result.Data.SelectToken("$.authors[?(@.id == 102)]");
        Assert.IsNotNull(leeAuthor, "Harper Lee should exist in authors");
        var leeBooks = leeAuthor.SelectToken("books");
        Assert.IsTrue(leeBooks is JObject, "Harper Lee should have books as single object (one book)");
        Assert.AreEqual("To Kill a Mockingbird", leeBooks.SelectToken("title")?.Value<string>());

        var orwellAuthor = result.Data.SelectToken("$.authors[?(@.id == 103)]");
        Assert.IsNotNull(orwellAuthor, "George Orwell should exist in authors");
        var orwellBooks = orwellAuthor.SelectToken("books");
        Assert.IsTrue(orwellBooks is JArray, "George Orwell should have books as array (multiple books)");
        Assert.AreEqual(2, ((JArray)orwellBooks).Count, "George Orwell should have 2 books in authors collection");
    }
}
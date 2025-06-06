using JLio.Extensions.JSchema;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.ExtensionsTests;

public class JSchemaExtensionsTests
{
    [Test]
    public void GetPathsWithMultipleArrayItemsUsesIndexPaths()
    {
        var schemaJson = "{ 'type':'array', 'items': [ { 'type':'string' }, { 'type':'number' } ] }".Replace('\'', '"');
        var schema = JSchema.Parse(schemaJson);
        var result = schema.GetPaths();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(SchemaPropertyType.Array, result[0].Type);
        Assert.AreEqual("$", result[0].Path);
        Assert.IsTrue(result.Any(p => p.Path == "$[0]" && p.Type == SchemaPropertyType.Primitive));
        Assert.IsTrue(result.Any(p => p.Path == "$[1]" && p.Type == SchemaPropertyType.Primitive));
    }
}

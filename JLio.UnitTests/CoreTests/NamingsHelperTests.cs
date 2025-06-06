using JLio.Core.Extensions;
using NUnit.Framework;

namespace JLio.UnitTests.CoreTests;

public class NamingsHelperTests
{
    [TestCase("Hello", "hello")]
    [TestCase("hello", "hello")]
    [TestCase("", "")]
    [TestCase(null, null)]
    public void CamelCasingReturnsExpected(string input, string expected)
    {
        var result = NamingsHelper.CamelCasing(input);
        Assert.AreEqual(expected, result);
    }
}

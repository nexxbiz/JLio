using JLio.Core;
using JLio.Core.Models.Path;
using NUnit.Framework;

namespace JLio.UnitTests.SplitTextTests;

public class SplitTextUnitTests
{
    [TestCase("demo", new[] {"demo"})]
    [TestCase("demo, demo2", new[] {"demo", "demo2"})]
    [TestCase("(demo)", new[] {"(demo)"})]
    [TestCase("[demo]", new[] {"[demo]"})]
    [TestCase("(demo, demo3)", new[] {"(demo, demo3)"})]
    [TestCase("('demo', 'demo3')", new[] {"('demo', 'demo3')"})]
    [TestCase("'demo, demo3'", new[] {"'demo, demo3'"})]
    [TestCase("'demo, demo3', 'demo2'", new[] {"'demo, demo3'", "'demo2'"})]
    [TestCase("demo[demo2,demo3],demo4", new[] {"demo[demo2,demo3]", "demo4"})]
    [TestCase("demo[demo2]", new[] {"demo[demo2]"})]
    public void ArgumentsTest(string initial, string[] elements)
    {
        var result = SplitText.GetChoppedElements(initial, ',', CoreConstants.ArgumentLevelPairs);

        Assert.AreEqual(elements.Length, result.Count);
        for (var i = 0; i < elements.Length; i++) Assert.AreEqual(elements[i], result[i].Text);
    }

    [TestCase("demo", new[] {"demo"})]
    [TestCase("demo(arg1,arg2)", new[] {"demo", "arg1,arg2"})]
    [TestCase("demo(arg1,arg2(arg3))", new[] {"demo", "arg1,arg2(arg3)"})]
    [TestCase("demo(arg1,arg2[2])", new[] {"demo", "arg1,arg2[2]"})]
    [TestCase("demo[1]", new[] {"demo[1]"})]
    [TestCase("demo[1](arg1,arg2)", new[] {"demo[1]", "arg1,arg2"})]
    [TestCase("demo[1](arg1,arg2(arg3))", new[] {"demo[1]", "arg1,arg2(arg3)"})]
    [TestCase("demo[1](arg1,arg2[2])", new[] {"demo[1]", "arg1,arg2[2]"})]
    [TestCase("demo[(1,4)](arg1,arg2[2])", new[] {"demo[(1,4)]", "arg1,arg2[2]"})]
    public void FunctionsTest(string initial, string[] elements)
    {
        var result = SplitText.GetChoppedElements(initial, new[] {'(', ')'}, CoreConstants.ArgumentLevelPairs);

        Assert.AreEqual(elements.Length, result.Count);
        for (var i = 0; i < elements.Length; i++) Assert.AreEqual(elements[i], result[i].Text);
    }
}
using JLio.Core.Extensions;
using NUnit.Framework;

namespace JLio.UnitTests.SplittedPathTests;

public class PathTests
{
    [TestCase("$.demo", "demo")]
    [TestCase("$.demo.test", "test")]
    [TestCase("$.['demo']", "['demo']")]
    [TestCase("$.['demo'].['test']", "['test']")]
    [TestCase("$.['demo demo']", "['demo demo']")]
    [TestCase("$.['demo'].['test test']", "['test test']")]
    [TestCase("$.['demo demo[*]']", "['demo demo[*]']")]
    [TestCase("$.['demo'].['test test[*]']", "['test test[*]']")]
    [TestCase("$.['demo.demo[*]']", "['demo.demo[*]']")]
    [TestCase("$.['demo'].['test.test[*]']", "['test.test[*]']")]
    [TestCase("$.['demo.demo[?($.demo =1)]']", "['demo.demo[?($.demo =1)]']")]
    [TestCase("$.['demo'].['test.test[?($.demo =1)]']", "['test.test[?($.demo =1)]']")]
    public void CanSplitPath(string path, string lastElementName)
    {
        var sut = new JsonSplittedPath(path).LastName;
        Assert.AreEqual(lastElementName, sut);
    }

    [TestCase("$.demo", false)]
    [TestCase("$.demo.test", false)]
    [TestCase("$.['demo']", false)]
    [TestCase("$.['demo'].['test']", false)]
    [TestCase("$.['demo'][*]", true)]
    [TestCase("$.['demo'].['test'][*]", true)]
    [TestCase("$.['demo demo']", false)]
    [TestCase("$.['demo'].['test test']", false)]
    [TestCase("$.['demo demo[*]']", true)]
    [TestCase("$.['demo'].['test test[*]']", true)]
    [TestCase("$.['demo.demo[*]']", true)]
    [TestCase("$.['demo'].['test.test[*]']", true)]
    [TestCase("$.['demo.demo[?($.demo =1)]']", true)]
    [TestCase("$.['demo'].['test.test[?($.demo =1)]']", true)]
    public void CanDetectArray(string path, bool hasArrayNotation)
    {
        var sut = new JsonSplittedPath(path).LastElement.HasArrayIndicator;
        Assert.AreEqual(hasArrayNotation, sut);
    }

    [TestCase("$.demo", "")]
    [TestCase("$.demo.test", "")]
    [TestCase("$.['demo']", "")]
    [TestCase("$.['demo'].['test']", "")]
    [TestCase("$.['demo'][*]", "[*]")]
    [TestCase("$.['demo'].['test'][*]", "[*]")]
    [TestCase("$.['demo demo']", "")]
    [TestCase("$.['demo'].['test test']", "")]
    [TestCase("$.['demo demo[*]']", "[*]")]
    [TestCase("$.['demo'].['test test[*]']", "[*]")]
    [TestCase("$.['demo.demo[*]']", "[*]")]
    [TestCase("$.['demo'].['test.test[*]']", "[*]")]
    [TestCase("$.['demo.demo[?($.demo =1)]']", "[?($.demo =1)]")]
    [TestCase("$.['demo'].['test.test[?($.demo =1)]']", "[?($.demo =1)]")]
    public void CanGetArray(string path, string arrayNotation)
    {
        var sut = new JsonSplittedPath(path).LastElement.ArrayNotation;
        Assert.AreEqual(arrayNotation, sut);
    }
}
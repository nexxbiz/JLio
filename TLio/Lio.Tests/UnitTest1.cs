using System.Text.Json;
using NUnit.Framework;

namespace Lio.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var myTest = new Add();

            JsonSerializerOptions options = new JsonSerializerOptions();
            DiscriminatorC
            Assert.Pass();
        }
    }
    
    public interface ICommand{}

    public class Add : ICommand
    {
        
    }

    public class Set : ICommand
    {
        
    }
}
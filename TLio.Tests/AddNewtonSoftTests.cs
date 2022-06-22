using Json.More;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TLio.Contracts;
using TLio.Extensions;
using TLio.Implementations;
using TLio.Models;
using TLio.Services;
using System.Text.Json.Nodes;

namespace TLio.Tests
{
    public class AddNewtonSoftTests
    {
        private IScriptRunner scriptRunner;

        [SetUp]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();

            // Add any DI stuff here:
            serviceCollection.AddLio();

            // Create the ServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // serviceScopeMock will contain my ServiceProvider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet<IServiceProvider>(s => s.ServiceProvider)
                .Returns(serviceProvider);

            // serviceScopeFactoryMock will contain my serviceScopeMock
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            scriptRunner = new ScriptRunner(serviceScopeFactoryMock.As<IServiceScopeFactory>().Object);

        }

        [Test]
        public async Task AddPropertyToObjectUsingSystemTextJson()
        {
            var testObject = new MyTestObject();
            var script = new Script().AddCommand(new Add("$.myFirstInput.myNewProperty", new LiteralValue("demo")));

            var result = await scriptRunner.RunAsync(script,
                new Dictionary<string, object>()
                {
                    ["myFirstInput"] =  System.Text.Json.JsonSerializer.SerializeToNode(new MyTestObject())!

                }, CancellationToken.None); 
            Assert.Pass();
        }
    }
}

internal class MyTestObject
{
    public int MyProperty { get; set; } = 99;
}
    

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TLio.Contracts;
using TLio.Extensions;
using TLio.Models;
using TLio.Services;

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
        public async Task AddPropertyToObject()
        {
            var result = await scriptRunner.RunAsync(new Script(),
                new Dictionary<string, object>()
                {
                    ["myFirstInput"] = 10
                }, CancellationToken.None);
            Assert.Pass();

           
        }
    }
}
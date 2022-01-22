using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using JLio2.Core;
using JLio2.Core.Models;
using MediatR;
using NUnit.Framework;

namespace JLio2.UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
       
    }

    [Test]
    public void Test1()
    {
        var script = new ScriptDefinition();

        var scriptRunner = new ScriptRunner(null, null, new CancellationToken());
        scriptRunner.RunScriptAsync(script, new ScriptInput());
    }
}

public class Add
{
    
}
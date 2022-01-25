using Lio.Core.Models;
using Lio.Core.Runner;
using Microsoft.AspNetCore.Mvc;

namespace Lio.Client.Controllers;

[ApiController]
[Route("/api")]
public class ScriptRunnerController : ControllerBase
{
    private readonly IScriptRunner _scriptRunner;


    public ScriptRunnerController(IScriptRunner scriptRunner)
    {
        _scriptRunner = scriptRunner;
    }

    [HttpPost("/run-script")]
    public async Task<IActionResult> RunScript([FromBody]Script script)
    {
        var result = await _scriptRunner.RunScriptAsync(new ScriptDefinition(), new ScriptInput());
        return Ok();
    }

    public class Script
    {
        public object Input { get; set; }
        
        public object Commands { get; set; }
    }
}
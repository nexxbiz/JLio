using JLio.Newtonsoft.Client;
using Lio.Core.Commands.Implementations;
using Lio.Core.Models;
using Lio.Core.Runner;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IScriptRunner _scriptRunner;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IScriptRunner scriptRunner)
    {
        _logger = logger;
        _scriptRunner = scriptRunner;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpPost("/run-script")]
    public async Task<IActionResult> RunScript()
    {
        var result = await _scriptRunner.RunScriptAsync(new ScriptDefinition
        {
            new Add("$.demo", 100)
        }, NewtonSoftScriptInput.Create("{\"demo\": [1,2]}"));
        return Ok();
    }
}
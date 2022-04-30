using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TLio.Contracts;
using TLio.Implementations;
using TLio.Models;

namespace TLio.App.Controllers
{
    [ApiController]
    [Route("/api/test")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IScriptRunner scriptRunner;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IScriptRunner scriptRunner)
        {
            _logger = logger;
            this.scriptRunner = scriptRunner;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await scriptRunner.RunAsync(new Script
            {
                MutatorType = "Default",
                FetcherType = "Default",
                ScriptMetadata = new ScriptMetadata
                {
            
                },
                Commands = new List<ICommand>()
                {
                    new Add("$", new LiteralValue(""))
                }
            }, new Dictionary<string, object>()
            {
                ["myFirstInput"] = 10
            }, CancellationToken.None);

            return Ok();
        }
    }
}
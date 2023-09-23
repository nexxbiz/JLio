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
    public class JlioController : ControllerBase
    {
        private readonly ILogger<JlioController> _logger;
        private readonly IScriptRunner scriptRunner;

        public JlioController(ILogger<JlioController> logger, IScriptRunner scriptRunner)
        {
            _logger = logger;
            this.scriptRunner = scriptRunner;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await scriptRunner.RunAsync(new Script
            {
                DataEcosystemName = "NewtonsoftEcosystem",
                //removed items here , because they were default so minimaze the exposure of them. You still can set them if you want to
                Commands = new CommandsList()
                {
                    new Add("$.otherProperty", "sdsds")
                }
            }, new Dictionary<string, object>()  //i would like to have a own type defintion of this like InputObjects
            {
                ["myFirstInput"] = 10
            }, CancellationToken.None);

            return Ok(result.Output);
        }
    }
}
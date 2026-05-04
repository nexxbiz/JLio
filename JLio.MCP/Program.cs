using JLio.MCP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Redirect all logging to stderr so stdout carries only JSON-RPC traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<JLioEngineFactory>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<JLioTools>()
    .WithTools<JLioReferenceTools>()
    .WithTools<JLioScriptProposerTools>()
    .WithTools<JLioSearchTools>();

await builder.Build().RunAsync();

using JLio.Client;

namespace JLio.MCP;

/// <summary>
/// Factory that creates a fresh <see cref="JLioEngine"/> for every execution
/// so that log entries and execution state are isolated per call.
/// The engine is configured with the full V3 feature set plus all extensions.
/// </summary>
public sealed class JLioEngineFactory
{
    public JLioEngine Create()
    {
        return JLioEngineConfigurations.CreateV3()
            .Build();
    }
}

using System.Text.Json.Serialization;

namespace JLio2.Core.Models;

public class CommandDefinition
{
    [JsonIgnore]
    public Type Type { get; set; } = default!;

    public string? Name { get; set; } = nameof(Type);
}



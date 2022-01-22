namespace JLio2.Core.Models;

public class CommandDefinitionProperty
{
    public CommandDefinitionProperty()
    {
    }

    public CommandDefinitionProperty(IDictionary<string, string?> expressions)
    {
        Expressions = expressions;
    }

    public IDictionary<string, string?> Expressions { get; set; }

    public static CommandDefinitionProperty Literal(string expression) => new(CreateSingleExpression("Literal", expression));
    public static CommandDefinitionProperty Function(string expression) => new(CreateSingleExpression("Function", expression));

    private static IDictionary<string, string?> CreateSingleExpression(string syntax, string expression) => new Dictionary<string, string?> { [syntax] = expression };

}
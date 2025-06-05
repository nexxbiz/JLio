using JLio.Commands.Models;
using JLio.Core.Models;

namespace JLio.Commands.Builders;

public static class DecisionTableBuilders
{
    public static DecisionTablePathContainer DecisionTable(this JLioScript source, string path)
    {
        return new DecisionTablePathContainer(source, path);
    }

    public static JLioScript With(this DecisionTablePathContainer source, DecisionTableConfig config)
    {
        source.Script.AddLine(new DecisionTable { Path = source.Path, DecisionTableConfig = config });
        return source.Script;
    }

    public class DecisionTablePathContainer
    {
        public DecisionTablePathContainer(JLioScript script, string path)
        {
            Script = script;
            Path = path;
        }

        internal JLioScript Script { get; }
        internal string Path { get; }
    }
}

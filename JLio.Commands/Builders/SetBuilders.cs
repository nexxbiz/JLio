using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Builders;

public static class SetBuilders
{
    public static JLioScript OnPath(this SetValueContainer source, string path)
    {
        source.Script.AddLine(new Set(path, new FunctionSupportedValue(new FixedValue(source.Value))));
        return source.Script;
    }

    public static JLioScript OnPath(this SetValueAsFunctionContainer source, string path)
    {
        source.Script.AddLine(new Set(path, new FunctionSupportedValue(source.Function)));
        return source.Script;
    }

    public static SetValueContainer Set(this JLioScript source, JToken value)
    {
        return new SetValueContainer(source, value);
    }

    public static SetValueAsFunctionContainer Set(this JLioScript source, IFunction function)
    {
        return new SetValueAsFunctionContainer(source, function);
    }

    public class SetValueContainer
    {
        public SetValueContainer(JLioScript source, JToken value)
        {
            Script = source;
            Value = value;
        }

        internal JLioScript Script { get; }
        internal JToken Value { get; }
    }

    public class SetValueAsFunctionContainer
    {
        public SetValueAsFunctionContainer(JLioScript source, IFunction function)
        {
            Script = source;
            Function = function;
        }

        internal IFunction Function { get; }

        internal JLioScript Script { get; }
    }
}
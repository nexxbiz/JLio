using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;


namespace JLio.Commands.Builders
{
    public static class AddBuilders
    {
        public static JLioScript OnPath(this AddValueContainer source, string path)
        {
            source.Script.AddLine(new Add(path, new JLioFunctionSupportedValue(new FixedValue(source.Value))));
            return source.Script;
        }

        public static JLioScript OnPath(this AddValueAsFunctionContainer source, string path)
        {
            source.Script.AddLine(new Add(path, new JLioFunctionSupportedValue(source.Function)));
            return source.Script;
        }

        public static AddValueContainer Add(this JLioScript source, JToken value)
        {
            return new AddValueContainer(source, value);
        }

        public static AddValueAsFunctionContainer Add(this JLioScript source, IFunction function)
        {
            return new AddValueAsFunctionContainer(source, function);
        }

        public class AddValueContainer
        {
            public AddValueContainer(JLioScript source, JToken value)
            {
                Script = source;
                Value = value;
            }
            internal JLioScript Script { get; }
            internal JToken Value { get; }
        }

        public class AddValueAsFunctionContainer
        {
            public AddValueAsFunctionContainer(JLioScript source, IFunction function)
            {
                Script = source;
                Function = function;
            }

            internal JLioScript Script { get; }
            internal IFunction Function { get; }
        }
    }
}

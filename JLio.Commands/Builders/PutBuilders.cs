using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Builders
{
    public static class PutBuilders
    {
        public static JLioScript OnPath(this PutValueContainer source, string path)
        {
            source.Script.AddLine(new Put(path, new FunctionSupportedValue(new FixedValue(source.Value))));
            return source.Script;
        }

        public static JLioScript OnPath(this PutValueAsFunctionContainer source, string path)
        {
            source.Script.AddLine(new Put(path, new FunctionSupportedValue(source.Function)));
            return source.Script;
        }

        public static PutValueContainer Put(this JLioScript source, JToken value)
        {
            return new PutValueContainer(source, value);
        }

        public static PutValueAsFunctionContainer Put(this JLioScript source, IFunction function)
        {
            return new PutValueAsFunctionContainer(source, function);
        }

        public class PutValueContainer
        {
            public PutValueContainer(JLioScript source, JToken value)
            {
                Script = source;
                Value = value;
            }

            internal JLioScript Script { get; }
            internal JToken Value { get; }
        }

        public class PutValueAsFunctionContainer
        {
            public PutValueAsFunctionContainer(JLioScript source, IFunction function)
            {
                Script = source;
                Function = function;
            }

            internal IFunction Function { get; }

            internal JLioScript Script { get; }
        }
    }
}
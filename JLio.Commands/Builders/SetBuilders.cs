using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Builders
{
    public static class SetBuilders
    {


        public static CommandLines OnPath(this SetValueContainer source, string path)
        {
            source.JsonScript.AddLine(new Set(path, new JLioFunctionSupportedValue(new FixedValue(source.Value))));
            return source.JsonScript;
           
        }

        public static CommandLines OnPath(this SetValueAsFunctionContainer source, string path)
        {
            source.JsonScript.AddLine(new Set(path, new JLioFunctionSupportedValue(source.Function)));
            return source.JsonScript;

        }

        public static SetValueContainer SetValue(this CommandLines source, JToken value)
        {
            return new SetValueContainer(source, value);
        }

        public static SetValueAsFunctionContainer SetValue(this CommandLines source, IJLioFunction function)
        {
            return new SetValueAsFunctionContainer(source, function);

        }

        public class SetValueContainer
        {
            public SetValueContainer(CommandLines source, JToken value)
            {
                JsonScript = source;
                Value = value;
            }

            internal CommandLines JsonScript { get; }
            internal JToken Value { get; }
        }

        public class SetValueAsFunctionContainer
        {
            public SetValueAsFunctionContainer(CommandLines source, IJLioFunction function)
            {
                JsonScript = source;
                Function = function;
            }

            internal CommandLines JsonScript { get; }
            internal IJLioFunction Function { get; }
        }


    }
}

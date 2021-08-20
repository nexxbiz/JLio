using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Builders
{
    public static class AddBuilders
    {


        public static CommandLines OnPath(this AddValueContainer source, string path)
        {
            source.JsonScript.AddLine(new Add(path, new JLioFunctionSupportedValue(new FixedValue(source.Value))));
            return source.JsonScript;
           
        }

        public static CommandLines OnPath(this AddValueAsFunctionContainer source, string path)
        {
            source.JsonScript.AddLine(new Add(path, new JLioFunctionSupportedValue(source.Function)));
            return source.JsonScript;

        }

        public static AddValueContainer AddValue(this CommandLines source, JToken value)
        {
            return new AddValueContainer(source, value);
        }

        public static AddValueAsFunctionContainer AddValue(this CommandLines source, IJLioFunction function)
        {
            return new AddValueAsFunctionContainer(source, function);

        }

        public class AddValueContainer
        {
            public AddValueContainer(CommandLines source, JToken value)
            {
                JsonScript = source;
                Value = value;
            }

            internal CommandLines JsonScript { get; }
            internal JToken Value { get; }
        }

        public class AddValueAsFunctionContainer
        {
            public AddValueAsFunctionContainer(CommandLines source, IJLioFunction function)
            {
                JsonScript = source;
                Function = function;
            }

            internal CommandLines JsonScript { get; }
            internal IJLioFunction Function { get; }
        }


    }
}

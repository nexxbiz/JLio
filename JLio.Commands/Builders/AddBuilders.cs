using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static JLio.Commands.Builders.CommonBuilders;

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

        public static AddValueContainer Add(this NewLine source, JToken value)
        {
            return new AddValueContainer(source.Script, value);
        }

        public static AddValueAsFunctionContainer Add(this NewLine source, IJLioFunction function)
        {
            return new AddValueAsFunctionContainer(source.Script, function);

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
            public AddValueAsFunctionContainer(JLioScript source, IJLioFunction function)
            {
                Script = source;
                Function = function;
            }

            internal JLioScript Script { get; }
            internal IJLioFunction Function { get; }
        }


    }
}

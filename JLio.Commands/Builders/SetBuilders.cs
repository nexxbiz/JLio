﻿using JLio.Core;
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


        public static JLioScript OnPath(this SetValueContainer source, string path)
        {
            source.Script.AddLine(new Set(path, new JLioFunctionSupportedValue(new FixedValue(source.Value))));
            return source.Script;
           
        }

        public static JLioScript OnPath(this SetValueAsFunctionContainer source, string path)
        {
            source.Script.AddLine(new Set(path, new JLioFunctionSupportedValue(source.Function)));
            return source.Script;

        }

        public static SetValueContainer Set(this NewLine source, JToken value)
        {
            return new SetValueContainer(source.Script, value);
        }

        public static SetValueAsFunctionContainer Set(this NewLine source, IJLioFunction function)
        {
            return new SetValueAsFunctionContainer(source.Script, function);

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
            public SetValueAsFunctionContainer(JLioScript source, IJLioFunction function)
            {
                Script = source;
                Function = function;
            }

            internal JLioScript Script { get; }
            internal IJLioFunction Function { get; }
        }


    }
}

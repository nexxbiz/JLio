﻿using JLio.Core;
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

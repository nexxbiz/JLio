﻿using JLio.Commands;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Functions;
using Newtonsoft.Json;

namespace JLio.Client
{
    public class JLioParseOptions : IJLioParseOptions
    {
        public JsonConverter JLioFunctionConverter { get; set; }
        public JsonConverter JLioCommandConverter { get; set; }

        public static JLioParseOptions CreateDefault()
        {
            var commandProvider = new JLioCommandsProvider();
            commandProvider.Register<Add>();

            var functionsProvider = new JLioFunctionsProvider();
            functionsProvider.Register<DatetimeFunction>();

            return new JLioParseOptions
            {
                JLioCommandConverter = new JLioCommandConverter(commandProvider),
                JLioFunctionConverter = new JLioFunctionConverter(functionsProvider)
            };
        }
    }
}
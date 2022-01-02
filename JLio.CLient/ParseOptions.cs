﻿using JLio.Commands;
using JLio.Commands.Advanced;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Functions;
using Newtonsoft.Json;

namespace JLio.Client
{
    public class ParseOptions : IParseOptions
    {
        private CommandsProvider commandsProvider = new CommandsProvider();
        private FunctionsProvider functionsProvider = new FunctionsProvider();

        public JsonConverter JLioFunctionConverter { get; set; }
        public IParseOptions RegisterFunction<T>() where T : IFunction
        {
            functionsProvider.Register<T>();
            return this;
        }

        public IParseOptions RegisterCommand<T>() where T : ICommand
        {
            commandsProvider.Register<T>();
            return this;
        }

        public JsonConverter JLioCommandConverter { get; set; }

        public static ParseOptions CreateDefault()
        {
            var commandProvider = new CommandsProvider();
            commandProvider
                .Register<Add>()
                .Register<Put>()
                .Register<Set>()
                .Register<Remove>()
                .Register<Copy>()
                .Register<Move>()
                .Register<Compare>()
                .Register<Merge>();

            var functionsProvider = new FunctionsProvider();
            functionsProvider
                .Register<Datetime>()
                .Register<NewGuid>()
                .Register<Concat>()
                .Register<Parse>()
                .Register<Partial>()
                .Register<ToString>()
                .Register<Promote>();

            return new ParseOptions
            {
                JLioCommandConverter = new CommandConverter(commandProvider),
                JLioFunctionConverter = new FunctionConverter(functionsProvider)
            };
        }
    }
}
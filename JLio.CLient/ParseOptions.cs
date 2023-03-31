using JLio.Commands;
using JLio.Commands.Advanced;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Functions;
using Newtonsoft.Json;
using System.Threading;

namespace JLio.Client
{
    public class ParseOptions : IParseOptions
    {
        public FunctionsProvider FunctionsProvider { get; private set; }
        public CommandsProvider CommandsProvider { get; private set; }

        public JsonConverter JLioFunctionConverter { get; set; }
        public JsonConverter JLioCommandConverter { get; set; }

        public ParseOptions RegisterFunction<F>() where F : IFunction
        {
            FunctionsProvider.Register<F>();
            return this;
        }

        public ParseOptions RegisterCommand<C>() where C : ICommand
        {
            CommandsProvider.Register<C>();
            return this;
        }
        public static ParseOptions CreateDefault()
        {
            var commandsProvider = new CommandsProvider();
            commandsProvider
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
               .Register<Promote>()
               .Register<Format>();



            return new ParseOptions
            {
                FunctionsProvider = functionsProvider,
                CommandsProvider = commandsProvider,
                JLioCommandConverter = new CommandConverter(commandsProvider),
                JLioFunctionConverter = new FunctionConverter(functionsProvider)
            };
        }
    }
}
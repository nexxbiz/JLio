using JLio.Commands;
using JLio.Commands.Advanced;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Functions;
using Newtonsoft.Json;

namespace JLio.Client;

public class ParseOptions : IParseOptions
{
    public FunctionsProvider FunctionsProvider { get; private set; }
    public CommandsProvider CommandsProvider { get; private set; }

    public JsonConverter JLioFunctionConverter { get; set; }
    public JsonConverter JLioCommandConverter { get; set; }

    public IParseOptions RegisterFunction<F>() where F : IFunction
    {
        FunctionsProvider.Register<F>();
        return this;
    }

    public IParseOptions RegisterCommand<C>() where C : ICommand
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
           .Register<Merge>()
           .Register<DecisionTable>()
           .Register<IfElse>();

        var functionsProvider = new FunctionsProvider();
        functionsProvider
           .Register<Datetime>()
           .Register<Partial>()
           .Register<Promote>()
           .Register<Fetch>();



        var options = new ParseOptions
        {
            FunctionsProvider = functionsProvider,
            CommandsProvider = commandsProvider,
            JLioCommandConverter = new CommandConverter(commandsProvider),
            JLioFunctionConverter = new FunctionConverter(functionsProvider)
        };

        FixedValue.DefaultFunctionConverter = (FunctionConverter)options.JLioFunctionConverter;

        return options;
    }
}
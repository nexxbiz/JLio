using Newtonsoft.Json;

namespace JLio.Core.Contracts;

public interface IParseOptions
{
    JsonConverter JLioCommandConverter { get; set; }
    JsonConverter JLioFunctionConverter { get; set; }

    IParseOptions RegisterCommand<C>() where C : ICommand;
    IParseOptions RegisterFunction<F>() where F : IFunction;
}
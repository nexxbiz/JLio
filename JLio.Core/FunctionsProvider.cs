using System;
using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Core;

public class FunctionsProvider : IFunctionsProvider, IFunctionsProviderRegistrar
{
    private readonly FunctionRegistrations functions = new FunctionRegistrations();

    public IFunction this[string function]
    {
        get
        {
            if (!functions.ContainsKey(function)) return null;
            var functionImplementation = functions[function];
            return CreateInstance(functionImplementation);
        }
    }

    public IFunctionsProviderRegistrar Register<T>() where T : IFunction
    {
        var function = typeof(T);
        var functionInstance = (IFunction) Activator.CreateInstance(function);
        DeleteIfFunctionNameAlreadyExists(functionInstance);

        functions.Add(functionInstance.FunctionName, new FunctionRegistration(function));
        return this;
    }

    private void DeleteIfFunctionNameAlreadyExists(IFunction functionInstance)
    {
        if (functionInstance != null && !functions.ContainsKey(functionInstance.FunctionName))
            functions.Remove(functionInstance.FunctionName);
    }

    private static IFunction CreateInstance(FunctionRegistration functionRegistration)
    {
        return (IFunction) Activator.CreateInstance(functionRegistration.Type);
    }
}
using System;
using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Core
{
    public class JLioFunctionsProvider : IJLioFunctionsProvider, IJLioFunctionsProviderRegistrar
    {
        private readonly JLioFunctionRegistrations functions = new JLioFunctionRegistrations();
        public IFunction this[string function]
        {
            get
            {
                if (!functions.ContainsKey(function)) return null;
                var functionImplementation = functions[function];
                return CreateInstance(functionImplementation);
            }
        }

        public IJLioFunctionsProviderRegistrar Register<T>() where T : IFunction
        {
            var function = typeof(T);
            var functionInstance = (IFunction) Activator.CreateInstance(function);
            DeleteIfFunctionNameAlreadyExists<T>(functionInstance);

            functions.Add(functionInstance.FunctionName, new JLioFunctionRegistration(function));
            return this;
        }

        private void DeleteIfFunctionNameAlreadyExists<T>(IFunction functionInstance) where T : IFunction
        {
            if (functionInstance != null && !functions.ContainsKey(functionInstance.FunctionName))
                functions.Remove(functionInstance.FunctionName);
        }

        private static IFunction CreateInstance(JLioFunctionRegistration functionRegistration)
        {
            return (IFunction) Activator.CreateInstance(functionRegistration.Type);
        }
    }
}
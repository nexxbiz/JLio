using System;
using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Core
{
    public class JLioFunctionsProvider : IJLioFunctionsProvider, IJLioFunctionsProviderRegistrar
    {
        private readonly JLioFunctionRegistrations functions = new JLioFunctionRegistrations();

        public int NumberOfFunctions => functions.Count;

        public IJLioFunction this[string function]
        {
            get
            {
                if (!functions.ContainsKey(function)) return null;
                var functionImplementation = functions[function];
                return CreateInstance(functionImplementation);
            }
        }

        public IJLioFunctionsProviderRegistrar Register<T>() where T : IJLioFunction
        {
            var function = typeof(T);
            var functionInstance = (IJLioFunction) Activator.CreateInstance(function);
            DeleteIfFunctionNameAlreadyExists<T>(functionInstance);

            functions.Add(functionInstance.FunctionName, new JLioFunctionRegistration(function));
            return this;
        }

        private void DeleteIfFunctionNameAlreadyExists<T>(IJLioFunction functionInstance) where T : IJLioFunction
        {
            if (functionInstance != null && !functions.ContainsKey(functionInstance.FunctionName))
                functions.Remove(functionInstance.FunctionName);
        }

        private static IJLioFunction CreateInstance(JLioFunctionRegistration functionRegistration)
        {
            return (IJLioFunction) Activator.CreateInstance(functionRegistration.Type);
        }
    }
}
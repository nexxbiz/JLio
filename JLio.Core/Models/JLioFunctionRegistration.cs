using System;

namespace JLio.Core.Models
{
    public class JLioFunctionRegistration
    {
        public JLioFunctionRegistration(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        //todo implement the ctor compiled lamba expression for performance instead of using the reflection activator.creteinstance method.
        // see: https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
    }
}
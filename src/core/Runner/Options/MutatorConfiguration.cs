using System;

namespace Lio.Core.Runner.Options
{
    public class MutatorConfiguration
    {
        public MutatorConfiguration(Type pathFinder, Type mutatorType)
        {
            PathFinder = pathFinder;
            MutatorType = mutatorType;
        }

        public Type PathFinder { get; }
        public Type MutatorType { get; }
    }
}
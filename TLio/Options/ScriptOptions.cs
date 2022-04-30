using System;
using System.Collections.Generic;

namespace TLio.Options
{
    public class ScriptOptions
    {
        public ScriptOptions()
        {
            DataFetchers = new Dictionary<string, Type>();
            Mutators = new Dictionary<string, Type>();
        }

        public IDictionary<string, Type> DataFetchers { get; }
        public IDictionary<string, Type> Mutators { get; }
        
        public ScriptOptions RegisterMutator(Type mutator, string name)
        {
            Mutators.Add(name, mutator);
            return this;
        }

        public ScriptOptions RegisterDataFetcher(Type dataFetcher, string name)
        {
            DataFetchers.Add(name, dataFetcher);
            return this;
        }

        public ScriptOptions RegisterDataFetcher<TDataFetcher>(string name) =>
            RegisterDataFetcher(typeof(TDataFetcher), name);
        
        public ScriptOptions RegisterMutator<TMutator>(string name) =>
            RegisterMutator(typeof(TMutator), name);

    }
}
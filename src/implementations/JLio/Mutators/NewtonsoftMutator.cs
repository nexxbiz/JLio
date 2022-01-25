using System;
using System.Collections.Generic;
using Lio.Core;
using Lio.Core.Contracts;

namespace JLio.Mutators;

public class NewtonsoftMutator : ISpecificMutator
{
    public TargetTypes GetTargetType(string path)
    {
        throw new NotImplementedException();
    }

    public void AddItemToArray(string path, IFunctionSupportedValue value)
    {
        throw new NotImplementedException();
    }

    public List<string> GetPathsForSelectionPath(string selectionPath)
    {
        throw new NotImplementedException();
    }
}
namespace JLio2.Core;

public interface ISpecificMutator
{
    TargetTypes GetTargetType(string path);
    void AddItemToArray(string path, IFunctionSupportedValue value);
    List<string> GetPathsForSelectionPath(string selectionPath);
}
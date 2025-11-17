using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class;

public class ClassManager : IClassManager
{
    private readonly Dictionary<string, IClass> _classByNames;

    public ClassManager(IEnumerable<IClass> classes)
    {
        _classByNames = new Dictionary<string, IClass>(StringComparer.InvariantCultureIgnoreCase);
        foreach(var c in classes)
            _classByNames.Add(c.Name, c);
    }

    #region IClassManager

    public IEnumerable<IClass> Classes
        => _classByNames.Values;

    public IClass? this[string name]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            if (_classByNames.TryGetValue(name, out var c))
                return c;
            return null;
        }
    }

    #endregion
}

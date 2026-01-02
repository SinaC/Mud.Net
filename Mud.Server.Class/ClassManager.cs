using Mud.Common.Attributes;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class;

[Export(typeof(IClassManager)), Shared]
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
            return _classByNames.GetValueOrDefault(name);
        }
    }

    #endregion
}

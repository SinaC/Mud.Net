namespace Mud.Server.Class.Interfaces;

public interface IClassManager
{
    IEnumerable<IClass> Classes { get; }

    IClass? this[string name] { get; }
}

namespace Mud.Server.Interfaces.Class;

public interface IClassManager
{
    IEnumerable<IClass> Classes { get; }

    IClass? this[string name] { get; }
}

using System.Diagnostics;

namespace Mud.POC.Combat;

[DebuggerDisplay("{Name}")]
public class ItemCorpse : EntityBase, IItemCorpse
{
    private string _name = null!;
    private string _corpseName = null!;
    private IRoom? _room = null!;

    public ItemCorpse(string name, ICharacter victim)
    {
        _name = name;
        _corpseName = "the corpse of " + victim.Name;
    }

    public string Name => _name;
    public string CorpseName => _corpseName;
    public IRoom? Room => _room;

    public void SetRoom(IRoom? room)
    {
        _room = room;
    }
}

public interface IItem : IEntity
{
    string Name { get; }

    IRoom? Room { get; }

    void SetRoom(IRoom? room);
}

public interface IItemCorpse : IItem
{
    string CorpseName { get; }
}
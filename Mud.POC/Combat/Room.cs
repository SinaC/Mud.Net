using System.Diagnostics;

namespace Mud.POC.Combat;

[DebuggerDisplay("{Name}")]
public class Room : EntityBase, IRoom
{
    public static IRoom DeathRoom { get; } = new Room("death");
    public static IRoom FleeRoom { get; } = new Room("flee");

    private string _name = null!;
    private List<ICharacter> _people = [];
    private List<IItem> _content = [];

    public string Name => _name;
    public IEnumerable<ICharacter> People => _people;
    public IEnumerable<IItem> Content => _content;

    public Room(string name)
    {
        _name = name;
    }

    public void Add(ICharacter character)
    {
        if (_people.Contains(character))
            return;
        _people.Add(character);
    }

    public void Remove(ICharacter character)
    {
        if (!_people.Contains(character))
            return;
        _people.Remove(character);
    }

    public void Add(IItem item)
    {
        if (_content.Contains(item))
            return;
        _content.Add(item);
    }

    public void Remove(IItem item)
    {
        if (!_content.Contains(item))
            return;
        _content.Remove(item);
    }

    public void Recompute()
    {
    }
}

public interface IRoom : IEntity
{
    string Name { get; }
    IEnumerable<ICharacter> People { get; }
    IEnumerable<IItem> Content { get; }

    void Add(ICharacter character);
    void Remove(ICharacter character);

    void Add(IItem item);
    void Remove(IItem item);

    void Recompute();
}

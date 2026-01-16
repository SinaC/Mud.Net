using System.Diagnostics;

namespace Mud.POC.Combat;

[DebuggerDisplay("{Name} {IsDead} {Room}")]
public class PlayableCharacter : CharacterBase, IPlayableCharacter
{
    private IGroup? _group;
    private List<INonPlayableCharacter> _pets = [];

    public IGroup? Group => _group;
    public IEnumerable<INonPlayableCharacter> Pets => _pets;

    public PlayableCharacter(ICombatManager combatManager, string name, int hitPoints)
        : base(combatManager, name, hitPoints)
    {
    }

    public void LeaveGroup()
    {
        if (_group == null)
            return;
        var aloneInGroup = _group.RemoveMember(this);
        if (aloneInGroup)
            _group = null;
    }

    public void JoinGroup(IPlayableCharacter pc)
    {
        if (pc.Group != null)
            return;
        if (_group == null)
        {
            var group = new Group(this, pc);
            _group = group;
            pc.JoinGroup(group);
        }
        else
            pc.JoinGroup(_group);
    }

    public void JoinGroup(IGroup group)
    {
        _group = group;
    }

    public void AddPet(INonPlayableCharacter pet)
    {
        if (_pets.Contains(pet))
            return;
        _pets.Add(pet);
        pet.SetMaster(this);
    }

    public void RemovePet(INonPlayableCharacter pet)
    {
        if (!_pets.Contains(pet))
            return;
        _pets.Remove(pet);
    }

    public void RemoveDeadFlag()
    {
        _isDead = false;
    }
}

public interface IPlayableCharacter : ICharacter
{
    IGroup? Group { get; }
    IEnumerable<INonPlayableCharacter> Pets { get; }

    void JoinGroup(IPlayableCharacter pc);
    void JoinGroup(IGroup group);

    void AddPet(INonPlayableCharacter pet);
    void RemovePet(INonPlayableCharacter pet);

    void RemoveDeadFlag();
}

using System.Diagnostics;

namespace Mud.POC.Combat;

[DebuggerDisplay("{Name} {IsDead} {Room}")]
public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
{
    private IPlayableCharacter? _master = null;

    public IPlayableCharacter? Master => _master;

    public NonPlayableCharacter(ICombatManager combatManager, string name, int hitPoints)
        : base(combatManager, name, hitPoints)
    {
    }

    public void SetMaster(IPlayableCharacter? master)
    {
        _master = master;
        // clear aggro table
        CombatManager.ClearAggroTable(this);
    }
}

public interface INonPlayableCharacter : ICharacter
{
    IPlayableCharacter? Master { get; }

    void SetMaster(IPlayableCharacter? master);
}

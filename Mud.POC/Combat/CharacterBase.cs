namespace Mud.POC.Combat;

public abstract class CharacterBase : EntityBase, ICharacter
{
    protected ICombatManager CombatManager { get; }

    protected bool _isDead = false;
    private string _name = null!;
    private IRoom? _room = null!;
    private int _hitPoints = 0;

    public string Name => _name;
    public int HitPoints => _hitPoints;
    public IRoom? Room => _room;

    public CharacterBase(ICombatManager combatManager, string name, int hitPoints)
    {
        CombatManager = combatManager;

        _name = name;
        _hitPoints = hitPoints;
    }

    public void SetHitPoints(int hitPoints)
    {
        _hitPoints = hitPoints;
    }

    public void SetRoom(IRoom? room)
    {
        _room = room;
    }

    //
    public bool IsDead => _isDead;

    public void FlagAsDead()
    {
        _isDead = true;
    }

    public void Recompute()
    {
    }

    // to simulate combat behavior
    public int Initiative { get; set; }
    public int AutoAttackCount { get; set; }
    public int AutoAttachDamage { get; set; }
    public bool IsCounterAttackActive { get; set; }
    public int CounterAttackDamage { get; set; }
}

public interface ICharacter : IEntity
{
    string Name { get; }
    int HitPoints { get; }
    IRoom? Room { get; }

    void SetHitPoints(int hitPoints);
    void SetRoom(IRoom? room);

    bool IsDead { get; }
    void FlagAsDead();

    void Recompute();

    //
    int Initiative { get; }
    int AutoAttackCount { get; }
    public int AutoAttachDamage { get; }
    bool IsCounterAttackActive { get; }
    int CounterAttackDamage { get; }
}

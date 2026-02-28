namespace Mud.POC.DeferredAction;

public abstract class Skill
{
    public string Name { get; }
    public int ManaCost { get; }
    private readonly int _cooldownTicks;
    private readonly Dictionary<Mob, int> _lastUsedTick = new();

    public DamageType DamageType { get; } // TODO should be optional

    protected Skill(string name, int manaCost, int cooldownTicks = 0)
    {
        Name = name;
        ManaCost = manaCost;
        _cooldownTicks = cooldownTicks;
    }

    public bool IsOnCooldown(Mob caster, World world)
        => _lastUsedTick.TryGetValue(caster, out var tick) && world.CurrentTick - tick < _cooldownTicks;

    public void Use(Mob caster, Mob target, World world)
    {
        if (caster.Mana < ManaCost) return;
        _lastUsedTick[caster] = world.CurrentTick;
        caster.Mana -= ManaCost;
        Execute(caster, target, world);
    }

    public abstract void Execute(Mob caster, Mob target, World world);
}

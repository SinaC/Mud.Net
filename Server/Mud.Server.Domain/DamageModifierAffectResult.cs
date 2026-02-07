namespace Mud.Server.Domain;

public class DamageModifierAffectResult
{
    public int ModifiedDamage { get; set; }
    public bool WornOff { get; set; }
    public DamageModifierAffectAction Action { get; set; }
}

public enum DamageModifierAffectAction
{
    Nop,
    DamageIncreased,
    DamageDecreased,
    DamagePartiallyAbsorbed,
    DamageFullyAbsorbed,
}

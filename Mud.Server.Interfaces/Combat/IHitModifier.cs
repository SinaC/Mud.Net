using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Combat;

public interface IHitModifier
{
    string AbilityName { get; }
    string DamageNoun { get; }
    int Learned { get; }
    int Thac0Modifier(int baseThac0);
    int DamageModifier(IItemWeapon? weapon, int level, int baseDamage);
}
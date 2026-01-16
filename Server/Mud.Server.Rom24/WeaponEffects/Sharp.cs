using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Random;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Sharp")]
public class Sharp : IDamageModifierWeaponEffect
{
    private IRandomManager RandomManager { get; }

    public Sharp(IRandomManager randomManager)
    {
        RandomManager = randomManager;
    }

    public int DamageModifier(ICharacter holder, ICharacter victim, IItemWeapon weapon, int learned, int baseDamage)
    {
        int percent = RandomManager.Range(1, 100);
        if (percent <= learned / 8)
            return baseDamage + (2 * baseDamage * percent / 100);
        return 0;
    }
}

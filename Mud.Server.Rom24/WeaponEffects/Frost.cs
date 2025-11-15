using Mud.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Frost")]
public class Frost : IPostHitDamageWeaponEffect
{
    private IRandomManager RandomManager { get; }
    private IEffectManager EffectManager { get; }

    public Frost(IRandomManager randomManager, IEffectManager effectManager)
    {
        RandomManager = randomManager;
        EffectManager = effectManager;
    }

    public bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon)
    {
        int damage = RandomManager.Range(1, 2 + weapon.Level / 6);
        victim.Act(ActOptions.ToRoom, "{0} freezes {1}.", weapon, victim);
        victim.Act(ActOptions.ToCharacter, "The cold touch of {0} surrounds you with ice.", weapon);
        victim.Damage(holder, damage, SchoolTypes.Cold, null, false);
        var coldEffect = EffectManager.CreateInstance<ICharacter>("Cold");
        coldEffect?.Apply(victim, holder, "Chill touch", weapon.Level / 2, damage);
        return true;
    }
}

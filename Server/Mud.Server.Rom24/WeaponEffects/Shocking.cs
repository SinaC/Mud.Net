using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Shocking")]
public class Shocking : IPostHitDamageWeaponEffect
{
    private IRandomManager RandomManager { get; }
    private IEffectManager EffectManager { get; }

    public Shocking(IRandomManager randomManager, IEffectManager effectManager)
    {
        RandomManager = randomManager;
        EffectManager = effectManager;
    }

    public bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon)
    {
        int damage = RandomManager.Range(1, 2 + weapon.Level / 5);
        victim.Act(ActOptions.ToRoom, "{0:N} is struck by lightning from {1}.", victim, weapon);
        victim.Act(ActOptions.ToCharacter, "You are shocked by {0}.", weapon);
        victim.AbilityDamage(holder, damage, SchoolTypes.Lightning, null, false);
        var shockEffect = EffectManager.CreateInstance<ICharacter>("Shock");
        shockEffect?.Apply(victim, holder, "Shocking weapon", weapon.Level / 2, damage);
        return true;
    }
}

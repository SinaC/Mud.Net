using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Vampiric")]
public class Vampiric : IPostHitDamageWeaponEffect
{
    private IRandomManager RandomManager { get; }

    public Vampiric(IRandomManager randomManager)
    {
        RandomManager = randomManager;
    }

    public bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon)
    {
        int damage = RandomManager.Range(1, 1 + weapon.Level / 5);
        victim.Act(ActOptions.ToRoom, "{0} draws life from {1}.", weapon, victim);
        victim.Act(ActOptions.ToCharacter, "You feel {0} drawing your life away.", weapon);
        victim.AbilityDamage(holder, damage, SchoolTypes.Negative, null, false);
        holder.Heal(holder, damage / 2);
        holder.UpdateAlignment(-1);
        return true;
    }
}

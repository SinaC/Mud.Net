using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Effects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Poison")]
public class Poison : IPostHitDamageWeaponEffect
{
    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }

    public Poison(IAuraManager auraManager, IAffectManager affectManager)
    {
        AuraManager = auraManager;
        AffectManager = affectManager;
    }

    public bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon)
    {
        var wieldPoisonAura = weapon.Auras.FirstOrDefault(x => x.Affects.OfType<IItemWeaponFlagsAffect>().Any(aff => aff.Modifier.IsSet("Poison")));
        int level = wieldPoisonAura?.Level ?? weapon.Level;
        if (!victim.SavesSpell(level / 2, SchoolTypes.Poison))
        {
            victim.Send("You feel poison coursing through your veins.");
            victim.Act(ActOptions.ToRoom, "{0:N} is poisoned by the venom on {1}.", victim, weapon);
            int duration = Math.Min(1, level / 2);

            var victimPoisonAura = victim.Auras.FirstOrDefault(x => x.Affects.OfType<ICharacterFlagsAffect>().Any(aff => aff.Modifier.IsSet("Poison")));
            if (victimPoisonAura == null)
            {
                var poisonAffect = AffectManager.CreateInstance("Poison");
                AuraManager.AddAura(victim, "Poison", holder, 3 * level / 4, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                    poisonAffect);
            }
            else
                victimPoisonAura.Update(3 * level / 4, TimeSpan.FromMinutes(duration));

            // weaken the poison if it's temporary 
            if (wieldPoisonAura != null && !wieldPoisonAura.AuraFlags.HasFlag(AuraFlags.Permanent))
            {
                wieldPoisonAura.DecreaseLevel();
                bool wornOff = wieldPoisonAura.DecreasePulseLeft(1);
                if (wieldPoisonAura.Level <= 1 || wornOff)
                {
                    weapon.RemoveAura(wieldPoisonAura, true);
                    holder.Act(ActOptions.ToCharacter, "The %G%poison%x% on {0} has worn off.", weapon);
                }
            }
            return true;
        }
        return false;
    }
}

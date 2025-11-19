using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Vorpal")]
public class Vorpal : IInstantDeathWeaponEffect
{
    private ILogger Logger { get; }
    private IRandomManager RandomManager { get; }

    public Vorpal(ILogger logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    public bool Trigger(ICharacter holder, ICharacter victim, IItemWeapon weapon, SchoolTypes damageType)
    {
        // vorpal -> kill in one hit !
        // immune: no chance
        // resistant: 0.1%
        // normal: 0.2%
        // vulnerable: 0.5%
        // calculate weapon (or not) damage
        if (victim.BodyParts.IsSet("Head")
            && !(victim is IPlayableCharacter pcVictim && pcVictim.IsImmortal))
        {
            int chance;
            ResistanceLevels resistanceLevel = victim.CheckResistance(damageType);
            switch (resistanceLevel)
            {
                case ResistanceLevels.Immune:
                    chance = 0;
                    break;
                case ResistanceLevels.Resistant:
                    chance = 1;
                    break;
                case ResistanceLevels.Vulnerable:
                    chance = 5;
                    break;
                case ResistanceLevels.Normal:
                    chance = 2;
                    break;
                default:
                    chance = 0;
                    break;
            }
            if (chance > 0 && RandomManager.Range(0, 999) < chance)
            {
                Logger.LogDebug("Vorpal: who {victim} what {weapon} by {effect}", victim, weapon, this);
                holder.ActToNotVictim(victim, "The %M%vorpal%x% of {0} strikes {1}.", weapon, victim);
                holder.Act(ActOptions.ToCharacter, "The %M%vorpal%x% of your weapon strikes {0}.", victim);
                victim.Act(ActOptions.ToCharacter, "The %M%Vorpal%x% of {0} strikes and KILLS you.", weapon);
                holder.Act(ActOptions.ToRoom, "{0:N} is dead!!", victim);

                // TODO: beheaded ?

                return true; // instant-death will be handled by caller
            }
        }
        return false;
    }
}

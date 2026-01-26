using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.WeaponEffects;

[WeaponEffect("Vorpal")]
public class Vorpal : IInstantDeathWeaponEffect
{
    private ILogger<Vorpal> Logger { get; }
    private IRandomManager RandomManager { get; }
    private IResistanceCalculator ResistanceCalculator { get; }

    public Vorpal(ILogger<Vorpal> logger, IRandomManager randomManager, IResistanceCalculator resistanceCalculator)
    {
        Logger = logger;
        RandomManager = randomManager;
        ResistanceCalculator = resistanceCalculator;
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
            && !victim.ImmortalMode.IsSet("NoDeath"))
        {
            var resistanceLevel = ResistanceCalculator.CheckResistance(victim, damageType);
            var chance = resistanceLevel switch
            {
                ResistanceLevels.Immune => 0,
                ResistanceLevels.Resistant => 1,
                ResistanceLevels.Vulnerable => 5,
                ResistanceLevels.Normal => 2,
                _ => 0,
            };
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

using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("trip", "Ability", "Skill", "Combat")]
[Syntax("[cmd] <victim>")]
[Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 24)]
[Help(
@"Back by popular demand.  Trip is a somewhat dastardly attack, and involves
using any one of a number of methods to bring your opponent down to the ground.
Tripping large monsters is generally not a good idea, and agile ones will
find the attack easy to avoid.  Thieves and warriors may learn trip.")]
[OneLineHelp("a good way to introduce an opponent to the floor")]
public class Trip : OffensiveSkillBase
{
    private const string SkillName = "Trip";

    public Trip(ILogger<Trip> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        var npcUser = User as INonPlayableCharacter;
        if (Learned == 0
            || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Trip")))
            return "Tripping?  What's that?";

        if (Victim == User)
        {
            User.Act(ActOptions.ToRoom, "%W%{0:N} trips over {0:s} own feet!%x%", User);
            return "%W%You fall flat on your face!%x%";
        }

        var safeResult = Victim.IsSafe(User);
        if (safeResult != null)
            return safeResult;

        // TODO: check kill stealing

        if (User.CharacterFlags.IsSet("Charm") && npcUser?.Master == Victim)
            return User.ActPhrase("But {0:N} is your friend!", Victim);

        if (Victim.CharacterFlags.IsSet("Flying"))
            return User.ActPhrase("{0:s} feet aren't on the ground.", Victim);

        if (Victim.Position < Positions.Standing)
            return User.ActPhrase("{0:N} is already down..", Victim);

        return null;
    }

    protected override bool Invoke()
    {
        int chance = Learned;
        // modifiers
        if (User.Size < Victim.Size)
            chance -= (Victim.Size - User.Size) * 10; // bigger = harder to trip
        // dexterity
        chance += User[CharacterAttributes.Dexterity];
        chance -= (3 * Victim[CharacterAttributes.Dexterity]) / 2;
        // speed
        if ((User as INonPlayableCharacter)?.OffensiveFlags.IsSet("Fast") == true || User.CharacterFlags.IsSet("Haste"))
            chance += 10;
        if ((Victim as INonPlayableCharacter)?.OffensiveFlags.IsSet("Fast") == true || Victim.CharacterFlags.IsSet("Haste"))
            chance -= 20;
        // level
        chance += (User.Level - Victim.Level) * 2;

        // now the attack
        if (RandomManager.Chance(chance))
        {
            Victim.Act(ActOptions.ToCharacter, "%W%{0:N} trips you and you go down!%x%", User);
            User.Act(ActOptions.ToCharacter, "%W%You trip {0} and {0} goes down!%x%", Victim);
            User.ActToNotVictim(Victim, "%W%{0} trips {1}, sending {1:m} to the ground.%x%", User, Victim);
            Victim.SetDaze(2 * Pulse.PulseViolence);
            Victim.ChangePosition(Positions.Sitting);
            // TODO: check_killer(ch, Victim)
            int damage = RandomManager.Range(2, 2 + 2 * (int)User.Size);
            Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "trip", true);
            return true;
        }
        else
        {
            //Victim.AbilityDamage(User, 0, SchoolTypes.Bash, "trip", true);
            // TODO check_killer(ch,Victim);
            return false;
        }
    }
}

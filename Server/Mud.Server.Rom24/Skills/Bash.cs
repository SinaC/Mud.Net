using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("bash", "Ability", "Skill", "Combat")]
[Syntax("[cmd] <victim>")]
[Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 20)]
[Help(
@"Bash is a brute-force attack designed to knock your foe to his or her
knees.  Its success depends on many factors, including the bash rating, your
weight, and the size of your opponent.  Bashing a dragon is not generally a
wise idea.  Bashing has a small percentage chance to knock an item off of
your opponent.")]
[OneLineHelp("a forceful rush with the body, designed to flatten your foes")]
public class Bash : OffensiveSkillBase
{
    private const string SkillName = "Bash";

    public Bash(ILogger<Bash> logger, IRandomManager randomManager)
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
            || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Bash")))
            return "Bashing? What's that?";

        if (Victim == User)
            return "You try to bash your brains out, but fail.";

        var safeResult = Victim.IsSafe(User);
        if (safeResult != null)
            return safeResult;

        if (Victim.Position < Positions.Standing)
            return User.ActPhrase("You'll have to let {0:m} get back up first.", Victim);

        if (User.Position < Positions.Standing)
            return "It's hard to bash when you're not standing up!";

        // TODO: check kill stealing

        if (User.CharacterFlags.IsSet("Charm") && npcUser?.Master == Victim)
            return User.ActPhrase("But {0:N} is your friend!", Victim);

        return null;
    }

    protected override bool Invoke()
    {
        int chance = Learned;

        // modifiers
        // size and weight
        chance += User.CarryWeight / 250;
        chance -= Victim.CarryWeight / 250;
        if (User.Size < Victim.Size)
            chance -= (Victim.Size - User.Size) * 15; // big drawback to bash someone bigger
        else
            chance += (User.Size - Victim.Size) * 10; // big advantage to bash someone smaller
        // stats
        chance += User[CharacterAttributes.Strength];
        chance -= (4 * Victim[CharacterAttributes.Dexterity]) / 3;
        chance -= Victim[CharacterAttributes.ArmorBash] / 25;
        // speed
        if (User.CharacterFlags.IsSet("Haste")) // no need to test OFF_FAST because OFF_FAST adds haste effect
            chance += 10;
        if (Victim.CharacterFlags.IsSet("Haste")) // no need to test OFF_FAST because OFF_FAST adds haste effect
            chance -= 30;
        // level
        chance += User.Level - Victim.Level;

        // dodge?
        var (percentage, _) = Victim.GetAbilityLearnedAndPercentage("Dodge");
        if (chance < percentage)
            chance -= 3 * (percentage - chance);

        // now the attack
        if (RandomManager.Chance(chance))
        {
            User.Act(ActOptions.ToCharacter, "%W%You slam into {0}, and send {0:m} flying!%x%", Victim);
            User.Act(ActOptions.ToRoom, "%W%{0:N} sends {1} sprawling with a powerful bash%x%.", User, Victim);
            Victim.SetDaze(3 * Pulse.PulseViolence);
            Victim.ChangePosition(Positions.Sitting);
            int damage = RandomManager.Range(2, 2 + 2 * (int)User.Size + chance / 20);
            Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "bash", false);
            // TODO: check_killer(ch,Victim);
            return true;
        }
        //Victim.AbilityDamage(User, 0, SchoolTypes.Bash, "bash", false); // starts a fight
        User.Send("%W%You fall flat on your face!%x%");
        User.Act(ActOptions.ToRoom, "%W%{0:N} fall{0:v} flat on {0:s} face!%x%", User);
        Victim.Act(ActOptions.ToCharacter, "%W%You evade {0:p} bash, causing {0:m} to fall flat on {0:s} face.%x%", User);
        User.ChangePosition(Positions.Sitting);
        // TODO: check_killer(ch,Victim);
        return false;
    }
}

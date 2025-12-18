using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("disarm", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24)]
[Help(
@"Disarm is a somewhat showy and unreliable skill, designed to relieve your
opponent of his weapon.  The best possible chance of disarming occurs when you
are skilled both your own and your opponent's weapon.")]
[OneLineHelp("used to deprive your opponent of his weapon")]
public class Disarm : FightingSkillBase
{
    private const string SkillName = "Disarm";

    public Disarm(ILogger<Disarm> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected int Hand2HandLearned { get; set; }
    protected IItemWeapon UserWield { get; set; } = default!;
    protected IItemWeapon VictimWield { get; set; } = default!;

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Learned == 0)
            return "You don't know how to disarm opponents.";

        var npcUser = User as INonPlayableCharacter;

        // User has a weapon or hand to hand
        
        var userWield = User.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
        Hand2HandLearned = User.GetAbilityLearnedAndPercentage("Hand to Hand").percentage;
        if (userWield == null
            && (Hand2HandLearned == 0
                || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Disarm"))))
            return "You must wield a weapon to disarm.";
        UserWield = userWield!;

        // Victim wield
        var victimWield = Victim.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
        if (victimWield == null)
            return "Your opponent is not wielding a weapon.";
        VictimWield = victimWield!;

        return null;
    }

    protected override bool Invoke()
    {
        // find weapon learned
        int userLearned = User.GetWeaponLearnedAndPercentage(UserWield).percentage;
        int victimLearned = Victim.GetWeaponLearnedAndPercentage(UserWield).percentage;
        int userOnVictimWeaponLearned = User.GetWeaponLearnedAndPercentage(VictimWield).percentage;

        int chance = Learned;
        // modifiers
        // skill
        if (UserWield == null)
            chance = (chance * Hand2HandLearned) / 150;
        else
            chance = chance * userLearned / 100;
        chance += (userOnVictimWeaponLearned / 2 - victimLearned) / 2;
        // dex vs. strength
        chance += User[CharacterAttributes.Dexterity];
        chance -= 2 * Victim[CharacterAttributes.Strength];
        // level
        chance += (User.Level - Victim.Level) * 2;
        // and now the attack
        if (RandomManager.Chance(chance))
        {
            if (VictimWield.ItemFlags.IsSet("NoRemove"))
            {
                User.Act(ActOptions.ToCharacter, "%W%{0:S} weapon won't budge!%x%", Victim);
                Victim.Act(ActOptions.ToCharacter, "%W%{0:N} tries to disarm you, but your weapon won't budge!%x%", User);
                Victim.Act(ActOptions.ToRoom, "%W%{0} tries to disarm {1}, but fails.%x%", User, Victim); // equivalent of NO_NOTVICT
            }

            Victim.Act(ActOptions.ToCharacter, "%W%{0:N} DISARMS you and sends your weapon flying!%x%", User);
            Victim.Act(ActOptions.ToRoom, "%W%{0:N} disarm{0:v} {1}%x%", User, Victim);

            VictimWield.ChangeEquippedBy(null, true);
            if (!VictimWield.ItemFlags.HasAny("NoDrop", "Inventory"))
            {
                VictimWield.ChangeContainer(Victim.Room);
                // TODO: NPC tries to get its weapon back
                if (Victim is INonPlayableCharacter && Victim.CanSee(VictimWield) && Victim.GlobalCooldown == 0)
                    Victim.GetItem(VictimWield, Victim.Room);
            }

            // TODO  check_killer(ch, Victim);
            return true;
        }
        else
        {
            User.Act(ActOptions.ToCharacter, "%W%You fail to disarm {0}.%x%", Victim);
            User.Act(ActOptions.ToRoom, "%W%{0:N} tries to disarm {1}, but fails.%x%", User, Victim);
            // TODO  check_killer(ch, Victim);
            return false;
        }
    }
}

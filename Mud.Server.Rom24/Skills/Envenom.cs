using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("envenom", "Ability", "Skill", "Enchantment")]
[Syntax(
        "[cmd] <weapon>",
        "[cmd] <food>",
        "[cmd] <drink container>")]
[Skill(SkillName, AbilityEffects.Enchantment, PulseWaitTime = 36, LearnDifficultyMultiplier = 4)]
[AbilityItemWearOffMessage("The poison on {0} dries up.")]
[Help(
@"The envenom skill is a cowardly skill practiced only by thieves, designed to
win a battle through alchemy and treachery rather than skill or strength.
Or, put another way, it's a skill used by the smart to kill the foolish.
Food, drink, and weapons may be envenomed, with varying effects. Poisoned
food or drink puts a mild poison spell on the consumer, and is unlikely to
be more than a minor inconvience (after all, the typical adventurer could
drink sewer water with only a trace of the runs).  A poisoned weapon, on
the other hand, can inflict serious damage on an opponent as the poison 
burns through his bloodstream.  But be careful, blade venom evaporates 
quickly and is rendered almost powerless by repeated blows in combat.")]
public class Envenom : ItemInventorySkillBase
{
    private const string SkillName = "Envenom";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public Envenom(ILogger<Envenom> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Learned < 1)
            return "Are you crazy? You'd poison yourself!";

        if (Item is IItemPoisonable poisonable)
        {
            if (poisonable.ItemFlags.HasAny("Bless", "BurnProof"))
                return User.ActPhrase("You fail to poison {0}.", poisonable);
            // poisonable found
            return null;
        }

        if (Item is IItemWeapon weapon)
        {
            if (weapon.DamageType == SchoolTypes.Bash)
                return "You can only envenom edged weapons.";
            if (weapon.WeaponFlags.IsSet("Poison"))
                User.ActPhrase("{0} is already envenomed.", weapon);
            if (!weapon.WeaponFlags.IsNone
                || weapon.ItemFlags.HasAny("Bless", "BurnProof"))
                User.ActPhrase("You can't seem to envenom {0}.", weapon);
            // weapon found
            return null;
        }

        return User.ActPhrase("You can't poison {0}.", Item);
    }

    protected override bool Invoke()
    {
        if (Item is IItemPoisonable poisonable)
            return Invoke(poisonable);
        if (Item is IItemWeapon weapon)
            return Invoke(weapon);
        return false;
    }

    private bool Invoke(IItemPoisonable poisonable)
    {
        if (RandomManager.Chance(Learned))
        {
            User.Act(ActOptions.ToAll, "{0:N} treats {1} with deadly poison.", User, poisonable);
            poisonable.Poison();
            poisonable.Recompute();
            return true;
        }
        User.Act(ActOptions.ToCharacter, "You fail to poison {0}.", poisonable);
        return false;
    }

    private bool Invoke(IItemWeapon weapon)
    {
        int percent = RandomManager.Range(1, 100);
        if (RandomManager.Chance(percent))
        {
            int level = (User.Level * percent) / 100;
            int duration = (User.Level * percent) / (2 * 100);
            AuraManager.AddAura(weapon, SkillName, User, level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags(ServiceProvider, "Poison"), Operator = AffectOperators.Or });
            User.Act(ActOptions.ToAll, "{0:N} coat{0:v} {1} with deadly venom.", User, weapon);
            return true;
        }
        User.Act(ActOptions.ToCharacter, "You fail to envenom {0}.", weapon);
        return false;
    }
}

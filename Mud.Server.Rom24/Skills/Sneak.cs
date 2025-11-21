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
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("sneak", "Ability", "Skill")]
[Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
[Help(
@"Hide and sneak are similar skills, both related to remaining undetected.
Hide has a very high chance of success, but only works for as long as the
character remains stationary.  Sneak may be used when moving (including to
sneak by monsters), but has a lower chance of success.  Typing hide or sneak
a second time will cancel them.  Hide has the added benefit of increasing
the chance of a backstab hitting your opponent.")]
public class Sneak : NoTargetSkillBase
{
    private const string SkillName = "Sneak";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public Sneak(ILogger<Sneak> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
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

        User.Send("You attempt to move silently.");
        User.RemoveAuras(x => x.AbilityName == SkillName, true);

        if (User.CharacterFlags.IsSet("Sneak"))
            return string.Empty;

        return null;
    }

    protected override bool Invoke()
    {
        if (RandomManager.Chance(Learned))
        {
            AuraManager.AddAura(User, SkillName, User, User.Level, TimeSpan.FromMinutes(User.Level), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Sneak"), Operator = AffectOperators.Or });
            return true;
        }

        return false;
    }
}

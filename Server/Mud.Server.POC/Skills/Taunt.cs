using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;

namespace Mud.Server.POC.Skills;

[CharacterCommand("taunt", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.None, CooldownInSeconds = 8)]
[Syntax(
    "[cmd]",
    "[cmd] <target>")]
[Help(
@"Taunts the target to attack you")]
public class Taunt : OffensiveSkillBase
{
    private const string SkillName = "Taunt";

    private IAuraManager AuraManager { get; }
    private IAggroManager AggroManager { get; }

    public Taunt(ILogger<Maul> logger, IRandomManager randomManager, IAuraManager auraManager, IAggroManager aggroManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        AggroManager = aggroManager;
    }

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        var baseSetTargets = base.SetTargets(skillActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        if (User == Victim)
            return "What about fleeing instead?";

        var fighting = Victim.Fighting;
        if (fighting == null)
            return User.ActPhrase("{0:N} is not fighting right now.", Victim);

        if (fighting is INonPlayableCharacter && User.IsSameGroupOrPet(Victim))
            return "Kill stealing is not permitted.";

        return null;
    }

    protected override bool Invoke()
    {
        var tauntAura = User.GetAura(SkillName);
        if (tauntAura != null)
            tauntAura.Update(tauntAura.Level, TimeSpan.FromSeconds(6));
        else
        {
            AuraManager.AddAura(User, SkillName, User, User.Level, TimeSpan.FromSeconds(6), new AuraFlags("NoSave"), true,
                new CharacterAggroModifierAffect { MultiplierInPercent = 800 });
        }

        AggroManager.OnTaunt(User, Victim);
        User.Act(ActOptions.ToAll, "%W%{0:N} taunts {1:N}.%x%", User, Victim);

        return true;
    }
}
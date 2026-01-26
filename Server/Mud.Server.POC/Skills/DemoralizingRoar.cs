using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("DemoralizingRoar", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage), Shapes([Shapes.Bear])]
[Help(
@"The druid roars, decreasing nearby enemies' melee attack power by 138.  Lasts 30 sec.")]
//https://www.wowhead.com/classic/spell=9898/demoralizing-roar
public class DemoralizingRoar : OffensiveSkillBase
{
    private const string SkillName = "Demoralizing Roar";

    private IAuraManager AuraManager { get; }

    public DemoralizingRoar(ILogger<DemoralizingRoar> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool Invoke()
    {
        var roarAura = Victim.GetAura(SkillName);
        if (roarAura == null)
        {
            AuraManager.AddAura(Victim, SkillName, User, User.Level, TimeSpan.FromSeconds(30), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = -User.Level, Operator = AffectOperators.Add });
        }
        else
            roarAura.Update(User.Level, TimeSpan.FromSeconds(9));
        Actor.Act(ActOptions.ToAll, "%W%{0:N} roar{0:v} demoralizing {1}.%x%", Actor, Victim);
        return true;
    }
}

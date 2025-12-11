using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("DemoralizingRoar", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[AbilityShape(Shapes.Bear)]
[Help(
@"The druid roars, decreasing nearby enemies' melee attack power by 138.  Lasts 30 sec.")]
//https://www.wowhead.com/classic/spell=9898/demoralizing-roar
public class DemoralizingRoar : OffensiveSkillBase
{
    private const string SkillName = "Demoralizing Roar";

    private IAuraManager AuraManager { get; }

    public DemoralizingRoar(ILogger<OffensiveSkillBase> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool Invoke()
    {
        if (RandomManager.Chance(Learned))
        {
            var roarAura = Victim.GetAura(SkillName);
            if (roarAura == null)
            {
                AuraManager.AddAura(Victim, SkillName, User, User.Level, TimeSpan.FromSeconds(30), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = -User.Level, Operator = AffectOperators.Add });
            }
            else
                roarAura.Update(User.Level, TimeSpan.FromSeconds(9));
            return true;
        }
        // TODO: should use another way to ensure a fight is started
        // Hassan is immune to mental and when starting a fight with Demoralizing Roar but failed
        // the message will be 'Hassan is immune to your misses'
        Victim.AbilityDamage(User, 0, SchoolTypes.Mental, "roar", true); // start a fight if needed
        return false;
    }
}

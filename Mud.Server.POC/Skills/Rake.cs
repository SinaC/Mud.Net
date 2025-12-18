using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("rake", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[AbilityShape(Shapes.Cat)]
[Help(
@"Rake the target for 58 damage and an additional 96 damage over 9 sec.  Awards 1 combo point.")]
//https://www.wowhead.com/classic/spell=9904/rake
public class Rake : OffensiveSkillBase
{
    private const string SkillName = "Rake";

    private IAuraManager AuraManager { get; }

    public Rake(ILogger<OffensiveSkillBase> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool Invoke()
    {
        var damage = RandomManager.Range(1, User.Level/2); // half-damage of rake
        Victim.AbilityDamage(User, damage, SchoolTypes.Pierce, "rake", true);
        // + dot
        var rakeAura = Victim.GetAura(SkillName);
        if (rakeAura == null)
        {
            AuraManager.AddAura(Victim, SkillName, User, User.Level, TimeSpan.FromSeconds(9), AuraFlags.None, true,
                new RakeAffect()); // should be 96 damage over 9 seconds
        }
        else
            rakeAura.Update(User.Level, TimeSpan.FromSeconds(9));
        //check_killer(ch,victim);
        User.UpdateResource(ResourceKinds.Combo, 1);
        return true;
    }
}

using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;
using Mud.Server.POC.Affects;

namespace Mud.Server.POC.Skills;

[CharacterCommand("rake", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[Help(
@"Rake the target for 58 damage and an additional 96 damage over 9 sec.  Awards 1 combo point.")]
//https://www.wowhead.com/classic/spell=9904/rake
public class Rake : OffensiveSkillBase
{
    private const string SkillName = "Rake";

    protected override IGuard<ICharacter>[] Guards => [new RequiresShapesGuard([Shapes.Cat])];

    private IAuraManager AuraManager { get; }

    public Rake(ILogger<Rake> logger, IRandomManager randomManager, IAuraManager auraManager)
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
            AuraManager.AddAura(Victim, SkillName, User, User.Level, TimeSpan.FromSeconds(9), new AuraFlags(), true,
                new RakeAffect()); // should be 96 damage over 9 seconds
        }
        else
            rakeAura.Update(User.Level, TimeSpan.FromSeconds(9));
        //check_killer(ch,victim);
        User.UpdateResource(ResourceKinds.Combo, 1);
        return true;
    }
}

using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Skills;

[CharacterCommand("catform", "Ability", "Skill", "Shapeshift", "POC")]
[Skill(SkillName, AbilityEffects.Buff, PulseWaitTime = 20)]
[Help(
@"Shapeshift into Cat Form, increasing auto-attack damage by 40%, movement speed by 30%, granting protection from Polymorph effects, and reducing falling damage.")]
public class CatForm : NoTargetSkillBase
{
    private const string SkillName = "Cat Form";

    private IAuraManager AuraManager { get; }

    public CatForm(ILogger<CatForm> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool MustBeLearned => true;

    protected override bool Invoke()
    {
        if (User.Shape == Shapes.Cat)
        {
            User.ChangeShape(Shapes.Normal);
            return false;
        }

        // TODO: better wording
        User.Send("%W%You shapeshift into a cat.%x%");
        User.Act(ActOptions.ToRoom, "%W%{0:N} shapeshifts into a cat.%x%", this);
        User.ChangeShape(Shapes.Cat);
        // set energy: current=0, max=100
        User.SetBaseMaxResource(ResourceKinds.Energy, 100);
        User.SetResource(ResourceKinds.Energy, 0);
        // set combo: current=0, max=5
        User.SetBaseMaxResource(ResourceKinds.Combo, 5);
        User.SetResource(ResourceKinds.Combo, 0);

        // TODO: Affect changing Form + disable other form
        AuraManager.AddAura(User, SkillName, User, User.Level, new AuraFlags("NoDispel", "Permanent", "Shapeshift"), true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = User.Level * 4, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Haste", "Infrared"), Operator = AffectOperators.Add });

        return true;
    }
}

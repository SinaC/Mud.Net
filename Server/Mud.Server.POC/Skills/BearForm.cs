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

[CharacterCommand("bearform", "Ability", "Skill", "Shapeshift", "POC")]
[Skill(SkillName, AbilityEffects.Buff, PulseWaitTime = 20)]
[Help(
@"Shapeshift into Bear Form, increasing armor by 220% and Stamina by 25%, granting protection from Polymorph effects, and increasing threat generation.")]
public class BearForm : NoTargetSkillBase
{
    private const string SkillName = "Bear Form";

    private IAuraManager AuraManager { get; }

    public BearForm(ILogger<BearForm> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool MustBeLearned => true;

    protected override bool Invoke()
    {
        if (User.Shape == Shapes.Bear)
        {
            User.ChangeShape(Shapes.Normal);
            return false;
        }

        // TODO: better wording
        User.Send("%W%You shapeshift into a bear.%x%");
        User.Act(ActOptions.ToRoom, "%W%{0:N} shapeshifts into a bear.%x%", this);
        User.ChangeShape(Shapes.Bear);
        // set rage: current 100= max=100
        User.SetBaseMaxResource(ResourceKinds.Rage, 100);
        User.UpdateResource(ResourceKinds.Rage, 100); // TODO: should be 0

        // TODO: Affect changing Form + disable other form
        AuraManager.AddAura(User, SkillName, User, User.Level, new AuraFlags("NoDispel", "Permanent", "Shapeshift"), true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -User.Level * 5, Operator = AffectOperators.Add },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Constitution, Modifier = User.Level / 10, Operator = AffectOperators.Add },
            new CharacterResourceAffect { Location = ResourceKinds.HitPoints, Modifier = User.Level * 5, Operator = AffectOperators.Add },
            new CharacterIRVAffect { Location = IRVAffectLocations.Resistances, Modifier = new IRVFlags("Cold", "Bash"), Operator = AffectOperators.Add },
            new CharacterSizeAffect { Value = Sizes.Large });

        return true;
    }
}

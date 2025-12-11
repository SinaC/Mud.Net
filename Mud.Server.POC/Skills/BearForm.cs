using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

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
            User.Send("You are already in bear form.");
            return false;
        }

        User.Send("You shapeshift into a bear.");
        User.Act(ActOptions.ToRoom, "{0:N} shapeshifts into a bear.", this);
        User.ChangeShape(Shapes.Bear);
        User.SetMaxResource(ResourceKinds.Rage, 100);
        User.UpdateResource(ResourceKinds.Rage, 100); // TODO: should be 0

        // TODO: Affect changing Form
        AuraManager.AddAura(User, SkillName, User, User.Level, AuraFlags.NoDispel | AuraFlags.Permanent, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -User.Level * 20, Operator = AffectOperators.Add },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Constitution, Modifier = Math.Min(1, User.Level / 10), Operator = AffectOperators.Add });

        return true;
    }
}

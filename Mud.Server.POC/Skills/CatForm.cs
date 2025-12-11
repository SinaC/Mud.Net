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
            User.Send("You are already in cat form.");
            return false;
        }

        User.Send("You shapeshift into a cat.");
        User.Act(ActOptions.ToRoom, "{0:N} shapeshifts into a cat.", this);
        User.ChangeShape(Shapes.Cat);
        // set energy
        User.SetMaxResource(ResourceKinds.Energy, 100);
        User.SetResource(ResourceKinds.Energy, 100);
        // set combo
        User.SetMaxResource(ResourceKinds.Combo, 5);
        User.SetResource(ResourceKinds.Energy, 0);

        // TODO: Affect changing Form
        AuraManager.AddAura(User, SkillName, User, User.Level, AuraFlags.NoDispel | AuraFlags.Permanent, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = User.Level * 4, Operator = AffectOperators.Add });

        return true;
    }
}

using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills
{
    [CharacterCommand("catform", "Ability", "Skill", "Shapeshift")]
    [Syntax("[cmd] <victim>")]
    [Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 20)]
    [Help(
@"Shapeshift into Cat Form, increasing auto-attack damage by 40%, movement speed by 30%, granting protection from Polymorph effects, and reducing falling damage.")]
    public class CatForm : NoTargetSkillBase
    {
        private const string SkillName = "Cat Form";

        public CatForm(ILogger<CatForm> logger, IRandomManager randomManager)
        : base(logger, randomManager)
        {
        }

        protected override bool Invoke()
        {
            if (User.Form == Domain.Forms.Cat)
            {
                User.Send("You are already in cat form.");
                return false;
            }

            User.Send("You shapeshift into a cat.");
            User.Act(ActOptions.ToRoom, "{0:N} shapeshifts into a cat.", this);
            User.ChangeForm(Domain.Forms.Cat);

            return true;
        }
    }
}

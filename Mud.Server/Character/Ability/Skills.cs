using System;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Character.Ability
{
    [CharacterCommand("skills", "Ability")]
    [Syntax(
        "[cmd]",
        "[cmd] all")]
    public class Skills : AbilitiesCharacterGameActionBase
    {
        public override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Skill || x == AbilityTypes.Passive;
        public override string Title => "Skills";
    }
}

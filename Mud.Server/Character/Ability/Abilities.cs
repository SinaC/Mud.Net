using System;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Character.Ability
{
    [CharacterCommand("abilities", "Ability")]
    [Syntax(
        "[cmd]",
        "[cmd] all")]
    public class Abilities : AbilitiesCharacterGameActionBase
    {
        public override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => true;
        public override string Title => "Abilities";
    }
}

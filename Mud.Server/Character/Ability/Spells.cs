using System;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Character.Ability
{
    [CharacterCommand("spells", "Ability")]
    [Syntax(
        "[cmd]",
        "[cmd] all")]
    public class Spells : AbilitiesCharacterGameActionBase
    {
        public override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Spell;
        public override string Title => "Spells";
    }
}

using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpremember", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile to remember a target. The target can be referred to
with $q and $Q codes in MOBprograms. See also 'mob forget'.")]
[Syntax("mob remember [victim]")]
public class Remember : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Remember(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindChararacterInWorld(CharacterManager, Actor, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.SetMobProgramTarget(Whom);
    }
}

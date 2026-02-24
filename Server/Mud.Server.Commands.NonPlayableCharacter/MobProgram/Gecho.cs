using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpgecho", "MobProgram", Hidden = true)]
[Help("Prints the argument to all active players in the game")]
[Syntax("mob gecho [string]")]
public class Gecho : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Gecho(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        // send message to every PC
        foreach (var pc in CharacterManager.PlayableCharacters)
            pc.Send(actionInput.RawParameters);
    }
}

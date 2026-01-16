using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("nofollow", "Group", "Movement")]
[Syntax("[cmd]")]
// TODO: help
public class NoFollow : CharacterGameAction
{
    private ICharacterManager CharacterManager { get; }

    public NoFollow(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach(var follower in CharacterManager.Characters.Where(x => x.Leader == Actor))
            Actor.RemoveFollower(follower);
    }
}

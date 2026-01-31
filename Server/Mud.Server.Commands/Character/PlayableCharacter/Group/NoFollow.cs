using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("nofollow", "Group", "Movement")]
[Syntax("[cmd]")]
// TODO: help
public class NoFollow : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

    private ICharacterManager CharacterManager { get; }

    public NoFollow(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var follower in CharacterManager.Characters.Where(x => x.Leader == Actor))
        {
            if (follower is INonPlayableCharacter pet && Actor.Pets.Contains(pet))
                Actor.RemovePet(pet);
            else
                Actor.RemoveFollower(follower);
        }
    }
}

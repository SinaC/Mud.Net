using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("nofollow", "Group", "Movement")]
    [Syntax("[cmd]")]
    public class NoFollow : CharacterGameAction
    {
        private ICharacterManager CharacterManager { get; }

        public NoFollow(ICharacterManager characterManager)
        {
            CharacterManager = characterManager;
        }

        public override void Execute(IActionInput actionInput)
        {
            foreach(ICharacter follower in CharacterManager.Characters.Where(x => x.Leader == Actor))
                Actor.RemoveFollower(follower);
        }
    }
}

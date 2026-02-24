using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mppeace", "MobProgram", Hidden = true)]
[Help("Stops every fight in the room.")]
[Syntax("mob peace")]
public class Peace : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        foreach (var character in Actor.Room.People)
        {
            character.StopFighting(true);
            // Needed ?
            //if (character is INonPlayableCharacter npc && npc.ActFlags.HasFlag(ActFlags.Aggressive))
            //    npc.Remove
        }
    }
}

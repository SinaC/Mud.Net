using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Special
{
    public interface ISpecialBehavior
    {
        bool Execute(INonPlayableCharacter npc);
    }
}

using Mud.Blueprints.MobProgram;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.MobProgram;

public interface IMobProgramProcessor
{
    void Execute(INonPlayableCharacter npc, MobProgramBase mobProgram, params object?[] parameters);
}

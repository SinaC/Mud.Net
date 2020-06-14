using Mud.Server.Blueprints.Character;

namespace Mud.Server
{
    public interface INonPlayableCharacter : ICharacter
    {
        CharacterBlueprintBase Blueprint { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);
    }
}
